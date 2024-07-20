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

    [SerializeField] public EnchantableObject enchantableObject;

    Vector3 lastDrawnPosition = Vector3.negativeInfinity;

    private List<Vector3> drawnPoints = new List<Vector3>();
    private List<Vector3> runeShapeScreenPoints = new List<Vector3>();
    private float accumulatedDrawnDistance = 0.0f;

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
        //float average = enchantableObject.GetShapeMatchingFactor(enchantableObject.focusedRuneIndex, points, usingCameraComponent);
        //return average;

        return accumulatedDrawnDistance / drawnPoints.Count;

    }

    void StartDrawingRune(int runeIndex)
    {
        runeShapeScreenPoints = enchantableObject.GetShapeScreenPositions(runeIndex, usingCameraComponent);
        accumulatedDrawnDistance = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = usingCamera.GetRay(Input.mousePosition);

        if (Input.GetKeyDown(KeyCode.P))
        {
            StartDrawingRune(enchantableObject.focusedRuneIndex);
        }

        if (Input.GetMouseButton(0))
        {
            //trail.AddPosition(ray.origin + ray.direction * 0.01f);

            Vector3 position = ray.origin + ray.direction * trailDrawDistance;
            trail.transform.position = position;


            Vector3 screenPosition = usingCameraComponent.WorldToScreenPoint(position);

            if (Vector3.Distance(screenPosition, lastDrawnPosition) > 0.1f)
            {
                drawnPoints.Add(screenPosition);
                lastDrawnPosition = screenPosition;

                int nearestIndex = enchantableObject.GetNearestPointIndex(runeShapeScreenPoints, screenPosition, out float pointDistance);
                accumulatedDrawnDistance += pointDistance;
                //float score = CalculateMatchScore();
                Debug.Log(CalculateMatchScore());
                //Debug.Log(drawnPoints.Count);
                Debug.Log(runeShapeScreenPoints.Count);
            }
            

        }
        else
        {
            trail.transform.position = ray.origin + ray.direction * trailDrawDistance;
            trail.Clear();
            drawnPoints.Clear();
        }

    }
}
