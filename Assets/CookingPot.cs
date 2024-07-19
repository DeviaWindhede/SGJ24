using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CookingPot : MonoBehaviour
{
    public List<PhysicalIngredient> ingredients = new List<PhysicalIngredient>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        PhysicalIngredient ingredient = other.GetComponent<PhysicalIngredient>();
        if (ingredient != null)
        {
            ingredients.Add(ingredient);
        }
    }

    void OnTriggerExit(Collider other)
    {
        PhysicalIngredient ingredient = other.GetComponent<PhysicalIngredient>();
        if (ingredient != null)
        {
            ingredients.Remove(ingredient);
        }
    }
}
