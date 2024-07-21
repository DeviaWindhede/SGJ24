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

    public string runePointsString;
    public float runeSizeMultiplier = 1.0f;

    public List<Vector2> GetPoints()
    {
        return points;
    }
    void OnValidate()
    {
        if (runePointsString != null && runePointsString != "")
        {
            points.Clear();
            string[] pointsString = runePointsString.Split(' ');
            foreach (string pointString in pointsString)
            {
                if (pointString == "" || pointString == null) { continue; }
                string[] point = pointString.Split(',');
                points.Add(new Vector2(float.Parse(point[0]), float.Parse(point[1])) * runeSizeMultiplier);
            }
        }
    }

}
