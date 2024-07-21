using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CookingToolType
{
    Knife,
    Pestle,

}

public class CookingTool : MonoBehaviour
{
    public CookingToolType toolType;

    public Quaternion TargetOrientation;

    //knife variables
    public int knifeCutSeamIndex = -1;
    public CuttableIngredient knifeCutTarget = null;
    //
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void KnifeAssignCuttable(int seamIndex, CuttableIngredient ingredient)
    {
        knifeCutSeamIndex = seamIndex;
        knifeCutTarget = ingredient;
    }

    public void KnifeClearCuttable()
    {
        knifeCutSeamIndex = -1;
        knifeCutTarget = null;
    }

    // Update is called once per frame
    public void UseAnimation()
    {
        if (toolType == CookingToolType.Knife)
        {
            transform.Rotate(Vector3.up, 30.0f, Space.Self);

            if (knifeCutTarget != null && knifeCutSeamIndex >= 0)
            {
                knifeCutTarget.CutAtSeam(knifeCutSeamIndex);
            }
        }
        else if (toolType == CookingToolType.Pestle)
        {
            
        }
    }

}
