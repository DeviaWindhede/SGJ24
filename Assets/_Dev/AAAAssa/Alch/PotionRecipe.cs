using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using System;

[CreateAssetMenu(fileName = "CustomItem", menuName = "ScriptableObjects/PotionRecipe", order = 1)]
public class PotionRecipe : ScriptableObject
{
    public string PotionName;
    public List<AlchemySystem.Ingredient> ingredientTypes;
    public List<float> ingredientAmounts;

    void OnValidate()
    {
       
    }

}
