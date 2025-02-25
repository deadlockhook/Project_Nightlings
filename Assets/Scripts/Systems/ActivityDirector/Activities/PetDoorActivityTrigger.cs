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
        else if (collision.gameObject.tag == "Interactable_Blocks")
        {
            petDoorActivity.ResetActivity();

            GameObject parent = collision.gameObject.transform.parent.gameObject;

            if (parent)
            {
                Rigidbody[] blocks = parent.GetComponentsInChildren<Rigidbody>();

                for (int i = 0; i < blocks.Length; i++)
                {
                    Rigidbody blockRigidBody = blocks[i];

                    if (blockRigidBody)
                    {
                        Destroy(blockRigidBody.gameObject);
                    }
                }
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
