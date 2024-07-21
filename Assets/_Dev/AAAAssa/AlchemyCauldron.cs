using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlchemyCauldron : MonoBehaviour
{
    public List<PotionRecipe> potionRecipes;

    public List<PhysicalIngredient> physicalIngredients;

    public float[] ingredientAmounts = new float[Enum.GetNames(typeof(AlchemySystem.Ingredient)).Length];
    //public Color[] ingredientColours = new Color[Enum.GetNames(typeof(AlchemySystem.Ingredient)).Length];

    public float fluidMinY = 0.0f;
    public float fluidMaxY = 0.0f;
    public float fluidLevel = 0.0f;
    public float fullLevelContents = 1.0f; // the sum of contents to have fluidlevel be 1

    [SerializeField] private GameObject fluidPlane;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < ingredientAmounts.Length; i++)
        {
            ingredientAmounts[i] = 0;
        }
    }

    bool ValidatePotionRecipe(PotionRecipe recipe)
    {
        float grace = 0.05f;
        for (int i = 0; i < recipe.ingredientTypes.Count; i++)
        {
            if (ingredientAmounts[(int)recipe.ingredientTypes[i]] < (recipe.ingredientAmounts[i] - grace) )
            {
                return false;
            }
        }

        return true;
    }

    // Update is called once per frame
    void Update()
    {
        foreach (PotionRecipe recipe in  potionRecipes)
        {
            if (ValidatePotionRecipe(recipe)) 
            {
                Debug.Log("Valid Potion: " + recipe.PotionName);
            }
        }

        ConfigureFluid(); //temp
    }

    void ConfigureFluid()
    {
        float totalContents = 0.0f;
        foreach(float amount in ingredientAmounts)
        {
            totalContents += amount;
        }

        float progress = Mathf.Clamp01(totalContents / fullLevelContents);
        fluidPlane.transform.localPosition = new Vector3(
            fluidPlane.transform.localPosition.x,
            Mathf.Lerp(fluidMinY, fluidMaxY, progress),
            fluidPlane.transform.localPosition.z
        );

        //color

        Color color = Color.white;
        int count = 0;
        foreach(var ingredient in physicalIngredients)
        {
            color += ingredient.ingredientColor;
            count++;
        }
        if (count  > 0)
        {
            Color avgCol = color / (float)count;
        }
        fluidPlane.GetComponent<Renderer>().sharedMaterial.color = color;

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<PhysicalIngredient>(out PhysicalIngredient ingredient))
        {
            if (physicalIngredients.Contains(ingredient)) { return; }
            AddPhysicalIngredient(ingredient);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent<PhysicalIngredient>(out PhysicalIngredient ingredient))
        {
            RemovePhysicalIngredient(ingredient);
        }
    }

    public void AddPhysicalIngredient(PhysicalIngredient ingredient)
    {
        physicalIngredients.Add(ingredient);
        ingredientAmounts[(int)ingredient.ingredientType] += ingredient.GetProvidedAmount();
    }

    public void RemovePhysicalIngredient(PhysicalIngredient ingredient)
    {
        physicalIngredients.Remove(ingredient);
        ingredientAmounts[(int)ingredient.ingredientType] -= ingredient.GetProvidedAmount();

    }

    public void AddIngredient(AlchemySystem.Ingredient type, float amount)
    {
        ingredientAmounts[(int)type] += amount;
    }
    
}
