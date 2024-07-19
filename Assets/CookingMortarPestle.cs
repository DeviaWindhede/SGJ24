using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CookingMortarPestle : MonoBehaviour
{
    public List<PhysicalIngredient> ingredients = new List<PhysicalIngredient>();

    [SerializeField] Color ingredientColor;
    [SerializeField] float grindProgress = 0;

    [SerializeField] float minGrindScale = 0.5f;
    [SerializeField] float maxGrindScale = 1.75f;
    [SerializeField] float minGrindY = 0.0f;
    [SerializeField] float maxGrindY = -0.035f;

    [SerializeField] GameObject dustObject;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ConfigureDust()
    {
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


        dustObject.transform.localPosition = new Vector3(0, Mathf.Lerp(minGrindY, maxGrindY, grindProgress), 0);
        float scale = Mathf.Lerp(minGrindScale, maxGrindScale, grindProgress);
        dustObject.transform.localScale = new Vector3(scale, scale, scale);

        dustObject.transform.GetChild(0).gameObject.GetComponent<Renderer>().sharedMaterial.color = dustColor;
        dustObject.transform.GetChild(1).gameObject.GetComponent<Renderer>().sharedMaterial.color = dustColor;

        foreach (PhysicalIngredient ingredient in ingredients)
        {
            float s = Mathf.Lerp(ingredient.BaseScale, 0.1f, grindProgress);
            ingredient.transform.localScale = new Vector3(s, s, s);
        }

    }

    void OnTriggerEnter(Collider other)
    {
        PhysicalIngredient ingredient = other.GetComponent<PhysicalIngredient>();
        if (ingredient != null)
        {
            ingredients.Add(ingredient);
        }
        ConfigureDust();

    }

    void OnTriggerExit(Collider other)
    {
        PhysicalIngredient ingredient = other.GetComponent<PhysicalIngredient>();
        if (ingredient != null)
        {
            ingredients.Remove(ingredient);
        }
        ConfigureDust();

    }

    void OnValidate()
    {
        ConfigureDust();
    }
}
