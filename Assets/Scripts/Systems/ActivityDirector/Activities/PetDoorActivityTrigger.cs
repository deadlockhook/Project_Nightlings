using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetDoorActivityTrigger : MonoBehaviour
{
    // Start is called before the first frame update
    PetDoorActivity petDoorActivity;

    void Start()
    {
        petDoorActivity = FindObjectOfType<PetDoorActivity>();

    }
    private void OnTriggerEnter(Collider collision)
    {
        if (petDoorActivity.activityFinished || !petDoorActivity.inActivity)
            return;

        if (collision.gameObject.tag == "Interactable_Toy")
        {
            petDoorActivity.ResetActivity();
            Destroy(collision.gameObject);
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
