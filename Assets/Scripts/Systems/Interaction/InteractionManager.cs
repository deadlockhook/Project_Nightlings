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
        PickUpFirewood,
        CloseWindows,
        FlushToilet,
        CloseSkylightWithRemote,
        CloseBasementHatch,
    }

    void OnLocalPlayerViewUpdate()
    {

    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
