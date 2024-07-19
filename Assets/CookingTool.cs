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
    
    
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame

}
