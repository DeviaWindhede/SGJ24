using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class MeshCarver : MonoBehaviour
{
    [SerializeField] public GameObject carvingToolObject;

    private Vector3 toolPosition;
    private Vector3 toolRotation;
    private Vector3 toolTargetPosition;

    private CarvableMesh hitCarvable;
    private Mesh targetedMesh;
    private Vector3 localHitPoint;
    private Vector3 localAxis;
    private Vector3 worldHitPoint;

    private bool carving = false;

    private List<List<Vector3>> tVertList = new List<List<Vector3>>();
    private List<List<int>> tIndexList = new List<List<int>>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void MeshCalculate()
    {
        if (targetedMesh == null || hitCarvable == null) { return; }

        var targetVertices = targetedMesh.vertices;

        tVertList.Clear();
        tIndexList.Clear();

        for (int subMeshIDX = 0; subMeshIDX < targetedMesh.subMeshCount; subMeshIDX++)
        {
            tVertList.Add(new List<Vector3>());
            tIndexList.Add(new List<int>());

            List<Vector3> assembledVertices = new List<Vector3>();
            List<int> assembledIndices = new List<int>();

            var targetIndices = targetedMesh.GetIndices(subMeshIDX);

            for (int i = 0; i < targetIndices.Length; i += 3)
            {
                Vector3 v0Position = targetVertices[targetIndices[i + 0]];
                Vector3 v1Position = targetVertices[targetIndices[i + 1]];
                Vector3 v2Position = targetVertices[targetIndices[i + 2]];

                Vector3 v0World = hitCarvable.transform.TransformPoint(v0Position);
                Vector3 v1World = hitCarvable.transform.TransformPoint(v1Position);
                Vector3 v2World = hitCarvable.transform.TransformPoint(v2Position);

                Side v0Side = GetSide(v0World, worldHitPoint, Vector3.right);
                Side v1Side = GetSide(v1World, worldHitPoint, Vector3.right);
                Side v2Side = GetSide(v2World, worldHitPoint, Vector3.right);

                int rightSideCount = 0;
                if (v0Side == Side.Right) { rightSideCount++; }
                if (v1Side == Side.Right) { rightSideCount++; }
                if (v2Side == Side.Right) { rightSideCount++; }

                if (rightSideCount == 0)
                {
                    assembledVertices.Add(v0Position);
                    assembledVertices.Add(v1Position);
                    assembledVertices.Add(v2Position);
                    assembledIndices.Add(assembledVertices.Count - 3);
                    assembledIndices.Add(assembledVertices.Count - 2);
                    assembledIndices.Add(assembledVertices.Count - 1);
                }
                else if (rightSideCount == 1)
                {
                    Vector3 rightSideVert0 = new Vector3();
                    Vector3 leftSideVert0 = new Vector3();
                    Vector3 leftSideVert1 = new Vector3();
                    if (v0Side == Side.Right)
                    {
                        rightSideVert0 = v0World;
                        leftSideVert0 = v1World;
                        leftSideVert1 = v2World;
                    }
                    else if (v1Side == Side.Right)
                    {
                        rightSideVert0 = v1World;
                        leftSideVert0 = v0World;
                        leftSideVert1 = v2World;
                    }
                    else// if (v2Side == Side.Right)
                    {
                        rightSideVert0 = v2World;
                        leftSideVert0 = v0World;
                        leftSideVert1 = v1World;
                    }

                    Vector3 alignedRightSidePoint = AlignPointToPlane(rightSideVert0, leftSideVert1, Vector3.right, worldHitPoint);

                    Gizmos.color = Color.black;

                    //DrawTriangle(alignedRightSidePoint, leftSideVert0, leftSideVert1);

                    assembledVertices.Add(hitCarvable.transform.InverseTransformPoint(alignedRightSidePoint));
                    assembledVertices.Add(hitCarvable.transform.InverseTransformPoint(leftSideVert0));
                    assembledVertices.Add(hitCarvable.transform.InverseTransformPoint(leftSideVert1));
                    assembledIndices.Add(assembledVertices.Count - 3);
                    assembledIndices.Add(assembledVertices.Count - 2);
                    assembledIndices.Add(assembledVertices.Count - 1);

                }
                else if (rightSideCount == 2)
                {
                    Vector3 rightSideVert0 = new Vector3();
                    Vector3 rightSideVert1 = new Vector3();
                    Vector3 leftSideVert0 = new Vector3();
                    if (v0Side != Side.Right)
                    {
                        rightSideVert0 = v1World;
                        rightSideVert1 = v2World;
                        leftSideVert0 = v0World;
                    }
                    else if (v1Side != Side.Right)
                    {
                        rightSideVert0 = v0World;
                        rightSideVert1 = v2World;
                        leftSideVert0 = v1World;
                    }
                    else
                    {
                        rightSideVert0 = v0World;
                        rightSideVert1 = v1World;
                        leftSideVert0 = v2World;
                    }

                    Vector3 alignedRightSidePoint0 = AlignPointToPlane(rightSideVert0, leftSideVert0, Vector3.right, worldHitPoint);
                    Vector3 alignedRightSidePoint1 = AlignPointToPlane(rightSideVert1, leftSideVert0, Vector3.right, worldHitPoint);

                    Gizmos.color = Color.black;

                    //DrawTriangle(alignedRightSidePoint0, leftSideVert0, alignedRightSidePoint1);
                    assembledVertices.Add(hitCarvable.transform.InverseTransformPoint(alignedRightSidePoint0));
                    assembledVertices.Add(hitCarvable.transform.InverseTransformPoint(leftSideVert0));
                    assembledVertices.Add(hitCarvable.transform.InverseTransformPoint(alignedRightSidePoint1));

                    assembledIndices.Add(assembledVertices.Count - 3);
                    assembledIndices.Add(assembledVertices.Count - 2);
                    assembledIndices.Add(assembledVertices.Count - 1);

                }

                tVertList[subMeshIDX] = assembledVertices;
                tIndexList[subMeshIDX] = assembledIndices;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        //raycast to try hitting a carvable mesh
        Vector3 hitPoint = Vector3.zero;
        Vector3 hitNormal = Vector3.zero;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            hitCarvable = hit.collider.GetComponent<CarvableMesh>();
            hitPoint = hit.point;
            hitNormal = hit.normal;
        }
        else
        {
            hitCarvable = null;
        }

        if (hitCarvable != null)
        {
            MeshCalculate();


            targetedMesh = hitCarvable.mesh;
            localHitPoint = hitCarvable.transform.InverseTransformPoint(hitPoint);
            worldHitPoint = hitPoint;
            localAxis = hitCarvable.transform.InverseTransformDirection(Vector3.right);

            TargetCarve(hitPoint, hitNormal);

            if (Input.GetMouseButtonDown(0))
            {
                Carve(hitCarvable, hitPoint);
                carving = true;
            }

        }
        else
        {
            targetedMesh = null;
        }

        MoveCarveTool();


    }

    void MoveCarveTool()
    {
        Vector3 toolDistance = (toolPosition - toolTargetPosition);
        if (toolDistance.magnitude > 1.0f)
        {
            toolPosition -= toolDistance.normalized * Time.deltaTime * 5.0f;
        }
        else
        {
            toolPosition -= toolDistance * Time.deltaTime * 2.0f;
        }

        carvingToolObject.transform.position = toolPosition;
    }

    void TargetCarve(Vector3 hitPoint, Vector3 hitNormal)
    {
        toolTargetPosition = hitPoint + hitNormal * 1.0f;
    }

    void Carve(CarvableMesh toCarve, Vector3 hitPoint)
    {
        Vector3 CarveAxis = Vector3.right;

        Mesh assembledMesh = new Mesh();
        List<Vector3> combinedVerts = new List<Vector3>();
        foreach(List<Vector3> submeshVert in tVertList)
        {
            foreach(Vector3 vert in submeshVert)
            {
                combinedVerts.Add(vert);
            }
        }
        List<int> combinedIndices = new List<int>();
        foreach (List<int> submeshIndices in tIndexList)
        {
            foreach (int idx in submeshIndices)
            {
                combinedIndices.Add(idx);
            }
        }

        assembledMesh.vertices = combinedVerts.ToArray();

        List<SubMeshDescriptor> madeSubmeshes = new List<SubMeshDescriptor>();
        for (int i = 0; i < tVertList.Count; i++)
        {
            SubMeshDescriptor subDesc = new SubMeshDescriptor();

            int previousIndices = 0;
            for (int sm = 0; sm < i; sm++)
            {
                previousIndices += tIndexList[sm].Count;
            }
            subDesc.baseVertex = previousIndices;
            madeSubmeshes.Add(subDesc);
        }

        assembledMesh.SetSubMeshes(madeSubmeshes.ToArray());
        for (int i = 0; i < tVertList.Count; i++)
        {
            assembledMesh.SetTriangles(tIndexList[i].ToArray(), i);
        }

        assembledMesh.RecalculateNormals();
        assembledMesh.RecalculateBounds();
        assembledMesh.Optimize();
        assembledMesh.name = "Assembled Mesh";
        GameObject assembledObject = new GameObject("Assembled Object");
        assembledObject.transform.position = hitCarvable.transform.position;
        assembledObject.transform.rotation = hitCarvable.transform.rotation;
        assembledObject.transform.localScale = hitCarvable.transform.localScale;
        assembledObject.AddComponent<MeshFilter>().sharedMesh = assembledMesh;
        assembledObject.AddComponent<MeshRenderer>().sharedMaterial = hitCarvable.GetComponent<MeshRenderer>().sharedMaterial;
        assembledObject.AddComponent<MeshCollider>().sharedMesh = assembledMesh;

        Debug.Log("spawned assembled object");
    }

    enum Side
    {
        Left,
        Right,
        On,
        None
    }

    Side GetSide(Vector3 point, Vector3 planeOrigin, Vector3 planeNormal)
    {
        Vector3 localPoint = point - planeOrigin;
        float dot = Vector3.Dot(localPoint, planeNormal);
        if (dot > 0)
        {
            return Side.Left;
        }
        else if (dot < 0)
        {
            return Side.Right;
        }
        else
        {
            return Side.On;
        }
    }

    class CutTriangle
    {
        public int i0;
        public int i1;
        public int i2;

        public Side i0Side = Side.None;
        public Side i1Side = Side.None;
        public Side i2Side = Side.None;

        public CutTriangle(int a, int b, int c, Side i0Side, Side i1Side, Side i2Side) 
        {
            i0 = a;
            i1 = b;
            i2 = c;
            this.i0Side = i0Side;
            this.i1Side = i1Side;
            this.i2Side = i2Side;

        }
    }

    Vector3 GetPlaneIntersectionPoint(Vector3 pointA, Vector3 pointB, Vector3 planePosition, Vector3 planeNormal)
    {
        // Calculate direction vector AB
        Vector3 AB = (pointB - pointA);

        // Calculate the dot product of planeNormal and AB
        float dotProduct = Vector3.Dot(planeNormal, AB);

        // Check if the line is parallel to the plane
        //if (Mathf.Approximately(dotProduct, 0))
        if (Mathf.Abs(dotProduct) < 0.25f)
        {
            // Line is parallel to the plane, no intersection point
            // You might want to handle this case depending on your requirements
            return Vector3.zero;
        }

        // Calculate the parameter t
        float t = Vector3.Dot(planeNormal, planePosition - pointA) / dotProduct;

        // Calculate the intersection point
        Vector3 intersectionPoint = pointA + t * AB;

        //is the intersection not between pointA and pointB?
        if (t < 0 || t > 1)
        {
            return Vector3.zero;
        }

        return intersectionPoint;
    }

    private void DrawTriangle(Vector3 pointA, Vector3 pointB, Vector3 pointC)
    {
        /*if (pointA != Vector3.zero && pointB != Vector3.zero) */{ Gizmos.DrawLine(pointA, pointB); }
        /*if (pointB != Vector3.zero && pointC != Vector3.zero) */{ Gizmos.DrawLine(pointB, pointC); }
        /*if (pointC != Vector3.zero && pointA != Vector3.zero) */{ Gizmos.DrawLine(pointC, pointA); }
    }

    private bool TriangleIntersectsPlane(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 planePosition, Vector3 planeNormal)
    {
        Side aSide = GetSide(pointA, planePosition, planeNormal);
        Side bSide = GetSide(pointB, planePosition, planeNormal);
        Side cSide = GetSide(pointC, planePosition, planeNormal);

        if (aSide == Side.On || bSide == Side.On || cSide == Side.On)
        {
            return true;
        }

        if (aSide == bSide && bSide == cSide)
        {
            return false;
        }

        return true;
    }

    private Vector3 AlignPointToPlane(Vector3 point, Vector3 otherPoint, Vector3 planeNormal, Vector3 planePosition)
    {
        Vector3 direction = (point - otherPoint).normalized;

        //move point along direction until it is on the plane
        float distance = Vector3.Dot(planeNormal, planePosition - point) / Vector3.Dot(planeNormal, direction);
        Vector3 alignedPoint = point + direction * distance;
        
        return alignedPoint;
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < tIndexList.Count; i ++)
        {

            for (int j = 0; j < tIndexList.Count; j += 3)
            {
                Vector3 v0Position = tVertList[i][tIndexList[i][j + 0]];
                Vector3 v1Position = tVertList[i][tIndexList[i][j + 1]];
                Vector3 v2Position = tVertList[i][tIndexList[i][j + 2]];

                Vector3 v0World = hitCarvable.transform.TransformPoint(v0Position);
                Vector3 v1World = hitCarvable.transform.TransformPoint(v1Position);
                Vector3 v2World = hitCarvable.transform.TransformPoint(v2Position);
                DrawTriangle(v0World, v1World, v2World);
            }
        }

    }
}
