using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shape : MonoBehaviour
{
    [SerializeField] public List<Vector3> basePoints = new List<Vector3>();
    [SerializeField] public List<Vector3> subdividedPoints = new List<Vector3>();

    [SerializeField] string jsonString = "";

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void CalculateBase()
    {
        //decode jsonString
        basePoints.Clear();
        string[] lines = jsonString.Split(' ');
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains('#')) continue;

            string[] numbers = lines[i].Split(',');
            if (numbers.Length == 0) continue;

            float x = float.Parse(numbers[0]) / 10.0f;
            float y = -float.Parse(numbers[1]) / 10.0f + 5.0f;
            basePoints.Add(new Vector3(x, y, 0));

        }
    }
    void DrawBase()
    {
        for (int i = 0; i < basePoints.Count; i++)
        {
            //Gizmos.DrawWireSphere(basePoints[i], 0.01f);
            if (i < basePoints.Count - 1)
            {
                Gizmos.DrawLine(transform.position + basePoints[i], transform.position + basePoints[(i + 1) % basePoints.Count]);
            }
        }
    }

    void SubdivideBase()
    {
        subdividedPoints.Clear();
        const int numSubdivisions = 10;

        for (int i = 0; i < basePoints.Count - 1; i++)
        {
            Vector3 p1 = basePoints[i];
            Vector3 p2 = basePoints[(i + 1)];
            float distance = Vector3.Distance(p1, p2);

            int workingSubdivisions = (int)(numSubdivisions * distance * 10);

            for (int j = 0; j < workingSubdivisions; j++)
            {
                float t = j / (float)workingSubdivisions;
                Vector3 p = p1 + t * (p2 - p1);
                subdividedPoints.Add(p);
            }
        }
    }

    void DrawSubdivide()
    {
     
        
        for (int i = 0; i < subdividedPoints.Count - 1; i++)
        {
            Debug.DrawLine(transform.position + subdividedPoints[i], transform.position + subdividedPoints[(i + 1)], i % 2 == 0 ? Color.green : Color.blue);
        }
    }

    void OnDrawGizmos()
    {
        CalculateBase();
        SubdivideBase();
        DrawBase();
        //DrawSubdivide();
    }

    Vector3 GetClosestPoint(Vector3 comparison)
    {

        float minDistance = float.MaxValue;
        int minIndex = -1;
        for (int i = 0; i < subdividedPoints.Count; i++)
        {
            float distance = Vector3.Distance(comparison, subdividedPoints[i]);
            if (distance < minDistance)
            {
                minDistance = distance;
                minIndex = i;
            }
        }

        if (minIndex > -1) return subdividedPoints[minIndex];

        return new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
    }

    public float Match(List<Vector3> points)
    {
        float totalDistance = 0;
        for (int i = 0; i < points.Count; i++)
        {
            Vector3 closest = GetClosestPoint(points[i]);
            totalDistance += Vector3.Distance(points[i], closest);
        }

        return totalDistance / points.Count;
    }
}


