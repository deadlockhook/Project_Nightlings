using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandyBarHintTrigger : MonoBehaviour
{
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered)
            return;

        if (other.CompareTag("Player"))
        {
            HintManager.Instance.DisplayGameHint(HintType.CandyBar);
            hasTriggered = true;
        }
    }
}
