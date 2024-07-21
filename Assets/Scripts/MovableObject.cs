using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableObject : MonoBehaviour
{
    enum ObjectType
    {
        Tool,
        Ingredient,
        Cuttable,
    }

    public bool hasTargetOrientation = false;
    public Quaternion targetOrientation = Quaternion.identity;

    private Vector3 targetPosition = Vector3.zero;
    private bool pickedUp = false;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (pickedUp)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 2.0f);
            if (hasTargetOrientation)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetOrientation, Time.deltaTime * 2.0f);
            }
        }
    }

    public void Pickup()
    {
        pickedUp = true;
    }

    public void Drop()
    {
        pickedUp = false;
    }

    public void SetTargetPosition(Vector3 position)
    {
        targetPosition = position;
    }

}
