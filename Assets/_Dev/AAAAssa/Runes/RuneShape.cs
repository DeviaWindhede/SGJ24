using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

[CreateAssetMenu(fileName = "CustomItem", menuName = "ScriptableObjects/RuneShape", order = 1)]
public class RuneShape : ScriptableObject
{
    public string runeName;

    public List<Vector2> points = new List<Vector2>();
    public bool isClosed = false;

    public List<Vector2> GetPoints()
    {
        return points;
    }

    
}
