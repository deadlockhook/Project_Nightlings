using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    private InteractionManager interactionInstance = null;

    private float interactionDistance = 1.6f;
    private KeyCode interactionKey = KeyCode.Mouse0;

    public LayerMask interactableLayer;   
    public LayerMask obstacleLayer;
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

    private PlayerController playerController = null;
    private Camera playerCamera = null;

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

    public void OnLocalPlayerSetup(PlayerController targetController, Camera targetCamera)
    {
       playerController = targetController;
       playerCamera = targetCamera;
    }
    public void OnLocalPlayerUpdate()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer | obstacleLayer))
        {
            if (((1 << hit.collider.gameObject.layer) & interactableLayer) != 0)
            {
                OnInteractableFound(hit.transform.gameObject, hit);
            }
        }


    }

    private GameObject interactableObject;
    private void OnInteractableFound(GameObject interactable, RaycastHit hit)
    {
        if (interactable.tag == "Interactable_Pickup")
        {
            if (Input.GetKeyDown(interactionKey))
            {
                interactableObject = interactable;
                Debug.Log("Interactable found: " + interactable.name);
                interactable.transform.position = playerCamera.transform.position + playerCamera.transform.forward;
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
