using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ShapeMeshGenerator : MonoBehaviour
{
    [SerializeField] Shape shape;
    [SerializeField] ShapeDrawer shapeDrawer;

    struct Segment
    {
       public Vector3 edge0;
       public Vector3 edge1;
    }

    List<Vector3> storedVertices = new List<Vector3>();
    List<int> storedIndices= new List<int>();

    int storedPointCount = 0;
    void OnEnable()
    {
        storedVertices.Clear();
        storedIndices.Clear();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        List<Vector3> pointList = new List<Vector3>();

        if (shape != null)
        {
            pointList = shape.basePoints;
        }
        if (shapeDrawer != null)
        {
            pointList = shapeDrawer.drawnPoints;
        }

        if (pointList.Count != storedPointCount)
        {
            storedPointCount = pointList.Count;
        }
        else
        {
            return;
        }

        var mesh = new Mesh
        {
            name = "Procedural Mesh"
        };

        Debug.Log("mesh generated");


        List<Segment> segments = new List<Segment>();

        for (int i = 0; i < pointList.Count -1; i++)
        {
            Vector3 point0 = pointList[i];
            Vector3 point1 = pointList[i+1];

            Vector3 dir = point1 - point0;
            Vector3 dirPerpendicular = new Vector3(-dir.y, dir.x, 0);

            Segment s;

            float scaleFac = 1.0f;

            int distanceToEnd = Mathf.Min(i, pointList.Count - i);
            if (distanceToEnd < 10)
            {
                float sc = (float)distanceToEnd / 10.0f;
                scaleFac *= (sc*sc);
            }

            s.edge0 = point0 - dirPerpendicular.normalized * 0.2f * scaleFac;
            s.edge1 = point0 + dirPerpendicular.normalized * 0.2f * scaleFac;
            segments.Add(s);
        }

        //for (int i = 0; i < 1000; i++)
        //{
        //    float progress = (float)i / 1000.0f;
        //    float smoothFactor = 0.1f;
        //
        //    int idx0 = Mathf.FloorToInt(progress * pointList.Count);
        //    int idx1 = Mathf.CeilToInt(progress * pointList.Count);
        //
        //    if (idx1 >= pointList.Count) break;
        //
        //    Vector3 point0 = pointList[idx0];
        //    Vector3 point1 = pointList[idx1];
        //
        //    Vector3 dir = point1 - point0;
        //    Vector3 dirPerpendicular = new Vector3(-dir.y, dir.x, 0);
        //
        //    Segment s;
        //    s.edge0 = point0 - dirPerpendicular.normalized * 0.25f;
        //    s.edge1 = point0 + dirPerpendicular.normalized * 0.25f;
        //    segments.Add(s);
        //
        //}

        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();
        List<Vector3> normals = new List<Vector3>();

        for (int i = 0; i < segments.Count - 1; i++)
        {
            vertices.Add(segments[i].edge0);
            vertices.Add(segments[i].edge1);
            normals.Add(Vector3.back);
            normals.Add(Vector3.back);

            int segment0StartIndex = i * 2;
            int segment1StartIndex = (i + 1) * 2;

            indices.Add(segment0StartIndex);
            indices.Add(segment1StartIndex);
            indices.Add(segment0StartIndex + 1);

            indices.Add(segment1StartIndex);
            indices.Add(segment1StartIndex + 1);
            indices.Add(segment0StartIndex + 1);
        }

        vertices.Add(segments[segments.Count - 1].edge0);
        vertices.Add(segments[segments.Count - 1].edge1);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);


        storedVertices = vertices;
        storedIndices = indices;

        mesh.vertices = vertices.ToArray();
        mesh.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);
        mesh.normals = normals.ToArray();

        GetComponent<MeshFilter>().mesh = mesh;

    }

    private void OnValidate()
    {
        
    }

    private void OnDrawGizmos()
    {
        //for (int i = 0; i < storedIndices.Count; i += 3)
        //{
        //    Gizmos.DrawLine(storedVertices[storedIndices[i]], storedVertices[storedIndices[i + 1]]);
        //    Gizmos.DrawLine(storedVertices[storedIndices[i+1]], storedVertices[storedIndices[i + 2]]);
        //    Gizmos.DrawLine(storedVertices[storedIndices[i + 2]], storedVertices[storedIndices[i]]);
        //}
    }
}


