using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPCam : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform playerBody;

    private Camera thisCam;
    private float xRotation = 0f;

    //private GameObject oldGrabbableObject = null;
    private GameObject[] grabbableGameObjects;


    void Start()
    {
        thisCam = GetComponent<Camera>();
        Cursor.lockState = CursorLockMode.Locked;
        CheckGrabbableObjectOutline();
    }

    void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);

        HighlightGameObject();
    }

    public virtual void DisableOutlineFromGrabbable(GameObject[] pGrabbableGameObjects)
    {
        foreach (GameObject grabbable in pGrabbableGameObjects)
        {
            if (grabbable.GetComponent<Outline>() != null)
            {
                Outline grabbableOutline = grabbable.GetComponent<Outline>();
                grabbableOutline.enabled = false;
            }
        }
    }

    public virtual void CheckGrabbableObjectOutline()
    {
        GameObject[] localGrabbableGameObjects = GameObject.FindGameObjectsWithTag("GrabbableObject");
        grabbableGameObjects = localGrabbableGameObjects;

        foreach (GameObject grabbable in localGrabbableGameObjects)
        {
            if (grabbable.GetComponent<Outline>() == null) grabbable.AddComponent<Outline>();
            Outline grabbableOutline = grabbable.GetComponent<Outline>();

            grabbableOutline.enabled = false;
            grabbableOutline.OutlineColor = new Color(1.0f, 0.5f, 0.0f, 1.0f);
            grabbableOutline.OutlineWidth = 3.0f;
        }
    }

    public virtual void HighlightGameObject()
    {
        GameObject grabbableObjectTargeted = GetGrabbableWithRaycast();

        if (grabbableObjectTargeted != null)
        {
            Outline outlineTargeted = grabbableObjectTargeted.GetComponent<Outline>();

            outlineTargeted.enabled = true;
            //oldGrabbableObject = grabbableObjectTargeted;
        }
        else
        {

            DisableOutlineFromGrabbable(grabbableGameObjects);
        }

        //else
        //{
        //    if (oldGrabbableObject != null && oldGrabbableObject != grabbableObjectTargeted)
        //    {
        //        Outline oldOutline = oldGrabbableObject.GetComponent<Outline>();
        //        oldOutline.enabled = false;
        //    }
        //}
    }

    public virtual GameObject GetGrabbableWithRaycast() 
    {
        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        Ray ray = thisCam.ScreenPointToRay(screenCenter);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 3.0f))
        {
            GameObject grabbableObject = hit.collider.gameObject;

            if (grabbableObject != null && grabbableObject.CompareTag("GrabbableObject")) return grabbableObject;
            else return null;
        }
        else
        {
            return null;
        }
    }
}
