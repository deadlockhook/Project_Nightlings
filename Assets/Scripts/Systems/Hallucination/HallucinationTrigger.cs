using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HallucinationTrigger : MonoBehaviour
{
	public GameObject creature;

	private bool hasTriggered = false;

	private void OnTriggerEnter(Collider other)
	{
		if (hasTriggered)
			return;

		if (other.CompareTag("Player"))
		{
			int roll = Random.Range(0, 100);
			if (roll < 10)
			{
				creature.SetActive(true);
				Debug.Log("Scary guy appeared woah!");
				hasTriggered = true;
			}
		}
	}
}
