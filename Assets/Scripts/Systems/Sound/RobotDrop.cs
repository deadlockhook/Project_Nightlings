using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotDrop : MonoBehaviour
{
    private bool canPlaySound = false;

    private void Start()
    {
        StartCoroutine(EnableSound(3f));
    }

    private IEnumerator EnableSound(float delay)
    {
        yield return new WaitForSeconds(delay);
        canPlaySound = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (canPlaySound)
        {
            SoundManager.Instance.PlaySound("TinToyDrop", GetComponent<AudioSource>());
        }
    }
}
