using System.Collections;
using System.Collections.Generic;
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


        //cast ray on CookingIngredient layer
        RaycastHit hit;

        float div = Screen.width / 640.0f;

        cutIndex = -1;

        Ray ray = cameraObject.GetComponent<PixelCamRaycast>().GetRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 1000f))
        {
            if (hit.collider != null)
            {
                if (hit.collider.gameObject.TryGetComponent<MovableObject>(out var movable))
                {
                    if (heldObject == null && Input.GetMouseButtonDown(0))
                    {
                        heldObject = movable;
                        heldObject.Pickup();
                    }
                }
            }
        }

        if (heldObject)
        {
            heldObject.SetTargetPosition(hit.point);

            if (Input.GetMouseButtonUp(0))
            {
                heldObject.Drop();
                heldObject = null;
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
}
