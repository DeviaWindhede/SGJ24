using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PixelCamRaycast : MonoBehaviour
{
    private Camera cam;

    // Start is called before the first frame update
    void Start() 
    {
        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update() {}

    public int renderWidth = 640;
    
    public Ray GetRay(Vector3 screenPos)
    {
        float resolutionFactor = Screen.width / (float)renderWidth;

        Ray ray = cam.ScreenPointToRay(screenPos / resolutionFactor);
        return ray;
    }
}
