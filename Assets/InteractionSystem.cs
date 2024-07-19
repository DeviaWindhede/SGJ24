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


    public enum CameraMode
    {
        Main,
        Cutting,
        Grinding,
        Stirring,

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

        Ray ray = cameraObject.GetComponent<PixelCamRaycast>().GetRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 1000f, LayerMask.GetMask("CookingIngredient")))
        {
            Debug.Log(hit);
            //if ray hit a CookingIngredient
            if (hit.collider != null)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    //get the PhysicalIngredient component of the hit object
                    targetedIgredient = hit.collider.gameObject.GetComponent<PhysicalIngredient>();
                }
            }
        }
        else if (Physics.Raycast(ray, out hit, 1000f, LayerMask.GetMask("CookingTool")))
        {
            Debug.Log(hit);

            //if ray hit a CookingTool
            if (hit.collider != null)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    //get the CookingTool component of the hit object
                    targetedTool = hit.collider.gameObject.GetComponent<CookingTool>();
                }
            }
        }
        else
        {
            //if ray didn't hit any CookingIngredient
            if (!Input.GetMouseButton(0))
            {
                targetedIgredient = null;
                targetedTool = null;
            }
        }

        
        if (Physics.Raycast(ray, out hit, 1000f, LayerMask.GetMask("CookingGeometry")))
        {
            hitCookingGeometry = hit.point + new Vector3(0.0f, 1.0f, 0.0f);
        }


        if (targetedIgredient != null)
        {
            //give targetedIngredient velocity towards hitCookingGeometry

            targetedIgredient.GetComponent<Rigidbody>().velocity = ((hitCookingGeometry - targetedIgredient.transform.position) * 2.0f);
        }
        if (targetedTool != null)
        {
            //give targetedIngredient velocity towards hitCookingGeometry

            targetedTool.GetComponent<Rigidbody>().velocity = ((hitCookingGeometry - targetedTool.transform.position) * 2.0f);
            targetedTool.GetComponent<Rigidbody>().rotation = Quaternion.Slerp(targetedTool.GetComponent<Rigidbody>().rotation, targetedTool.TargetOrientation, Time.deltaTime * 2.0f);
        }

    }
}
