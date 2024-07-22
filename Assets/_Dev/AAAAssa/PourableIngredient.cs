using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PourableIngredient : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private GameObject cork;
    [SerializeField] private ParticleSystem pouringEffect;
    [SerializeField] private Vector3 pouringOffset;

    [SerializeField] private Quaternion pouringOrientation;

    [SerializeField] public Gradient lifeTimeGradient = new Gradient();
    [SerializeField] public Gradient speedGradient = new Gradient();

    private bool isPouring = false;
    private bool hadTargetOrientation = false;

    public AlchemyCauldron cauldronObject = null;

    void Start()
    {
           
    }

    bool OverCauldron(Vector3 pourPosition)
    {
        if (!cauldronObject) { return false; }

        Vector3 cauldronXZ = new Vector3(cauldronObject.transform.position.x, 0, cauldronObject.transform.position.z);
        Vector3 pourPositionXZ = new Vector3(pourPosition.x, 0, pourPosition.z);

        float distance = Vector3.Distance(cauldronXZ, pourPositionXZ);

        if (distance < 1.0f)
        {
            return true;
        }

        return false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isPouring)
        {
            Vector3 pourOrigin = transform.position + transform.TransformDirection(pouringOffset);
            pouringEffect.transform.position = pourOrigin;

            transform.rotation = Quaternion.Slerp(transform.rotation, pouringOrientation, Time.deltaTime * 2.0f);

            if (OverCauldron(pourOrigin))
            {
                if (TryGetComponent<CookingMortarPestle>(out var mortarPestle))
                {
                    Dictionary<AlchemySystem.Ingredient, float> provided = mortarPestle.GetPour(Time.deltaTime * 0.5f);
                    float sum = 0.0f;
                    foreach (var pair in provided)
                    {
                        sum += pair.Value;
                        cauldronObject.AddIngredient(pair.Key, pair.Value);
                    }
             
                }
                else
                {
                    PhysicalIngredient thisIngredient = GetComponent<PhysicalIngredient>();
                    cauldronObject.AddIngredient(thisIngredient.ingredientType, thisIngredient.IngredientAmount * Time.deltaTime);
                }
            }
        }
    }

    public void ConfigurePouringEffect(Color pourColor)
    {
        //pouringEffect.colorOverLifetime.color.gradient.colorKeys[0].color = pourColor;
        //pouringEffect.colorBySpeed.color.gradient.colorKeys[1].color      = pourColor;

        //{
        //    var lifeCol = pouringEffect.colorOverLifetime;
        //    Gradient grad = lifeCol.color.gradient;
        //    grad.colorKeys[0].color = pourColor;
        //    lifeCol.color = grad;
        //}
        //{
        //    var speedCol = pouringEffect.colorBySpeed;
        //    Gradient grad = speedCol.color.gradient;
        //    grad.colorKeys[1].color = pourColor;
        //    speedCol.color = grad;
        //}

        var lifeCol = pouringEffect.colorOverLifetime;
        lifeCol.color = lifeTimeGradient;

        var speedCol = pouringEffect.colorBySpeed;
        speedCol.color = speedGradient;

    }

    public void BeginPour()
    {
        pouringEffect.Play();
        isPouring = true;
        if (cork != null)
        {
            cork.SetActive(false);
        }

        var movableObject = GetComponent<MovableObject>();
        hadTargetOrientation = movableObject.hasTargetOrientation;
        movableObject.hasTargetOrientation = false;
    }

    public void EndPour()
    {
        pouringEffect.Stop();
        isPouring = false;
        if (cork != null)
        {
            cork.SetActive(true);
        }

        GetComponent<MovableObject>().hasTargetOrientation = hadTargetOrientation;
    }
}
