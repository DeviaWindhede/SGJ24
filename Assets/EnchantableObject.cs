using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class EnchantableObject : MonoBehaviour
{

    public List<RuneShape> requestedRunes = new List<RuneShape>();
    public List<Vector3> runePositions = new List<Vector3>();

    private List<LineRenderer> lineRenderers = new List<LineRenderer>();

    public int testRuneTarget = 0;
    public bool move = false;
    public Camera camToMove;

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
            lineRenderer.widthMultiplier = 0.02f;
            lineRenderer.loop = requestedRunes[i].isClosed;
            lineRenderer.SetPositions(linePoints.ToArray());
            lineRenderers.Add(lineRenderer);
        }

    }

    void MoveCameraToRune(Camera cameraToMove, int runeIndex)
    {
        Vector3 basePosition = transform.TransformPoint(runePositions[runeIndex]);
        Vector3 direction = transform.TransformDirection(0, 0, -1);
        Vector3 localUp = transform.TransformDirection(Vector3.up);
        Vector3 targetPosition = basePosition + direction * 2.0f;

        cameraToMove.transform.position = targetPosition;
        cameraToMove.transform.LookAt(basePosition, localUp);

    }

    // Update is called once per frame
    void Update()
    {
        
        if (move)
        {
            MoveCameraToRune(camToMove, testRuneTarget);

        }

        ConfigureLine(0);
        ConfigureLine(1);
        ConfigureLine(2);
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

    void OnDrawGizmos()
    {
        RenderRunes();
    }

}
