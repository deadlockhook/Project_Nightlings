using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using TMPro;
using UnityEngine.UI;

public class CutsceneManager : MonoBehaviour
{
	public static CutsceneManager Instance;

	[Header("Cutscene Settings")]
	public PlayableDirector night1Cutscene;
	public PlayableDirector night2Cutscene;
	public PlayableDirector night3Cutscene;

	private float skipHoldTime = 3f;
	private float currentHoldTime = 0f;
	private bool isHoldingSkip = false;

	[Header("Skip UI")]
	public CanvasGroup skipPanel;
	public Image skipProgressImage;

	public TextMeshProUGUI skipText;

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

		if(skipText != null)
			skipText.gameObject.SetActive(false);
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

		if(skipText != null)
			skipText.gameObject.SetActive(true);

		if (skipPanel != null)
			skipPanel.alpha = 0f;

		if (skipProgressImage != null)
			skipProgressImage.fillAmount = 0f;
	}

	private void Update()
	{
		if (isPlayingCutscene && currentDirector != null)
		{
			if (Input.GetKey(KeyCode.Return))
			{
				currentHoldTime += Time.deltaTime;

				if (!isHoldingSkip)
				{
					isHoldingSkip = true;
					if (skipPanel != null)
						skipPanel.alpha = 1f;
				}

				if (skipProgressImage != null)
				{
					skipProgressImage.fillAmount = Mathf.Clamp01(currentHoldTime / skipHoldTime);
				}

				if (currentHoldTime >= skipHoldTime)
				{
					SkipCutscene();
				}
			}
			else
			{
				currentHoldTime = 0f;
				isHoldingSkip = false;

				if (skipPanel != null)
					skipPanel.alpha = 0f;

				if (skipProgressImage != null)
					skipProgressImage.fillAmount = 0f;
			}

			if (currentDirector.state != PlayState.Playing)
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
		List<GameObject> clocks = new List<GameObject>(GameObject.FindGameObjectsWithTag("Clock"));
		clocks.ForEach(clock => clock.GetComponent<Clock>().ResetClock());

		isPlayingCutscene = false;
		if (playerController != null)
		{
			playerController.enabled = true;
			playerController.EnableFlashlight();
		}

		if (skipText != null)
			skipText.gameObject.SetActive(false);

		Cursor.lockState = CursorLockMode.Locked;
		//Cursor.visible = false;

		if (currentDirector == night1Cutscene)
		{
			ControlHintManager.Instance.ShowControlHints();
		}

		if (skipPanel != null)
			skipPanel.alpha = 0f;

		if (skipProgressImage != null)
			skipProgressImage.fillAmount = 0f;
	}
}
