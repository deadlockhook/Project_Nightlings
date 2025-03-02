using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class CutsceneManager : MonoBehaviour
{
	public static CutsceneManager Instance;

	[Header("Cutscene Settings")]
	public PlayableDirector night1Cutscene;
	public PlayableDirector night2Cutscene;
	public PlayableDirector night3Cutscene;

	private PlayerController playerController;
	private PlayableDirector currentDirector;
	private bool isPlayingCutscene = false;
	public bool IsPlayingCutscene { get { return isPlayingCutscene; } }

	private void Awake()
	{
		if (Instance == null)
			Instance = this;
		else
			Destroy(gameObject);
	}

	private void Start()
	{
		playerController = FindObjectOfType<PlayerController>();
	}

	public void PlayCutsceneWithIndex(int nightIndex)
	{
		switch (nightIndex)
		{
			case 0:
				currentDirector = night1Cutscene;
				break;
			case 1:
				currentDirector = night2Cutscene;
				break;
			case 2:
				currentDirector = night3Cutscene;
				break;
			default:
				return;
		}

		if (currentDirector == null)
		{
			return;
		}

		if (playerController != null)
			playerController.enabled = false;

		isPlayingCutscene = true;
		currentDirector.Play();
	}

	private void Update()
	{
		if (isPlayingCutscene && currentDirector != null)
		{
			if (Input.GetKeyDown(KeyCode.Return))
			{
				SkipCutscene();
			}

			else if (currentDirector.state != PlayState.Playing)
			{
				EndCutscene();
			}
		}
	}

	public void SkipCutscene()
	{
		if (currentDirector != null)
		{
			currentDirector.time = currentDirector.duration;
			currentDirector.Evaluate();
			currentDirector.Stop();
		}
		EndCutscene();
	}

	private void EndCutscene()
	{
		isPlayingCutscene = false;
		if (playerController != null)
			playerController.enabled = true;
	}
}
