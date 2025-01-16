using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    void OnLocalPlayerViewUpdate()
    {
        //Raycast and get the tagged object
        //Check if the object is interactable
        //Set the held object to the interactable type and trigger specific time

        //TriggerEvent(); //Trigger the event if the interaction key is pressed
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
                break;
            case Interactions.CloseSkylightWithRemote:
                break;
            case Interactions.CloseBasementHatch:
                break;
        }
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
