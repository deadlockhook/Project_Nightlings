using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HallucinationEndTrigger : MonoBehaviour
{
	public GameObject creature;
	public GameObject flashLight;
	public GameObject hallwayLight;
	public AudioClip scarySound;
	public float flickerDuration = 1.0f;
	public float flickerInterval = 0.1f;

	private bool hasTriggered = false;
	private AudioSource audioSource;

	private void Start()
	{
		audioSource = GetComponent<AudioSource>();
		if (audioSource == null)
		{
			audioSource = gameObject.AddComponent<AudioSource>();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (hasTriggered)
			return;

		if (other.CompareTag("Player"))
		{
			if (creature == null || !creature.activeSelf)
			{
				return;
			}

			hasTriggered = true;

			creature.SetActive(false);

			if (scarySound != null)
			{
				audioSource.PlayOneShot(scarySound);
			}


			StartCoroutine(FlickerFlashlights());
		}
	}

	private IEnumerator FlickerFlashlights()
	{
		float elapsed = 0f;
		while (elapsed < flickerDuration)
		{

			if (flashLight != null)
			{
				flashLight.SetActive(!flashLight.activeSelf);
			}

			if (hallwayLight != null)
			{
				hallwayLight.SetActive(!hallwayLight.activeSelf);
			}

			yield return new WaitForSeconds(flickerInterval);
			elapsed += flickerInterval;
		}

		if (flashLight != null)
		{
			flashLight.SetActive(true);
		}
		if (hallwayLight != null)
		{
			hallwayLight.SetActive(true);
		}
	}
}
