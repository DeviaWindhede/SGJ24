using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlchemyCauldron : MonoBehaviour
{

    public List<PhysicalIngredient> physicalIngredients;

    public float[] ingredientAmounts = new float[Enum.GetNames(typeof(AlchemySystem.Ingredient)).Length];
    //public Color[] ingredientColours = new Color[Enum.GetNames(typeof(AlchemySystem.Ingredient)).Length];

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < ingredientAmounts.Length; i++)
        {
            ingredientAmounts[i] = 0;
        }


    }

    // Update is called once per frame
    void Update()
    {
        
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
