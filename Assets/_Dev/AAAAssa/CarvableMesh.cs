using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarvableMesh : MonoBehaviour
{
    public Mesh mesh;

    // Start is called before the first frame update
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
