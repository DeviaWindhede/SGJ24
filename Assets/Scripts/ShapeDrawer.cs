using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class ShapeDrawer : MonoBehaviour
{
    [SerializeField] Shape shapeToMatch;
    public List<Vector3> drawnPoints = new List<Vector3>();
    public float drawnMatch = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(2))
        {
            drawnPoints.Clear();
        }
        //is left mouse button down?
        if (Input.GetMouseButton(0))
        {
            //get the mouse position
            Ray clickRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            //calculate the point where the ray intersects the z=0 plane
            Vector3 clickRayAt0 = clickRay.origin - clickRay.direction * (clickRay.origin.z / clickRay.direction.z);

            const float distanceBeforeNewPoint = 0.05f;
            if (drawnPoints.Count == 0 || Vector3.Distance(clickRayAt0, drawnPoints[drawnPoints.Count-1]) > distanceBeforeNewPoint)
            {
                drawnPoints.Add(clickRayAt0);
            }
            drawnMatch = shapeToMatch.Match(drawnPoints);
        }

    }

    void OnDrawGizmos()
    {
        //for (int i = 0; i < drawnPoints.Count - 1; i++)
        //{
        //    Debug.DrawLine(drawnPoints[i], drawnPoints[(i + 1)], i % 2 == 0 ? Color.green : Color.blue);
        //}

        //draw text that shows DrawnMatch
        float matchPercentage = Mathf.Round(100.0f - drawnMatch*100);
        string matchString = "" + matchPercentage.ToString() + "%";
        Handles.Label(transform.position, matchString);
    }
}
