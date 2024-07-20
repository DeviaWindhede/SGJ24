using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RotateTowardsCamera : MonoBehaviour
{
    private Camera _mainCamera;

    private void Awake()
    {
        _mainCamera = FindObjectOfType<PixelCamRaycast>().GetComponent<Camera>();
    }

    public void Rotate()
    {
        transform.rotation = _mainCamera.transform.rotation;
    }

    void Update()
    {
        Rotate();
    }
}
