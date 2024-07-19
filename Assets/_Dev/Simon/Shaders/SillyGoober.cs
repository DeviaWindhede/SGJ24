using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class SillyGoober : MonoBehaviour
{
    private void SyncMaterialProperty()
    {
        Renderer rend = GetComponent<Renderer>();
        rend.material.SetFloat("_SillyGoober", (float)transform.GetInstanceID());
    }

    // Start is called before the first frame update
    void Start()
    {
        SyncMaterialProperty();
    }

    // Update is called once per frame
    void Update()
    {

    }

    //private void OnValidate()
    //{
    //    SyncMaterialProperty();
    //}
}