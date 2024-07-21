using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(MeshCollider))]
public class PhysicalIngredient : MonoBehaviour
{
    public AlchemySystem.IngredientInteractType interactType;
    public AlchemySystem.Ingredient ingredientType;
    public Color ingredientColor;

    public float IngredientAmount = 1.0f;

    public float BaseScale = 1.0f;

    public List<BuoyancyArea> buoyancyAreas;

    public float AirDrag = 0.0f;
    public float AirAngularDrag = 0.05f;

    public float WaterDrag = 0.5f;
    public float WaterAngularDrag = 0.5f;

    bool inWater = false;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    void FixedUpdate()
    {

        bool inAnyWater = false;
        for (int i = 0; i < buoyancyAreas.Count; i++)
        {
            BuoyancyArea area = buoyancyAreas[i];
            Vector3 areaPosition = area.transform.position;
            areaPosition.y = 0;

            Vector3 position = transform.position;
            position.y = 0;
            
            float areaRadius = area.areaRadius;
            float areaBuoyancy = area.buoyancyForce;
            float radius = GetComponent<MeshCollider>().bounds.extents.magnitude;

            float distance = Vector3.Distance(areaPosition, position);
            float depth = transform.position.y - area.transform.position.y;
            if (depth < 0 && distance < areaRadius + radius)
            {
                GetComponent<Rigidbody>().AddForceAtPosition(Vector3.up * areaBuoyancy * Mathf.Abs(depth), transform.position, ForceMode.Force);
                inAnyWater = true;
            }
        }

        if (inAnyWater && !inWater)
        {
            ApplyDragSettings(true);
        }
        else if (!inAnyWater && inWater)
        {
            ApplyDragSettings(false);
        }

    }


    void ApplyDragSettings(bool inWater)
    {
        if (inWater)
        {
            GetComponent<Rigidbody>().drag = WaterDrag;
            GetComponent<Rigidbody>().angularDrag = WaterAngularDrag;
        }
        else
        {
            GetComponent<Rigidbody>().drag = AirDrag;
            GetComponent<Rigidbody>().angularDrag = AirAngularDrag;
        }
    }

    private void OnValidate()
    {
        transform.localScale = new Vector3(BaseScale, BaseScale, BaseScale);
    }

    public void UseAnimationBegin()
    {
        if (interactType == AlchemySystem.IngredientInteractType.Pour)
        {
            var pourable = GetComponent<PourableIngredient>();
            pourable.ConfigurePouringEffect(ingredientColor);
            pourable.BeginPour();
        }
    }

    public void UseAnimationEnd()
    {
        if (interactType == AlchemySystem.IngredientInteractType.Pour)
        {
            var pourable = GetComponent<PourableIngredient>();
            pourable.EndPour();
        }
    }

    public float GetProvidedAmount()
    {
        return IngredientAmount;
    }
}
