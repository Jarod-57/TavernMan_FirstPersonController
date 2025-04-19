using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;

namespace Invector.vCharacterController
{
    public class vThirdPersonController : vThirdPersonAnimator
    {
        public virtual void ControlAnimatorRootMotion()
        {
            if (!this.enabled) return;

            if (inputSmooth == Vector3.zero)
            {
                transform.position = animator.rootPosition;
                transform.rotation = animator.rootRotation;
            }

            if (useRootMotion)
                MoveCharacter(moveDirection);
        }

        public virtual void ControlLocomotionType()
        {
            if (lockMovement) return;

            /*if (locomotionType.Equals(LocomotionType.FreeWithStrafe) && !isStrafing || locomotionType.Equals(LocomotionType.OnlyFree))
            {
                isStrafing = true;
                SetControllerMoveSpeed(strafeSpeed);
                SetAnimatorMoveSpeed(strafeSpeed);
            }
            else if (locomotionType.Equals(LocomotionType.OnlyStrafe) || locomotionType.Equals(LocomotionType.FreeWithStrafe) && isStrafing)
            {
                isStrafing = true;
                SetControllerMoveSpeed(strafeSpeed);
                SetAnimatorMoveSpeed(strafeSpeed);
            }*/

            isStrafing = true;
            SetControllerMoveSpeed(strafeSpeed);
            SetAnimatorMoveSpeed(strafeSpeed);

            if (!useRootMotion)
                MoveCharacter(moveDirection);
        }

        public virtual void ControlRotationType()
        {
            // if (lockRotation) return;

            // bool validInput = input != Vector3.zero || (isStrafing ? strafeSpeed.rotateWithCamera : freeSpeed.rotateWithCamera);

            // if (validInput)
            // {
                // calculate input smooth
                inputSmooth = Vector3.Lerp(inputSmooth, input, (isStrafing ? strafeSpeed.movementSmooth : freeSpeed.movementSmooth) * Time.deltaTime);

                Vector3 dir = (isStrafing && (!isSprinting || sprintOnlyFree == false) || (freeSpeed.rotateWithCamera && input == Vector3.zero)) && rotateTarget ? rotateTarget.forward : moveDirection;
                RotateToDirection(dir);
            // }
        }

        public virtual void UpdateMoveDirection(Transform referenceTransform = null)
        {
            if (input.magnitude <= 0.01)
            {
                moveDirection = Vector3.Lerp(moveDirection, Vector3.zero, (isStrafing ? strafeSpeed.movementSmooth : freeSpeed.movementSmooth) * Time.deltaTime);
                return;
            }

            if (referenceTransform && !rotateByWorld)
            {
                //get the right-facing direction of the referenceTransform
                var right = referenceTransform.right;
                right.y = 0;
                //get the forward direction relative to referenceTransform Right
                var forward = Quaternion.AngleAxis(-90, Vector3.up) * right;
                // determine the direction the player will face based on input and the referenceTransform's right and forward directions
                moveDirection = (inputSmooth.x * right) + (inputSmooth.z * forward);
            }
            else
            {
                moveDirection = new Vector3(inputSmooth.x, 0, inputSmooth.z);
            }
        }

        public virtual void Sprint(bool value)
        {
            var sprintConditions = (input.sqrMagnitude > 0.1f && isGrounded &&
                !(isStrafing && !strafeSpeed.walkByDefault && (horizontalSpeed >= 0.5 || horizontalSpeed <= -0.5 || verticalSpeed <= 0.1f)));

            if (value && sprintConditions)
            {
                if (input.sqrMagnitude > 0.1f)
                {
                    if (isGrounded && useContinuousSprint)
                    {
                        isSprinting = !isSprinting;
                    }
                    else if (!isSprinting)
                    {
                        isSprinting = true;
                    }
                }
                else if (!useContinuousSprint && isSprinting)
                {
                    isSprinting = false;
                }
            }
            else if (isSprinting)
            {
                isSprinting = false;
            }
        }

        /*public virtual void Strafe()
        {
            isStrafing = !isStrafing;
        }*/

        public virtual void Jump()
        {
            // trigger jump behaviour
            jumpCounter = jumpTimer;
            isJumping = true;

            // trigger jump animations
            if (input.sqrMagnitude < 0.1f)
                animator.CrossFadeInFixedTime("Jump", 0.1f);
            else
                animator.CrossFadeInFixedTime("JumpMove", .2f);
        }

        #region Pick and Drop Controller

        public virtual FPCam GetPlayerCameraScript()
        {
            return gameObject.GetComponentInChildren<FPCam>(); 
        }

        public virtual void Grab()
        {
            // Red Right Hand
            RightHandAttach rightHandAttach = gameObject.GetComponentInChildren<RightHandAttach>();
            Transform rightHandAttachTransform = rightHandAttach.GetRightHandAttachTransform();

            // FPCam Script
            FPCam fpCam = GetPlayerCameraScript();

            if (!GetHandFull())
            {
                GameObject grabbableObject = fpCam.GetGrabbableWithRaycast();

                if (grabbableObject != null) AttachObjectToRightHand(grabbableObject, rightHandAttachTransform);
            }
            else
            {
                StartCoroutine(DropAndGrab());
            }
        }

        public virtual void AttachObjectToRightHand(GameObject pGrabbableObject, Transform pRightHandAttachTransform)
        {
            if (pGrabbableObject != null)
            {
                Rigidbody grabbableObjectRB = pGrabbableObject.GetComponent<Rigidbody>();

                pGrabbableObject.transform.SetParent(pRightHandAttachTransform);
                pGrabbableObject.transform.position = pRightHandAttachTransform.position;
                grabbableObjectRB.constraints = RigidbodyConstraints.FreezeAll;
            } 
        }

        public virtual void DetachObjectFromRightHand(GameObject pGrabbableObject)
        {
            if (pGrabbableObject != null)
            {
                Rigidbody grabbableObjectRB = pGrabbableObject.GetComponent<Rigidbody>();

                pGrabbableObject.transform.parent = null;
                grabbableObjectRB.constraints = RigidbodyConstraints.None;
            }
        }

        public virtual void Drop()
        {
            if (GetHandFull() == true)
            {
                RightHandAttach rightHandAttach = gameObject.GetComponentInChildren<RightHandAttach>();
                GameObject childGrabbableObject = rightHandAttach.GetGrabbableObjectInHand();

                DetachObjectFromRightHand(childGrabbableObject);
            }
        }

        public virtual IEnumerator DropAndGrab()
        {
            Drop();
            
            yield return new WaitForSeconds(0.2f);
            
            Grab();
        }

        public virtual bool GetHandFull()
        {
            RightHandAttach rightHandAttach = gameObject.GetComponentInChildren<RightHandAttach>();
            GameObject childGrabbableObject = rightHandAttach.GetGrabbableObjectInHand();

            if (childGrabbableObject != null) return true;
            else return false;
        }

        #endregion
    }
}