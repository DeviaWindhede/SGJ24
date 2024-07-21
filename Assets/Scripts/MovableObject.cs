using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(MeshCollider))]

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
    public Vector3 positionOffset = new Vector3(0.0f, 0.5f, 0.0f);
    private bool pickedUp = false;

    public LayerMask carriedLayerMaskExcludes;
    public LayerMask droppedLayerMaskExcludes;

    public bool kinematicOnStart = false;
    public bool kinematicOnPickup = false;
    public bool kinematicOnDrop = false;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (pickedUp)
        {
            //transform.position = Vector3.Lerp(transform.position, targetPosition + positionOffset, Time.deltaTime * 2.0f);
            GetComponent<Rigidbody>().velocity = ((targetPosition + positionOffset - transform.position) * 2.0f);
            if (hasTargetOrientation)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetOrientation, Time.deltaTime * 2.0f);
            }
        }
    }

    public void Pickup()
    {
        pickedUp = true;
        var rigidBody = GetComponent<Rigidbody>();

        rigidBody.isKinematic = kinematicOnPickup;
        rigidBody.excludeLayers = carriedLayerMaskExcludes;

    }

    public void Drop()
    {
        pickedUp = false;
        var rigidBody = GetComponent<Rigidbody>();

        rigidBody.isKinematic = kinematicOnDrop;
        rigidBody.excludeLayers = droppedLayerMaskExcludes;
    }

    public void SetTargetPosition(Vector3 position)
    {
        targetPosition = position;
    }

    void OnValidate()
    {
        var rigidBody = GetComponent<Rigidbody>();

        rigidBody.isKinematic = kinematicOnStart;
        rigidBody.excludeLayers = droppedLayerMaskExcludes;
    }

}
