using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class EnchantableObject : MonoBehaviour
{

    public List<RuneShape> requestedRunes = new List<RuneShape>();
    public List<Vector3> runePositions = new List<Vector3>();

    private List<LineRenderer> lineRenderers = new List<LineRenderer>();

    [SerializeField] private Material lineMaterial;
    [SerializeField] private Material focusedLineMaterial;

    [SerializeField] public float runeFocusDistance = 0.5f;

    [SerializeField] private GameObject cameraOrigin;

    public Camera camToMove;
    public int focusedRuneIndex = -1;
    //public bool move = false;


    void ConfigureLine(int runeIndex)
    {
        var linePoints = GetRuneWorldPositions(runeIndex);
        lineRenderers[runeIndex].SetPositions(linePoints.ToArray());
    }

    // Start is called before the first frame update
    void Start()
    {
        //create Line object and give it points
        for (int i = 0; i < requestedRunes.Count; i++)
        {
            GameObject lineObject = new GameObject();
            lineObject.name = "RuneLine " + i.ToString();
            var lineRenderer = lineObject.AddComponent<LineRenderer>();
            var linePoints = GetRuneWorldPositions(i);
            lineRenderer.positionCount = linePoints.Count;
            lineRenderer.widthMultiplier = 0.015f;
            lineRenderer.loop = requestedRunes[i].isClosed;
            lineRenderer.SetPositions(linePoints.ToArray());
            lineRenderer.material = lineMaterial;

            lineRenderers.Add(lineRenderer);
        }


        ConfigureLine(0);
        ConfigureLine(1);
        ConfigureLine(2);
    }

    void MoveCameraToRune(Camera cameraToMove, int runeIndex)
    {

        Vector3 basePosition = transform.TransformPoint(runePositions[runeIndex]);
        Vector3 direction = transform.TransformDirection(0, 0, -1);
        Vector3 localUp = transform.TransformDirection(Vector3.up);
        Vector3 targetPosition = basePosition + direction * runeFocusDistance;

        Quaternion targetOrientation = Quaternion.LookRotation(basePosition - targetPosition, localUp);

        cameraToMove.transform.position = Vector3.Lerp(cameraToMove.transform.position, targetPosition, 0.05f);
        cameraToMove.transform.rotation = Quaternion.Slerp(cameraToMove.transform.rotation, targetOrientation, 0.05f);

        //cameraToMove.transform.LookAt(basePosition, localUp);
    }

    void MoveCameraToBasePosition(Camera cameraToMove)
    {
        Vector3 targetPosition = cameraOrigin.transform.position;
        Quaternion targetOrientation = cameraOrigin.transform.rotation;

        cameraToMove.transform.position = Vector3.Lerp(cameraToMove.transform.position, targetPosition, 0.05f);
        cameraToMove.transform.rotation = Quaternion.Slerp(cameraToMove.transform.rotation, targetOrientation, 0.05f);
    }

    // Update is called once per frame
    void Update()
    {
        if (focusedRuneIndex >= 0 && focusedRuneIndex < requestedRunes.Count)
        {
            MoveCameraToRune(camToMove, focusedRuneIndex);
        }
        else
        {
            MoveCameraToBasePosition(camToMove);
        }
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (focusedRuneIndex < requestedRunes.Count && focusedRuneIndex >= 0)
            {
                lineRenderers[focusedRuneIndex].material = lineMaterial;
            }
            focusedRuneIndex++;
            if (focusedRuneIndex < requestedRunes.Count && focusedRuneIndex >= 0)
            {
                lineRenderers[focusedRuneIndex].material = focusedLineMaterial;
            }


            if (focusedRuneIndex >= requestedRunes.Count) { focusedRuneIndex = -1; }
        }
    }

    List<Vector3> GetRuneWorldPositions(int runeIndex)
    {
        RuneShape rune = requestedRunes[runeIndex];
        Vector3 requestedPosition = runePositions[runeIndex];

        List<Vector3> output = new List<Vector3>();
        var runePoints = rune.GetPoints();
        for (int i = 0; i < runePoints.Count; i++)
        {
            Vector2 runePos = runePoints[i];
            Vector3 worldPos = transform.TransformPoint(requestedPosition + new Vector3(runePos.x, runePos.y, 0));
            output.Add(worldPos);
        }

        return output;
    }

    void RenderRunes()
    {
        for (int i = 0; i < requestedRunes.Count; i++)
        {
            RuneShape rune = requestedRunes[i];

            var points = GetRuneWorldPositions(i);
            for (int j = 0; j < points.Count - (rune.isClosed ? 0 : 1); j++)
            {
                Vector3 startPoint = points[j];
                Vector3 endPoint = points[(j + 1) % points.Count];

                Gizmos.DrawLine(startPoint, endPoint);
            }

        }
    }

    List<Vector3> SubdivideRune(RuneShape rune)
    {
        List<Vector3> subdivided = new List<Vector3>();
        for (int i = 0; i < rune.points.Count - (rune.isClosed ? 0 : 1); i++)
        {
            Vector3 startPoint = rune.points[i];
            Vector3 endPoint = rune.points[(i + 1) % rune.points.Count];

            //subdivided.Add(startPoint);
            int steps = 16;
            for (int j = 0; j < steps; j++)
            {
                float progress = (float)j / (float)steps;
                Vector3 mixedPoint = Vector3.Lerp(startPoint, endPoint, progress);
                subdivided.Add(mixedPoint);

            }
        }

        return subdivided;
    }

    public int GetNearestPointIndex(List<Vector3> points, Vector3 target, out float outDistance)
    {
        float minDistance = float.MaxValue;
        int minDistanceIndex = 0;

        for (int i = 0; i < points.Count; i++)
        {
            float distance = Vector3.Distance(points[i], target);
            if (distance < minDistance)
            {
                minDistance = distance;
                minDistanceIndex = i;
            }
        }
        outDistance = minDistance;
        return minDistanceIndex;
    }

    

    public float GetShapeMatchingFactor(int runeIndex, List<Vector3> drawnShape, Camera cam)
    {
        List<Vector3> runeShape = GetShapeScreenPositions(runeIndex, cam);
        float accumulatedDistance = 0.0f;
        for (int i = 0; i < drawnShape.Count; i++)
        {
            int nearestIndex = GetNearestPointIndex(runeShape, drawnShape[i], out float pointDistance);
            accumulatedDistance += pointDistance;

        }
        float average = accumulatedDistance / drawnShape.Count;

        return average;
    }

    public List<Vector3> GetShapeScreenPositions(int runeShapeIndex, Camera cam)
    {
        List<Vector3> outScreenPoints = new List<Vector3>();

        RuneShape rune = requestedRunes[runeShapeIndex];
        Vector3 requestedPosition = runePositions[runeShapeIndex];

        var points = SubdivideRune(rune);

        for (int i = 0; i < points.Count; i++)
        {
            Vector3 point = points[i];
            Vector3 screenPoint = cam.WorldToScreenPoint(transform.TransformPoint(requestedPosition + new Vector3(points[i].x, points[i].y, 0)));
            outScreenPoints.Add(screenPoint);
        }

        return outScreenPoints;
    }

    void OnDrawGizmos()
    {
        RenderRunes();
    }

}
