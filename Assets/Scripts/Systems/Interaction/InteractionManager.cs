using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class InteractionManager : MonoBehaviour
{
    public enum Interactions
    {
        PickUpToy = 0,
        ThrowToy,
        PickUpFirewood,
        ThrowFirewood,
        CloseWindows,
        FlushToilet,
        CloseSkylightWithRemote,
        CloseBasementHatch,
    }

    private float interactionDistance = 1.6f;
    private float objectLockDistance = 1.2f;
    private float interactableMovementSpeed = 20.0f;

    public LayerMask interactableLayer;
    public LayerMask obstacleLayer;

    private PlayerController playerController = null;
    private Camera playerCamera = null;

    private GameObject interactableObject;
    private Rigidbody interactableObjRigidBody;

    public void OnLocalPlayerSetup(PlayerController targetController, Camera targetCamera)
    {
       playerController = targetController;
       playerCamera = targetCamera;
    }
    public void OnLocalPlayerUpdate()
    {

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            if (hit.collider != null)
                 OnObjectTraceCollide(hit.transform.gameObject, hit);
        }

        if (interactableObject != null)
        {
            if (interactableObject.IsDestroyed())
            {
                interactableObject = null;
                interactableObjRigidBody = null;
            }
            else if (!playerController.playerControlActions.Player.Interact.IsPressed())
            {
                interactableObject = null;
                if (interactableObjRigidBody)
                {
                    interactableObjRigidBody.useGravity = true;
                    interactableObjRigidBody.constraints = RigidbodyConstraints.None;
                    interactableObjRigidBody = null;
                }
            }
            else
            {
                if (interactableObjRigidBody)
                {
                    Vector3 endPoint = playerCamera.transform.position + (playerCamera.transform.forward * objectLockDistance);
                    Vector3 direction = (endPoint - interactableObjRigidBody.position).normalized;
                    float distance = Vector3.Distance(interactableObjRigidBody.position, endPoint);

                    if (!Physics.Raycast(playerCamera.transform.position, direction, distance, obstacleLayer))
                    {
                        Vector3 targetVelocity = direction * (distance * interactableMovementSpeed);
                        interactableObjRigidBody.velocity = Vector3.Lerp(interactableObjRigidBody.velocity, targetVelocity, Time.deltaTime * interactableMovementSpeed);
                        interactableObjRigidBody.velocity *= 0.95f;
                        interactableObjRigidBody.angularVelocity *= 0.9f;
                    }
                    else
                        Debug.Log("Failed to hold object");

                }
                else
                    interactableObject.transform.position = playerCamera.transform.position + playerCamera.transform.forward;
            }
        }
    }

    private void OnObjectTraceCollide(GameObject gameObj, RaycastHit hit)
    {
        bool interactTriggered = playerController.playerControlActions.Player.Interact.triggered;
        if (gameObj.tag.Contains("Interactable_"))
        {
            if (interactTriggered)
            {
                Debug.Log("Interactable Object Found: " + gameObj.name);
                interactableObject = gameObj;
                interactableObjRigidBody = interactableObject.GetComponent<Rigidbody>();
                interactableObjRigidBody.useGravity = false;
                interactableObjRigidBody.interpolation = RigidbodyInterpolation.Interpolate;
                interactableObjRigidBody.constraints = RigidbodyConstraints.FreezeRotation;
                interactableObjRigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            }
        }

        if (gameObj.GetComponent<WindowsActivity>() != null)
        {
            if (interactTriggered)
            {
                gameObj.GetComponent<WindowsActivity>().ResetActivity();
            }
        }

        if (gameObj.tag.Contains("BasementHatch_Door"))
        {
            if (interactTriggered)
            {
                FindObjectOfType<BasementHatch>().ResetActivity();
            }
        }

        if (gameObj.tag.Contains("Skylight_Remote"))
        {
            if (interactTriggered)
            {
                FindObjectOfType<SkylightActivity>().ResetActivity();
            }
        }

        if (gameObj.tag.Contains("Toilet_Flush"))
        {
            if (interactTriggered)
            {
                FindObjectOfType<ToiletActivity>().ResetActivity();
            }
        }
    }
}
