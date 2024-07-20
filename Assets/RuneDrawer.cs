using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class RuneDrawer : MonoBehaviour
{
    [SerializeField] private PixelCamRaycast usingCamera;
    private Camera usingCameraComponent;

    [SerializeField] private Material trailMaterial;
    TrailRenderer trail;

    [SerializeField] private float trailDrawDistance = 0.25f;
    List<Vector3> points = new List<Vector3>();

    [SerializeField] public EnchantableObject enchantableObject;

    Vector3 lastDrawnPosition = Vector3.negativeInfinity;

    // Start is called before the first frame update
    void Start()
    {
        GameObject trailObj = new GameObject();
        trailObj.name = "RuneTrail";
        trail = trailObj.AddComponent<TrailRenderer>();
        trail.time = 1000;
        trail.widthMultiplier = 0.01f;
        trail.minVertexDistance = 0.01f;
        trail.material = trailMaterial;

        usingCameraComponent = usingCamera.GetComponent<Camera>();
    }

    float CalculateMatchScore()
    {
        float average = enchantableObject.GetShapeMatchingFactor(enchantableObject.focusedRuneIndex, points, usingCameraComponent);
        return average;
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = usingCamera.GetRay(Input.mousePosition);

        if (Input.GetMouseButton(0))
        {
            //trail.AddPosition(ray.origin + ray.direction * 0.01f);

            Vector3 position = ray.origin + ray.direction * trailDrawDistance;
            trail.transform.position = position;


            Vector3 screenPosition = usingCameraComponent.WorldToScreenPoint(position);

            if (Vector3.Distance(screenPosition, lastDrawnPosition) > 0.1f)
            {
                points.Add(screenPosition);
                lastDrawnPosition = screenPosition;
                float score = CalculateMatchScore();
                Debug.Log(score);
            }
            

        }
        else
        {
            trail.transform.position = ray.origin + ray.direction * trailDrawDistance;
            trail.Clear();
            points.Clear();
        }

    }
}
