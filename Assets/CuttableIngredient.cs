using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CuttableIngredient : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] public List<GameObject> cutObjects = new List<GameObject>();
    [SerializeField] public List<float> cutPoints = new List<float>();
    [SerializeField] public Vector3 cutAxis = Vector3.up;

    [SerializeField] private int cutIndex = 0;
    [SerializeField] private bool cut = false;
    

    void Start()
    {
        //foreach (GameObject cut in cutObjects)
        //{
        //    Rigidbody cutRigid = cut.GetComponent<Rigidbody>();
        //    MeshCollider cutMeshCollider = cut.GetComponent<MeshCollider>();
        //    cutMeshCollider.enabled = false;
        //}
    }

    // Update is called once per frame
    void Update()
    {
        if(cut)
        {
            cut = false;

            CutAtSeam(cutIndex);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Vector3 localAxis = transform.TransformDirection(cutAxis);
        Vector3 localRight = transform.TransformDirection(Vector3.right);
        Vector3 localForward = transform.TransformDirection(Vector3.fwd);

        for (int i = 0; i < cutPoints.Count; i++)
        {
            Gizmos.color = i == cutIndex ? Color.green : Color.red;

            bool isValidSeam = false;
            for (int j = 0; j < i+1; j++)
            {
                if (cutObjects[j].gameObject.activeSelf) { isValidSeam = true; }
            }

            if (!isValidSeam) { continue; }

            float cutPoint = cutPoints[i];
            Vector3 cutPosition = transform.position + localAxis * cutPoint;

            Vector3 minPoint = cutPosition - localRight * 0.1f - localForward * 0.1f;
            Vector3 maxPoint = cutPosition + localRight * 0.1f + localForward * 0.1f;

            //Gizmos.DrawLine(minPoint, maxPoint);
            //Gizmos.DrawWireSphere(cutPosition, 0.05f);
        }
    }
    
    public Mesh CreateMeshForCollision()
    {
        int livingChildren = 0;
        foreach (GameObject cut in cutObjects)
        {
            if (cut.gameObject.activeSelf)
            {
                livingChildren++;
            }
        }

        CombineInstance[] combine = new CombineInstance[livingChildren];

        int idx = 0;
        foreach (GameObject cut in cutObjects)
        {
            if (!cut.activeSelf) { continue; }

            MeshFilter meshFilter = cut.GetComponent<MeshFilter>();
            combine[idx].mesh = meshFilter.sharedMesh;

            Matrix4x4 meshMatrix = Matrix4x4.TRS(cut.transform.localPosition, cut.transform.localRotation, cut.transform.localScale);
            combine[idx].transform = meshMatrix;
            idx++;


        }

        Mesh newMesh = new Mesh();
        newMesh.CombineMeshes(combine);
        newMesh.RecalculateBounds();
       
        return newMesh;
    }

    public void CorrectColliders()
    {
        int livingChildren = 0;
        foreach (GameObject cut in cutObjects)
        {
            if (cut.gameObject.activeSelf)
            {
                livingChildren++;
            }
        }

        //CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();

        if (livingChildren == 0)
        {
            Destroy(this.gameObject);
        }
        else if (livingChildren == 1)
        {
            //capsuleCollider.enabled = false;
            GetComponent<MeshCollider>().enabled = false;
            GetComponent<Rigidbody>().isKinematic = true;

            foreach (GameObject cut in cutObjects)
            {
                cut.GetComponent<MeshCollider>().enabled = true;
                cut.GetComponent<Rigidbody>().isKinematic = false;
            }
        }
        else
        {
            GetComponent<MeshCollider>().sharedMesh = CreateMeshForCollision();
        }
    }

    public void CutAtSeam(int seamIndex)
    {
        bool anyAvailableCut = false;
        for (int i = 0; i < cutObjects.Count; i++)
        if (i < seamIndex + 1 && cutObjects[i].activeSelf)
        {
            anyAvailableCut = true;
            break;
        }

        if (!anyAvailableCut)
        {
            return;
        }

        GameObject copy = Instantiate(this.gameObject);
        CuttableIngredient copyCut = copy.GetComponent<CuttableIngredient>();

        for (int i = 0; i < cutObjects.Count; i++)
        {
            if (i < seamIndex + 1)
            {
                cutObjects[i].SetActive(false);
                //Destroy(cutObjects[i]);
            }
            else
            {
                copyCut.cutObjects[i].SetActive(false);
                // Destroy(copyCut.cutObjects[i]);

            }
        }

        CorrectColliders();
        copyCut.CorrectColliders();
    }

    private void OnValidate()
    {
        GetComponent<MeshCollider>().sharedMesh = CreateMeshForCollision();
    }

    public int GetCutPoint(Vector3 point, out Vector3 outCutPoint)
    {
        int nearestIndex = -1;
        float nearestCutPoint = float.MaxValue;
        outCutPoint = Vector3.zero;

        for (int i = 0; i < cutPoints.Count; i++)
        {
            Vector3 localAxis = transform.TransformDirection(cutAxis);
            Vector3 cutPosition = transform.position + localAxis * cutPoints[i];
            float distance = Vector3.Distance(cutPosition, point);
            if (distance < nearestCutPoint)
            {
                nearestCutPoint = distance;
                nearestIndex = i;
                outCutPoint = cutPosition;
            }
        }
        return nearestIndex;
    }
}
