using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadioController : MonoBehaviour
{
	public AudioSource radioSource;
	public string songName;

	void Start()
	{
		if (radioSource == null)
			radioSource = GetComponent<AudioSource>();

		radioSource.loop = true;

		SoundManager.Instance.PlaySound(songName, radioSource);
	}

	void Update()
	{
		radioSource.volume = SoundManager.Instance.musicVolume * SoundManager.Instance.masterVolume;
	}
}
