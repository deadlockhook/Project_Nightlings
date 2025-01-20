using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class InteractionManager : MonoBehaviour
{

    private InteractionManager interactionInstance = null;
    private void Awake()
    {
        if (interactionInstance == null)
        {
            interactionInstance = this;
            DontDestroyOnLoad(interactionInstance);
        }
        else
            Destroy(this);
    }
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
    private KeyCode interactionKey = KeyCode.Mouse0;
    private float interactableMovementSpeed = 5.0f;

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

    public void OnLocalPlayerFixedUpdate()
    {
       
    }
    public void OnLocalPlayerUpdate()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer | obstacleLayer))
            if (((1 << hit.collider.gameObject.layer) & interactableLayer) != 0)
                OnInteractableFound(hit.transform.gameObject, hit);

        if (interactableObject != null)
        {

            if (interactableObject.IsDestroyed())
            {
                interactableObject = null;
                interactableObjRigidBody = null;
            }
            else if (!Input.GetKey(interactionKey))
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
                    Vector3 endPoint = playerCamera.transform.position + (playerCamera.transform.forward * interactionDistance);
                    Vector3 direction = (endPoint - interactableObjRigidBody.position).normalized;
                    float distance = Vector3.Distance(interactableObjRigidBody.position, endPoint);

                    if (!Physics.Raycast(interactableObjRigidBody.position, direction, distance, LayerMask.GetMask("Default")))
                    {
                        interactableObjRigidBody.MovePosition(Vector3.Lerp(interactableObjRigidBody.position, endPoint, Time.deltaTime * interactableMovementSpeed));
                    }
                }
                else
                    interactableObject.transform.position = playerCamera.transform.position + playerCamera.transform.forward;
            }
        }
    }

    private void OnInteractableFound(GameObject interactable, RaycastHit hit)
    {
        if (interactable.tag == "Interactable_Pickup")
        {
            if (Input.GetKeyDown(interactionKey))
            {
                interactableObject = interactable;
                interactableObjRigidBody = interactableObject.GetComponent<Rigidbody>();
                interactableObjRigidBody.useGravity = false;  
                interactableObjRigidBody.interpolation = RigidbodyInterpolation.Interpolate;
                interactableObjRigidBody.constraints = RigidbodyConstraints.FreezeRotation;
                interactableObjRigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            }
        }

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

    private int lineLength = 10; 
    private int lineThickness = 2;  
    private Color crosshairColor = Color.white;  

    void OnGUI()
    {
        GUI.color = crosshairColor;

        float centerX = Screen.width / 2;
        float centerY = Screen.height / 2;

        GUI.DrawTexture(new Rect(centerX - lineLength, centerY - (lineThickness / 2), lineLength * 2, lineThickness), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(centerX - (lineThickness / 2), centerY - lineLength, lineThickness, lineLength * 2), Texture2D.whiteTexture);
    }
}
