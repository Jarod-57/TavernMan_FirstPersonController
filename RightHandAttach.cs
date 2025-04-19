using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightHandAttach : MonoBehaviour
{
    public GameObject GetRightHandAttachGameObject()
    {
        return this.gameObject;
    }

    public Transform GetRightHandAttachTransform()
    {
        return this.transform;
    }

    public GameObject GetGrabbableObjectInHand()
    {
        GameObject childGrabbable;

        if (transform.childCount > 0) childGrabbable = gameObject.transform.GetChild(0).gameObject;
        else childGrabbable = null;

        if (childGrabbable != null && childGrabbable.CompareTag("GrabbableObject")) return childGrabbable;
        else return null;
    }
}
