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
         //   if (((1 << hit.collider.gameObject.layer) & interactableLayer) != 0)
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

                    if (!Physics.Raycast(interactableObjRigidBody.position, direction, distance, obstacleLayer))
                    {
                        Vector3 targetVelocity = direction * (distance * interactableMovementSpeed);
                        interactableObjRigidBody.velocity = Vector3.Lerp(interactableObjRigidBody.velocity, targetVelocity, Time.deltaTime * interactableMovementSpeed);
                        interactableObjRigidBody.velocity *= 0.95f;
                        interactableObjRigidBody.angularVelocity *= 0.9f; 
                    }
                }
                else
                    interactableObject.transform.position = playerCamera.transform.position + playerCamera.transform.forward;
            }
        }
    }

    private void OnObjectTraceCollide(GameObject gameObj, RaycastHit hit)
    {
        if (gameObj.tag.Contains("Interactable_"))
        {
            Debug.Log("Interactable Object Found: " + gameObj.name);
            if (playerController.playerControlActions.Player.Interact.triggered)
            {
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
            Debug.Log("Activity Object Found: " + gameObj.name);
            if (playerController.playerControlActions.Player.Interact.triggered)
            {
                gameObj.GetComponent<WindowsActivity>().ResetActivity();
                Debug.Log("Activity Reset");
            }
        }

        Debug.Log("Object in sight");
    }

    void TriggerEvent(Interactions action)
    {
        switch (action)
        {
            case Interactions.PickUpToy:
                {
                    break;
                }
            case Interactions.PickUpFirewood:
                {
                    break;
                }
            case Interactions.ThrowToy:
            case Interactions.ThrowFirewood:
                {
                    break;
                }
            case Interactions.CloseWindows:
                {
                    break;
                }
            case Interactions.FlushToilet:
                {
                    break;
                }
            case Interactions.CloseSkylightWithRemote:
                {
                    break;
                }
            case Interactions.CloseBasementHatch:
                {
                    break;
                }
            default:
                {
                    break;
                }
        }
    }

    /*private int lineLength = 10; 
    private int lineThickness = 2;  
    private Color crosshairColor = Color.white;  

    void OnGUI()
    {
        GUI.color = crosshairColor;

        float centerX = Screen.width / 2;
        float centerY = Screen.height / 2;

        GUI.DrawTexture(new Rect(centerX - lineLength, centerY - (lineThickness / 2), lineLength * 2, lineThickness), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(centerX - (lineThickness / 2), centerY - lineLength, lineThickness, lineLength * 2), Texture2D.whiteTexture);
    }*/
}
