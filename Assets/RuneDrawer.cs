using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuneDrawer : MonoBehaviour
{
    [SerializeField] private PixelCamRaycast usingCamera;

    TrailRenderer trail;

    // Start is called before the first frame update
    void Start()
    {
        GameObject trailObj = new GameObject();
        trail = trailObj.AddComponent<TrailRenderer>();
        trail.time = 1000;
        trail.widthMultiplier = 0.02f;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(2))
        {
            trail.Clear();
        }
        if (Input.GetMouseButtonDown(0))
        {
            trail.Clear();
            Ray ray = usingCamera.GetRay(Input.mousePosition);
            trail.transform.position = ray.origin;
        }
        if (Input.GetMouseButton(0))
        {
            Ray ray = usingCamera.GetRay(Input.mousePosition);
            //trail.AddPosition(ray.origin + ray.direction * 0.01f);
            trail.transform.position = ray.origin;
        }


    }
}
