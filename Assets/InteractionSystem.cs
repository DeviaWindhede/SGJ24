using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;


public class InteractionSystem : MonoBehaviour
{
    PhysicalIngredient targetedIgredient;
    CookingTool targetedTool;

    Vector3 hitCookingGeometry;

    Vector3 toolTargetPosition;
    Quaternion toolTargetOrientation;

    CuttableIngredient cuttableIngredient;
    int cutIndex = 0;
    Vector3 hitCutPoint;

    MovableObject heldObject = null;

    public enum CameraMode
    {
        Main,
        Cutting,
        Grinding,
        Stirring,
        Recipes,

    }
    [SerializeField] private GameObject cameraObject;
    public List<GameObject> cameraModes = new List<GameObject>();

    public CameraMode cameraMode = CameraMode.Main;

    public float cameraTransitionSpeed = 2.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    //void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawSphere(hitCookingGeometry, 0.1f);
    //}

    // Update is called once per frame
    bool TargetingCuttableSeam(MovableObject targetedObject, Vector3 hitPosition, out Vector3 targetPosition, out int targetSeamIndex)
    {
        if (targetedObject != null && targetedObject.type == MovableObject.ObjectType.Cuttable)
        {
            CuttableIngredient cuttable = targetedObject.GetComponent<CuttableIngredient>();
            int cutIndex = cuttable.GetCutPoint(hitPosition, out Vector3 outHitCutPoint);
            if (cutIndex >= 0)
            {
                targetSeamIndex = cutIndex;
                targetPosition = outHitCutPoint;
                return true;
            }

        }

        targetSeamIndex = -1;
        targetPosition = new Vector3();
        return false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            cameraMode++;
            if ((int)cameraMode >= System.Enum.GetValues(typeof(CameraMode)).Length)
            {
                cameraMode = 0;
            }
        }

        int cameraModeIndex = (int)cameraMode;
        if (cameraModes.Count >= cameraModeIndex)
        {
            Vector3 currentPos = cameraObject.transform.position;
            Vector3 targetPos = cameraModes[cameraModeIndex].transform.position;

            Quaternion currentOrientation = cameraObject.transform.rotation;
            Quaternion targetOrientation = cameraModes[cameraModeIndex].transform.rotation;

            cameraObject.transform.position = Vector3.Lerp(currentPos, targetPos, Time.deltaTime * cameraTransitionSpeed);
            cameraObject.transform.rotation = Quaternion.Slerp(currentOrientation, targetOrientation, Time.deltaTime * cameraTransitionSpeed);
        }

        Vector3 target = Vector3.zero;


        LayerMask interactMask = LayerMask.GetMask("CookingGeometry", "CookingIngredient", "CookingTool", "CuttableObject");
        if (heldObject != null && heldObject.type != MovableObject.ObjectType.Tool) { interactMask |= LayerMask.GetMask("AlchemyInteractArea"); }

        MovableObject hitMovableObject = null;

        Ray ray = cameraObject.GetComponent<PixelCamRaycast>().GetRay(Input.mousePosition);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000f, interactMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider != null)
            {
                target = hit.point;

                if (hit.collider.gameObject.TryGetComponent<MovableObject>(out var movable))
                {
                    hitMovableObject = movable;
                }
                else 
                {
                    MovableObject parentMovable = hit.collider.gameObject.GetComponentInParent<MovableObject>();
                    if (parentMovable != null)
                    {
                        hitMovableObject = parentMovable;
                    }
                }
                if (hitMovableObject != null && heldObject == null && Input.GetMouseButtonDown(0))
                {
                    heldObject = hitMovableObject;
                    heldObject.Pickup();
                }

                if (hit.collider.gameObject.TryGetComponent<AlchemyInteractArea>(out var interactArea))
                {                   
                    if (heldObject && interactArea.AcceptsObject(heldObject)) 
                    {
                        bool pourable = heldObject.type == MovableObject.ObjectType.Ingredient && heldObject.GetComponent<PhysicalIngredient>().interactType == AlchemySystem.IngredientInteractType.Pour;

                        target = interactArea.GetTargetWorldPos(pourable);
                        heldObject.AssignToArea(interactArea);
                    }
                }
                else
                {
                    if (heldObject) { heldObject.AssignToArea(null); }
                }
            }
        }

        if (heldObject)
        {
            heldObject.SetTargetPosition(target);

            TryCutConditions(hitMovableObject, hit);

            if (Input.GetMouseButtonUp(0))
            {
                //special case for mortar:
                if (heldObject.type == MovableObject.ObjectType.Mortar)
                {
                    if (heldObject.assignedArea != null)
                    {
                        heldObject.GetComponent<CookingMortarPestle>().SwitchState(CookingMortarPestle.MortarState.Returning);
                        heldObject.Drop();
                        heldObject = null;
                    }
                }
                else
                {
                    heldObject.Drop();
                    heldObject = null;
                }
            }

            

            if (heldObject && heldObject.type == MovableObject.ObjectType.Mortar)
            {
                if (heldObject.TryGetComponent<CookingMortarPestle>(out var mortarPestle))
                {
                    bool empty = mortarPestle.IsEmpty();

                    if (!empty && Input.GetMouseButtonDown(1))
                    {
                        heldObject.BeginUse();
                    }

                    if (empty || Input.GetMouseButtonUp(1))
                    {
                        heldObject.EndUse();
                    }

                }

            }
            else
            {
                if (Input.GetMouseButtonDown(1))
                {
                    if (heldObject)
                    {
                        heldObject.BeginUse();
                    }
                }

                if (Input.GetMouseButtonUp(1))
                {
                    if (heldObject)
                    {
                        heldObject.EndUse();
                    }
                }
            }
        }



        //Ray ray = cameraObject.GetComponent<PixelCamRaycast>().GetRay(Input.mousePosition);
        //if (Physics.Raycast(ray, out hit, 1000f, LayerMask.GetMask("CookingIngredient")))
        //{
        //    if (hit.collider != null)
        //    {
        //        if (Input.GetMouseButtonDown(0))
        //        {
        //            //get the PhysicalIngredient component of the hit object
        //            targetedIgredient = hit.collider.gameObject.GetComponent<PhysicalIngredient>();
        //        }
        //    }
        //}
        //if (Physics.Raycast(ray, out hit, 1000f, LayerMask.GetMask("CuttableObject")))
        //{
        //    if (hit.collider != null)
        //    {
        //        cuttableIngredient = hit.collider.gameObject.GetComponent<CuttableIngredient>();
        //        cutIndex = cuttableIngredient.GetCutPoint(hit.point, out Vector3 outHitCutPoint);
        //        hitCutPoint = outHitCutPoint;
        //    }
        //}
        //else if (Physics.Raycast(ray, out hit, 1000f, LayerMask.GetMask("CookingTool")))
        //{
        //    if (hit.collider != null)
        //    {
        //        if (Input.GetMouseButtonDown(0))
        //        {
        //            //get the CookingTool component of the hit object
        //            targetedTool = hit.collider.gameObject.GetComponent<CookingTool>();
        //            targetedTool.GetComponent<Rigidbody>().isKinematic = true;
        //            targetedTool.GetComponent<MeshCollider>().enabled = false;
        //        }
        //    }
        //}
        //
        ////if ray didn't hit any CookingIngredient
        //if (!Input.GetMouseButton(0))
        //{
        //    targetedIgredient = null;
        //    if (targetedTool)
        //    {
        //        targetedTool.GetComponent<Rigidbody>().isKinematic = false;
        //        targetedTool.GetComponent<MeshCollider>().enabled = true;
        //    }
        //    targetedTool = null;
        //
        //}
        //
        //
        //if (Physics.Raycast(ray, out hit, 1000f, LayerMask.GetMask("CookingGeometry")))
        //{
        //    hitCookingGeometry = hit.point + new Vector3(0.0f, 1.0f, 0.0f);
        //    toolTargetPosition = hit.point + new Vector3(0.0f, 0.25f, 0.0f);
        //
        //}
        //
        //
        //if (targetedIgredient != null)
        //{
        //    //give targetedIngredient velocity towards hitCookingGeometry
        //
        //    targetedIgredient.GetComponent<Rigidbody>().velocity = ((hitCookingGeometry - targetedIgredient.transform.position) * 2.0f);
        //}
        //if (targetedTool != null)
        //{
        //    Vector3 finalToolTarget = cutIndex == -1 ? toolTargetPosition : hitCutPoint + new Vector3(0.0f, 0.45f, 0.0f);
        //
        //    targetedTool.transform.position = Vector3.Lerp(targetedTool.transform.position, finalToolTarget, Time.deltaTime * 2.0f);
        //    targetedTool.transform.rotation = Quaternion.Slerp(targetedTool.transform.rotation, targetedTool.TargetOrientation, Time.deltaTime * 2.0f);
        //
        //    //targetedTool.GetComponent<Rigidbody>().rotation = targetedTool.TargetOrientation;
        //    //targetedTool.GetComponent<Rigidbody>().rotation = Quaternion.Slerp(targetedTool.GetComponent<Rigidbody>().rotation, targetedTool.TargetOrientation, Time.deltaTime * 2.0f);
        //
        //    if (Input.GetMouseButtonDown(1))
        //    {
        //        if (cuttableIngredient)
        //        {
        //            cuttableIngredient.CutAtSeam(cutIndex);
        //        }
        //        targetedTool.transform.Rotate(Vector3.up, 30.0f, Space.Self);// Quaternion.Euler(35.0f, 0.0f, 0.0f);
        //    }
        //}
        //else
        //{
        //    if (cuttableIngredient)
        //    {
        //
        //    }
        //}

    }

    private void TryCutConditions(MovableObject hitMovableObject, RaycastHit hit)
    {
        if (!heldObject) { return; }
        if (!hitMovableObject) { return; }

        if (heldObject.type == MovableObject.ObjectType.Tool)
        {
            CookingTool heldTool = heldObject.GetComponent<CookingTool>();
            if (heldTool.toolType == CookingToolType.Knife)
            {
                if (TargetingCuttableSeam(hitMovableObject, hit.point, out Vector3 targetPosition, out int targetSeamIndex))
                {
                    heldObject.SetTargetPosition(targetPosition);
                    heldTool.KnifeAssignCuttable(targetSeamIndex, hitMovableObject.GetComponent<CuttableIngredient>());
                }
            }
        }
    }
}

