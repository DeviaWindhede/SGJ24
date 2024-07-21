using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlchemyInteractArea : MonoBehaviour
{
    public Vector3 offset = Vector3.zero;
    public Vector3 pourOffset = Vector3.zero;

    public bool customPlacementOrientation = false;
    public Quaternion placementOrientation;

    public bool validWhenDropped = false;

    public enum AreaType
    {
        Mortar,
        Pot,
        CuttingBoard,
        MortarPosition,
    }

    public AreaType type;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Vector3 GetTargetWorldPos(bool pourable)
    {
        return transform.position + transform.TransformDirection(offset) + (pourable ? transform.TransformDirection(pourOffset) : Vector3.zero);
    }

    public bool AcceptsObject(MovableObject objectQuery)
    {

        var objType = objectQuery.type;
        
        switch (type)
        {
            case AreaType.MortarPosition:
                return objType == MovableObject.ObjectType.Mortar;
            case AreaType.Mortar:
                return objType == MovableObject.ObjectType.Ingredient;
            case AreaType.Pot:
                return objType == MovableObject.ObjectType.Ingredient;
            case AreaType.CuttingBoard:
                return objType == MovableObject.ObjectType.Cuttable;
            default:
                return false;
        }
    }
}
