using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuoyancyArea : MonoBehaviour
{
    [SerializeField] public float areaRadius = 1.0f;
    [SerializeField] public float buoyancyForce = 1.0f;
   
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void DrawCircle(Vector3 position, float radius, int segments)
    {
        for (int i = 0; i < segments; i++)
        {
            float angle = i * 2 * Mathf.PI / segments;
            Vector3 p1 = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius) + position;
            angle = (i + 1) * 2 * Mathf.PI / segments;
            Vector3 p2 = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius) + position;
            Gizmos.DrawLine(p1, p2);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        DrawCircle(transform.position, areaRadius, 16);
    }
}
