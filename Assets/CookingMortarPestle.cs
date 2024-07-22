using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class CookingMortarPestle : MonoBehaviour
{
    public List<PhysicalIngredient> ingredients = new List<PhysicalIngredient>();
    public Dictionary<PhysicalIngredient, float> grindProgresses = new Dictionary<PhysicalIngredient, float>();
    
    public float[] ingredientAmounts = new float[Enum.GetNames(typeof(AlchemySystem.Ingredient)).Length];

    [SerializeField] Color ingredientColor;
    [SerializeField] float grindProgress = 0;

    [SerializeField] float minGrindScale = 0.5f;
    [SerializeField] float maxGrindScale = 1.75f;
    [SerializeField] float minGrindY = 0.0f;
    [SerializeField] float maxGrindY = -0.035f;
    [SerializeField] float grindMaxClamp = 10.0f;

    [SerializeField] GameObject dustObject;
    [SerializeField] private MovableObject pestle;
    [SerializeField] GameObject interactArea;


    public enum MortarState
    {
        Idle,
        Carry,
        Grind,
        Returning
    }
    public MortarState mortarState = MortarState.Grind;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        MovableObject mov = GetComponent<MovableObject>();
        if (mortarState != MortarState.Carry)
        {
            if (mov.pickedUp)
            {
                SwitchState(MortarState.Carry);
            }
        }

        if (mortarState == MortarState.Returning && mov.assignedArea != null)
        {
            Vector3 targetPosition = mov.assignedArea.GetTargetWorldPos(false);
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 4.0f);
            if (Vector3.Distance(targetPosition, transform.position) < 0.1f) 
            {
                transform.position = targetPosition;
                SwitchState(MortarState.Idle);
                transform.rotation = mov.assignedArea.placementOrientation;
            }
        }
    }

    public void SwitchState(MortarState state)
    {
        mortarState = state;

        if (state == MortarState.Carry)
        {
            //make pestle kinematic
            pestle.GetComponent<Rigidbody>().isKinematic = true;
            interactArea.SetActive(false);
        }
        else if (state == MortarState.Grind)
        {
            interactArea.SetActive(false);
        }
        else
        {
            pestle.GetComponent<Rigidbody>().isKinematic = false;
            interactArea.SetActive(true);
        }
    }

    public void UpdateGrindProgress(float angleDiff)
    {
        if (mortarState != MortarState.Grind) { return; }
        if (ingredients.Count == 0) { grindProgress = 0.0f; return; }

        angleDiff = Mathf.Clamp(angleDiff, 0, grindMaxClamp);
        //let's say it takes 3 full rotations to grind the ingredients
        float diffAdd = angleDiff / (1080 * 3.0f);

        for (int i = 0; i < ingredients.Count; i++)
        {
            Vector3 scale = ingredients[i].transform.localScale;
            if (scale.x <= 0.05f) { continue; }

            float grindProgress = grindProgresses[ingredients[i]];
            float grindAmount = Mathf.Clamp01(grindProgress + diffAdd);
            grindProgresses[ingredients[i]] = grindAmount;
        }



        ConfigureDust();
    }

    public bool IsEmpty()
    {
        float sum = 0.0f;
        foreach (var pair in grindProgresses)
        {
            sum += pair.Value * pair.Key.IngredientAmount;
        }
        return sum <= 0.0f;
    }

    void ConfigureDust()
    {
        if (ingredients.Count == 0) { return; }

        //blend the color of the dust to the average color of the ingredients
        Color dustColor = ingredientColor;
        if (ingredients.Count > 0)
        {
            Color averageColor = Color.black;
            foreach (PhysicalIngredient ingredient in ingredients)
            {
                averageColor += ingredient.ingredientColor;
            }
            averageColor /= ingredients.Count;
            dustColor = averageColor;
        }

        float totalProgress = 0.0f;
        int count = 0;
        for (int i = 0; i < ingredients.Count; i++)
        {
            Vector3 currScale = ingredients[i].transform.localScale;

            if (currScale.x <= 0.05f && grindProgresses[ingredients[i]] < 0.5f) { continue; }

            totalProgress += grindProgresses[ingredients[i]];
            float s = Mathf.Lerp(ingredients[i].BaseScale, 0.05f, grindProgresses[ingredients[i]]);

            ingredients[i].transform.localScale = Vector3.Min(currScale, new Vector3(s, s, s));
            count++;
        }
        totalProgress /= Mathf.Max(count, 1.0f);

        dustObject.transform.localPosition = new Vector3(0, Mathf.Lerp(minGrindY, maxGrindY, totalProgress), 0);
        float scale = Mathf.Lerp(minGrindScale, maxGrindScale, totalProgress);
        dustObject.transform.localScale = new Vector3(scale, scale, scale);

        dustObject.transform.GetChild(0).gameObject.GetComponent<Renderer>().sharedMaterial.color = dustColor;
        dustObject.transform.GetChild(1).gameObject.GetComponent<Renderer>().sharedMaterial.color = dustColor;

        var gradient = new Gradient();

        // Blend color from red at 0% to blue at 100%
        var colors = new GradientColorKey[2];
        colors[0] = new GradientColorKey(dustColor, 0.0f);
        colors[1] = new GradientColorKey(dustColor, 1.0f);
        gradient.colorKeys = colors;

        GetComponent<PourableIngredient>().lifeTimeGradient = gradient;
        GetComponent<PourableIngredient>().speedGradient = gradient;
    }

    public Dictionary<AlchemySystem.Ingredient, float> GetPour(float pourAmount)
    {
        Dictionary<AlchemySystem.Ingredient, float> pourOut = new Dictionary<AlchemySystem.Ingredient, float>();
        float remainingAmount = pourAmount;

        for (int i = 0; i < ingredients.Count; i++)
        {
            if (!pourOut.ContainsKey(ingredients[i].ingredientType)) { pourOut.Add(ingredients[i].ingredientType, 0.0f); } 
            float availableAmount = grindProgresses[ingredients[i]] * ingredients[i].IngredientAmount;
            float providedAmount = Mathf.Min(availableAmount, remainingAmount);
            pourOut[ingredients[i].ingredientType] += providedAmount;
            remainingAmount -= providedAmount;
            grindProgresses[ingredients[i]] -= providedAmount / ingredients[i].IngredientAmount;

            if (remainingAmount <= 0.0f) { break; }
        }

        ConfigureDust();
        return pourOut;
    }

    void OnTriggerEnter(Collider other)
    {
        PhysicalIngredient ingredient = other.GetComponent<PhysicalIngredient>();
        if (ingredient != null)
        {
            ingredients.Add(ingredient);
            grindProgresses[ingredient] = 0.0f;
        }
        ConfigureDust();

    }

    void OnTriggerExit(Collider other)
    {
        PhysicalIngredient ingredient = other.GetComponent<PhysicalIngredient>();
        if (ingredient != null)
        {
            if (grindProgresses[ingredient] > 0.1f) { return; }

            ingredients.Remove(ingredient);
            grindProgresses.Remove(ingredient);
        }
        ConfigureDust();

    }

    void OnValidate()
    {
        ConfigureDust();
    }
}
