using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

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

    public Vector3 pestleOrbitPosition = Vector3.zero;
    public float pestleOrbitDistance = 0.0f;
    public float pestleOrbitAngle = 0.0f;
    [SerializeField] private CookingMortarPestle mortar;
    [SerializeField] private PixelCamRaycast pestleOrbitCamera;
    private Vector3 pestleClickPosition;
    private float pestleProgressWindup = 0.0f;

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

    public void PestleAssignOrbit(Vector3 position, float distance)
    {
        pestleOrbitPosition = position;
        pestleOrbitDistance = distance;
    }

    float GetAngleDifference(float angle0, float angle1)
    {
        float dir0 = angle1 - angle0;
        if (Mathf.Abs(dir0) <= 180.0f) { return dir0; }

        return 360.0f - Mathf.Abs(dir0);
    }

    private void Update()
    {
        if (toolType == CookingToolType.Pestle)
        {
            Vector3 mousePos = Input.mousePosition;
            if (Input.GetMouseButtonDown(0))
            {
                pestleClickPosition = mousePos;
            }
            if (Input.GetMouseButton(0))
            {
                pestleProgressWindup = Mathf.Clamp01(pestleProgressWindup + Time.deltaTime);
            }
            else
            {
                pestleProgressWindup = 0.0f;
            }


            MovableObject movable = GetComponent<MovableObject>();
            if (movable.pickedUp)
            {
                if (mortar.mortarState != CookingMortarPestle.MortarState.Grind)
                {
                    mortar.SwitchState(CookingMortarPestle.MortarState.Grind);
                }

                Vector3 mouseDiff = mousePos - pestleClickPosition;
                float angle = Mathf.Atan2(mouseDiff.y, mouseDiff.x);
                float oldAngle = pestleOrbitAngle;

                
                pestleOrbitAngle = Mathf.Rad2Deg * angle * pestleProgressWindup;

                transform.localPosition = pestleOrbitPosition + new Vector3(Mathf.Cos(pestleOrbitAngle * Mathf.Deg2Rad), 0.0f, Mathf.Sin(pestleOrbitAngle * Mathf.Deg2Rad)) * pestleOrbitDistance * pestleProgressWindup;
                
                mortar.UpdateGrindProgress(Mathf.Abs(GetAngleDifference(pestleOrbitAngle, oldAngle)) * pestleProgressWindup);
            }
            else
            {
                if (mortar.mortarState == CookingMortarPestle.MortarState.Grind)
                {
                    mortar.SwitchState(CookingMortarPestle.MortarState.Idle);
                }
            }
        }
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
