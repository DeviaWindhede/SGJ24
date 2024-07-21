using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(MeshCollider))]

public class MovableObject : MonoBehaviour
{
    public enum ObjectType
    {
        Tool,
        Ingredient,
        Cuttable,
    }

    public enum MovementType
    {
        Kinematic,
        Velocity,
    }

    public ObjectType type;
    public MovementType moveType = MovementType.Velocity;
    public MovementType assignedMoveType = MovementType.Kinematic;

    public bool hasTargetOrientation = false;
    public Quaternion targetOrientation = Quaternion.identity;

    private Vector3 targetPosition = Vector3.zero;
    public Vector3 positionOffset = new Vector3(0.0f, 0.5f, 0.0f);
    public Vector3 localOffset = new Vector3(0.0f, 0.0f, 0.0f);
    private bool pickedUp = false;

    public LayerMask carriedLayerMaskExcludes;
    public LayerMask droppedLayerMaskExcludes;

    public bool kinematicOnStart = false;
    public bool kinematicOnPickup = false;
    public bool kinematicOnDrop = false;

    private int originalLayer;
    private List<int> childLayers = new List<int>();

    AlchemyInteractArea assignedArea = null;

    // Start is called before the first frame update
    void Start()
    {   
    }

    // Update is called once per frame
    void Update()
    {
        if (pickedUp)
        {

            if (moveType == MovementType.Kinematic)
            {
                transform.position = Vector3.Lerp(transform.position, targetPosition + positionOffset + transform.TransformDirection(localOffset), Time.deltaTime * 2.0f);
            }
            else if (moveType == MovementType.Velocity)
            {
                GetComponent<Rigidbody>().velocity = ((targetPosition + positionOffset + transform.TransformDirection(localOffset) - transform.position) * 2.0f);
            }

            if (hasTargetOrientation)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetOrientation, Time.deltaTime * 2.0f);
            }
        }
        else if (assignedArea != null && assignedArea.validWhenDropped)
        {
            bool pourable = type == MovableObject.ObjectType.Ingredient && GetComponent<PhysicalIngredient>().interactType == AlchemySystem.IngredientInteractType.Pour;
            targetPosition = assignedArea.GetTargetWorldPos(pourable);

            if (assignedMoveType == MovementType.Kinematic)
            {
                transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 2.0f);
            }
            else if (assignedMoveType == MovementType.Velocity)
            {
                GetComponent<Rigidbody>().velocity = ((targetPosition - transform.position) * 2.0f);
            }

            if (assignedArea.customPlacementOrientation)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, assignedArea.placementOrientation, Time.deltaTime * 2.0f);
            }
        }
    }

    public void AssignToArea(AlchemyInteractArea area)
    {
        assignedArea = area;

        var rigidBody = GetComponent<Rigidbody>();
        if (assignedArea) { rigidBody.isKinematic = assignedMoveType == MovementType.Kinematic; }

    }

    private void CacheLayer()
    {
        originalLayer = gameObject.layer;
        for (int i = 0; i < transform.childCount; i++)
        {
            childLayers.Add(transform.GetChild(i).gameObject.layer);
        }
    }
    private void RestoreLayer()
    {
        gameObject.layer = originalLayer;
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.layer = childLayers[i];
        }
    }

    private void ChangeLayer(int newLayer)
    {
        gameObject.layer = newLayer;
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.layer = newLayer;
        }
    }

    public void Pickup()
    {
        pickedUp = true;
        var rigidBody = GetComponent<Rigidbody>();

        rigidBody.isKinematic = kinematicOnPickup;
        rigidBody.excludeLayers = carriedLayerMaskExcludes;


        CacheLayer();
        ChangeLayer(11);

    }

    public void Drop()
    {
        pickedUp = false;
        var rigidBody = GetComponent<Rigidbody>();

        rigidBody.isKinematic = kinematicOnDrop;
        rigidBody.excludeLayers = droppedLayerMaskExcludes;

        if (assignedArea) { rigidBody.isKinematic = assignedMoveType == MovementType.Kinematic; }

        RestoreLayer();

        EndUse();

        
    }

    public void BeginUse()
    {

        if (type == ObjectType.Tool)
        {
            GetComponent<CookingTool>().UseAnimation();
        }
        else if (type == ObjectType.Ingredient)
        {
            GetComponent<PhysicalIngredient>().UseAnimationBegin();
        }
        else if (type == ObjectType.Cuttable)
        {
            // Do cuttable stuff
        }
    }

    public void EndUse()
    {
        if (type == ObjectType.Tool)
        {
           
        }
        else if (type == ObjectType.Ingredient)
        {
            GetComponent<PhysicalIngredient>().UseAnimationEnd();
        }
        else if (type == ObjectType.Cuttable)
        {
            // Do cuttable stuff
        }
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
