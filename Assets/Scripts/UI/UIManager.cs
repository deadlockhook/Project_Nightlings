using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using TMPro;
using UnityEngine.Playables;

public class UIManager : MonoBehaviour
{
	public static UIManager Instance { get; private set; }
	private Stack<UIState> uiStateHistory = new Stack<UIState>();
	private List<GameObject> clocks;
	private ActivityDirector activityDirector;

	public PlayableDirector loseTimeline;

	public enum UIState
	{
		MainMenu,
		PauseMenu,
		Gameplay,
		Options,
		SoundOptions,
		VideoOptions,
		Win,
		Lose,
		NightPicker,
		NightInfo,
		CreditsUI
	}

	[Header("UI Screens")]
	private GameObject mainMenuUI;
	private GameObject pauseMenuUI;
	private GameObject gameplayUI;
	private GameObject optionsUI;
	private GameObject winUI;
	private GameObject loseUI;
	private GameObject soundOptionsUI;
	private GameObject videoOptionsUI;
	private GameObject creditsUI;

	[Header("Loading Screen")]
	public GameObject loadingScreen;
	public float loadingDisplayDuration = 0.5f;
	public float loadingFadeDuration = 1.0f;

	[Header("Death Screen")]
	public GameObject blackScreen;
	public TMP_Text deathCauseText;
	public TMP_Text timeOfDeathText;

	[Header("Win Screen")]
	public TMP_Text winText;
	public TMP_Text unlockedNightText;

	[Header("Night Picker and Info UI")]
	public GameObject nightPickerUI;
	public GameObject nightInfoUI;


	private bool isPaused = false;
	private Toggle audioVisualToggle;
	private Toggle motionBlurToggle;
	private Toggle chromaticAbberationToggle;
	private Toggle bloomToggle;

	//Singleton
	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
		DontDestroyOnLoad(gameObject);
	}

	private void Start()
	{
		mainMenuUI = transform.Find("MainMenu").gameObject;
		pauseMenuUI = transform.Find("Pause").gameObject;
		gameplayUI = transform.Find("Gameplay").gameObject;
		optionsUI = transform.Find("Options").gameObject;
		soundOptionsUI = transform.Find("SoundOptions").gameObject;
		videoOptionsUI = transform.Find("VideoOptions").gameObject;
		winUI = transform.Find("Win").gameObject;
		loseUI = transform.Find("Lose").gameObject;
		nightPickerUI = transform.Find("NightPicker").gameObject;
		nightInfoUI = transform.Find("NightInfo").gameObject;
		creditsUI = transform.Find("Credits").gameObject;

		audioVisualToggle = soundOptionsUI.transform.Find("AudioVisualToggle").GetComponent<Toggle>();
		motionBlurToggle = videoOptionsUI.transform.Find("MotionBlurToggle").GetComponent<Toggle>();
		chromaticAbberationToggle = videoOptionsUI.transform.Find("ChromaticAbberationToggle").GetComponent<Toggle>();
		bloomToggle = videoOptionsUI.transform.Find("BloomToggle").GetComponent<Toggle>();

		clocks = new List<GameObject>();
		clocks.AddRange(GameObject.FindGameObjectsWithTag("Clock"));

		ChangeUIState(UIState.MainMenu);

		if (nightPickerUI != null)
		{
			NightButton[] nightButtons = nightPickerUI.GetComponentsInChildren<NightButton>(true);
			foreach (NightButton button in nightButtons)
			{
				button.UpdateButtonState();
			}
		}
	}

	private void DeactivateAllScreens()
	{
		mainMenuUI.SetActive(false);
		pauseMenuUI.SetActive(false);
		gameplayUI.SetActive(false);
		optionsUI.SetActive(false);
		soundOptionsUI.SetActive(false);
		videoOptionsUI.SetActive(false);
		winUI.SetActive(false);
		loseUI.SetActive(false);
		creditsUI.SetActive(false);
		if (nightPickerUI != null) nightPickerUI.SetActive(false);
		if (nightInfoUI != null) nightInfoUI.SetActive(false);
	}

	public void ChangeUIState(UIState state)
	{
		DeactivateAllScreens();
		switch (state)
		{
			case UIState.MainMenu:
				if (SoundManager.Instance.currentMusic != "MainMenu")
				{
					SoundManager.Instance.StopMusic();
					SoundManager.Instance.PlayMusic("MainMenu");
				}
				Cursor.lockState = CursorLockMode.None;
				Time.timeScale = 1f;
				mainMenuUI.SetActive(true);
				break;
			case UIState.PauseMenu:
				Cursor.lockState = CursorLockMode.None;
				Time.timeScale = 0f;
				pauseMenuUI.SetActive(true);
				isPaused = true;
				break;
			case UIState.Gameplay:
				SoundManager.Instance.StopMusic();
				Time.timeScale = 1f;
				gameplayUI.SetActive(true);
				Cursor.lockState = CursorLockMode.Locked;
				isPaused = false;
				break;
			case UIState.Options:
				Time.timeScale = 0f;
				optionsUI.SetActive(true);
				Cursor.lockState = CursorLockMode.None;
				break;
			case UIState.SoundOptions:
				Time.timeScale = 0f;
				soundOptionsUI.SetActive(true);
				Cursor.lockState = CursorLockMode.None;
				break;
			case UIState.VideoOptions:
				Time.timeScale = 0f;
				videoOptionsUI.SetActive(true);
				Cursor.lockState = CursorLockMode.None;
				break;
			case UIState.Win:
				Cursor.lockState = CursorLockMode.None;
				Time.timeScale = 0f;
				winUI.SetActive(true);
				break;
			case UIState.Lose:
				Cursor.lockState = CursorLockMode.None;
				Time.timeScale = 0f;
				loseUI.SetActive(true);
				break;
			case UIState.NightPicker:
				Cursor.lockState = CursorLockMode.None;
				Time.timeScale = 0f;
				nightPickerUI.SetActive(true);
				break;
			case UIState.NightInfo:
				SoundManager.Instance.StopMusic();
				Cursor.lockState = CursorLockMode.Locked;
				Time.timeScale = 1f;
				nightInfoUI.SetActive(true);
				break;
			case UIState.CreditsUI:
				Cursor.lockState = CursorLockMode.None;
				Time.timeScale = 0f;
				creditsUI.SetActive(true);
				break;
		}
		uiStateHistory.Push(state);
	}

	private GameObject GetActiveUIPanel()
	{
		if (mainMenuUI.activeSelf) return mainMenuUI;
		if (pauseMenuUI.activeSelf) return pauseMenuUI;
		if (gameplayUI.activeSelf) return gameplayUI;
		if (winUI.activeSelf) return winUI;
		if (loseUI.activeSelf) return loseUI;
		if (nightPickerUI != null && nightPickerUI.activeSelf) return nightPickerUI;
		if (nightInfoUI != null && nightInfoUI.activeSelf) return nightInfoUI;
		return null;
	}

	private void HandleEscapeInput()
	{
		if (CutsceneManager.Instance != null && CutsceneManager.Instance.IsPlayingCutscene)
			return;

		bool canUseEscape = true;
		if ((loadingScreen != null && loadingScreen.activeSelf) ||
			(nightInfoUI != null && nightInfoUI.activeSelf))
		{
			canUseEscape = false;
		}
		if (Input.GetKeyDown(KeyCode.Escape) && canUseEscape)
		{
			if (optionsUI.activeSelf || soundOptionsUI.activeSelf || videoOptionsUI.activeSelf)
			{
				OptionsBack();
			}
			else if (creditsUI != null && creditsUI.activeSelf)
			{
				ChangeUIState(UIState.MainMenu);
			}
			else if (nightPickerUI != null && nightPickerUI.activeSelf)
			{
				ChangeUIState(UIState.MainMenu);
			}
			else if (!mainMenuUI.activeSelf)
			{
				if (isPaused)
				{
					ResumeGame();
				}
				else
				{
					PauseGame();
				}
			}
		}
	}

	private void Update()
	{
		if (blackScreen != null && blackScreen.activeSelf)
			return;

		if (winUI.activeSelf || loseUI.activeSelf)
			return;

		HandleEscapeInput();
	}

	public void OptionsBack()
	{
		if (uiStateHistory.Count > 1)
		{
			UIState previousState = uiStateHistory.Peek();

			while (previousState == UIState.Options || previousState == UIState.SoundOptions || previousState == UIState.VideoOptions)
			{
				uiStateHistory.Pop();
				if (uiStateHistory.Count == 0)
					break;

				previousState = uiStateHistory.Peek();
			}

			ChangeUIState(previousState);
		}
		else
		{
			ChangeUIState(UIState.MainMenu);
		}
	}

	public void BackFromNightPicker()
	{
		ChangeUIState(UIState.MainMenu);
	}

	public void PauseGame()
	{
		if (winUI.activeSelf || loseUI.activeSelf)
			return;

		isPaused = true;
		ChangeUIState(UIState.PauseMenu);
	}

	public void ResumeGame()
	{
		if (winUI.activeSelf || loseUI.activeSelf)
			return;
		isPaused = false;
		ChangeUIState(UIState.Gameplay);
	}

	public void GoToMainMenu()
	{
		StartCoroutine(LoadMainMenuWithLoading());
	}

	private IEnumerator LoadMainMenuWithLoading()
	{
		SceneManager.LoadScene("MainMenu");
		ChangeUIStateWithLoading(UIState.MainMenu);

		yield return new WaitForSecondsRealtime(loadingDisplayDuration + loadingFadeDuration);
	}

	public void GoToOptions()
	{
		ChangeUIState(UIState.Options);
	}

	public void GoToSoundOptions()
	{
		ChangeUIState(UIState.SoundOptions);
	}

	public void GoToVideoOptions()
	{
		ChangeUIState(UIState.VideoOptions);
	}

	public void ShowCredits()
	{
		ChangeUIState(UIState.CreditsUI);
	}

	public void QuitGame()
	{
		Debug.Log("Quitting game");
		Application.Quit();
	}

	public void WinGame()
	{
		//SoundManager.Instance.PlaySound("AlarmWin");

		ActivityDirector director = FindObjectOfType<ActivityDirector>();
		int activeNight = director != null ? director.GetActiveNight() : 0;

		string winMessage = "";
		string unlockMessage = "";

		if(activeNight == 0)
		{
			winMessage = "You Survived Friday Night!";
			unlockMessage = "Saturday Night Unlocked!";
		}
		else if(activeNight == 1)
		{
			winMessage = "You Survived Saturday Night!";
			unlockMessage = "Sunday Night Unlocked!";
		}
		else if(activeNight == 2)
		{
			winMessage = "You Survived Sunday Night!";
			unlockMessage = "All Nights Completed!";
		}
		else
		{
			winMessage = "You Survived!";
			unlockMessage = "";
		}

		if(winText != null)
		{
			winText.text = winMessage;
		}
		if(unlockedNightText != null)
		{
			unlockedNightText.text = unlockMessage;
		}

		isPaused = true;
		ChangeUIState(UIState.Win);
	}

	public void LoseGame(string deathCause)
	{
		isPaused = true;
		StartCoroutine(LoseSequence(deathCause));
	}

	private IEnumerator LoseSequence(string deathCause)
	{
		if (loseTimeline != null)
		{
			Debug.Log("JUMPSCARE");
			loseTimeline.Play();
			yield return new WaitForSecondsRealtime((float)loseTimeline.duration);
		}
		else
		{
			yield return new WaitForSecondsRealtime(3f);
		}

		if (blackScreen != null)
			blackScreen.SetActive(true);
		gameplayUI.SetActive(false);

		yield return new WaitForSecondsRealtime(1f);

		if (blackScreen != null)
			blackScreen.SetActive(false);
		loseUI.SetActive(true);

		ActivityDirector director = FindObjectOfType<ActivityDirector>();
		float deathTime = director != null ? director.DeathTime : 0f;
		string formattedTime = FormatTime(deathTime);

		if (deathCauseText != null)
			deathCauseText.text = "HINT: " + deathCause;

		if (timeOfDeathText != null)
			timeOfDeathText.text = "Killed at " + formattedTime;

		Cursor.lockState = CursorLockMode.None;
		Time.timeScale = 0f;
	}

	public string FormatTime(float seconds)
	{
		int totalMinutes = Mathf.FloorToInt(seconds);
		int hour = totalMinutes / 60;
		int minute = totalMinutes % 60;
		if (hour == 0)
			hour = 12;
		string period = "AM";
		return string.Format("{0}:{1:00} {2}", hour, minute, period);
	}

	public bool IsPaused()
	{
		return isPaused;
	}

	public bool IsInGame()
	{
		return !isPaused && gameplayUI.activeSelf;
	}

	public bool IsMainMenuActive()
	{
		return mainMenuUI.activeSelf;
	}

	public void StartGame()
	{
		ChangeUIState(UIState.NightPicker);
	}

	private IEnumerator TransitionToState(UIState newState)
	{
		loadingScreen.SetActive(true);
		CanvasGroup canvasGroup = loadingScreen.GetComponent<CanvasGroup>();
		canvasGroup.alpha = 1f;

		yield return new WaitForSecondsRealtime(loadingDisplayDuration);

		ChangeUIState(newState);

		float timer = 0f;
		while (timer < loadingFadeDuration)
		{
			timer += Time.deltaTime;
			canvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / loadingFadeDuration);
			yield return null;
		}

		loadingScreen.SetActive(false);
	}

	public void ChangeUIStateWithLoading(UIState newState)
	{
		StartCoroutine(TransitionToState(newState));
	}

	public void ProceedingToNextNight()
	{
		clocks.ForEach(clock => clock.GetComponent<Clock>().ResetClock());
	}

	public void DylanButton(int night)
	{
		if (winUI.activeSelf || loseUI.activeSelf)
			return;

		if (ProgressManager.Instance != null && !ProgressManager.Instance.IsNightUnlocked(night))
		{
			return;
		}

		StartCoroutine(LoadDylanScene(night));
	}

	private IEnumerator LoadDylanScene(int night)
	{
		Time.timeScale = 1f;

		SceneManager.LoadScene("Dylan_Test");
		ChangeUIStateWithLoading(UIState.NightInfo);

		yield return null;

		activityDirector = FindObjectOfType<ActivityDirector>();
		yield return StartCoroutine(ShowNightInfo(night));

		activityDirector.StartNight(night);
	}

	public void NightButton(int night)
	{
		if (winUI.activeSelf || loseUI.activeSelf)
			return;

		if (ProgressManager.Instance != null && !ProgressManager.Instance.IsNightUnlocked(night))
		{
			return;
		}

		StartCoroutine(LoadMainAndShowNightInfo(night));
	}

	private IEnumerator LoadMainAndShowNightInfo(int night)
	{
		Time.timeScale = 1f;

		SceneManager.LoadScene("Main");
		ChangeUIStateWithLoading(UIState.NightInfo);

		yield return null;

		activityDirector = FindObjectOfType<ActivityDirector>();
		yield return StartCoroutine(ShowNightInfo(night));

		activityDirector.StartNight(night);
	}

	private IEnumerator ShowNightInfo(int night)
	{
		ChangeUIState(UIState.NightInfo);

		PlayerController player = FindObjectOfType<PlayerController>();
		if (player != null)
		{
			player.enabled = false;
		}

		TMP_Text nightInfoText = nightInfoUI.GetComponentInChildren<TMP_Text>();
		switch (night)
		{
			case 0:
				nightInfoText.text = "Friday, 12:00am";
				break;
			case 1:
				nightInfoText.text = "Saturday, 12:00am";
				break;
			case 2:
				nightInfoText.text = "Sunday, 12:00am";
				break;
			default:
				nightInfoText.text = "Bro what night are you on?";
				break;
		}

		CanvasGroup cg = nightInfoUI.GetComponent<CanvasGroup>();
		if (cg == null)
			cg = nightInfoUI.AddComponent<CanvasGroup>();
		cg.alpha = 1f;

		yield return new WaitForSeconds(3f);

		if (CutsceneManager.Instance != null)
		{
			CutsceneManager.Instance.PlayCutsceneWithIndex(night);
		}

		float fadeDuration = 0.5f;
		float timer = 0f;
		while (timer < fadeDuration)
		{
			timer += Time.deltaTime;
			cg.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
			yield return null;
		}
		nightInfoUI.SetActive(false);

		while (CutsceneManager.Instance != null && CutsceneManager.Instance.IsPlayingCutscene)
		{
			yield return null;
		}

		if (player != null)
		{
			player.enabled = true;
		}

		ChangeUIState(UIState.Gameplay);
	}

	// OPTIONS SECTION
	private GameObject mainCam;
	private Camera mainCamera;
	private Volume volume;

	private bool motionBlurEnabled = false;
	private bool chromaticAbberationEnabled = false;
	private bool bloomEnabled = false;

	private void SetupMainCamera()
	{
		if (mainCam == null)
		{
			mainCam = GameObject.Find("Main Camera");
			if (mainCam != null)
			{
				mainCamera = mainCam.GetComponent<Camera>();
				volume = mainCam.GetComponent<Volume>();
			}
		}
	}

	private void ToggleEffect<T>(bool enabled) where T : VolumeComponent
	{
		SetupMainCamera();
		if (volume != null && volume.profile.TryGet<T>(out T effect))
		{
			effect.active = enabled;
		}
	}

	public void ToggleMotionBlur(bool enabled)
	{
		enabled = motionBlurToggle.isOn;
		motionBlurEnabled = enabled;
		ToggleEffect<MotionBlur>(enabled);
	}

	public void ToggleChromaticAbberation(bool enabled)
	{
		enabled = chromaticAbberationToggle.isOn;
		chromaticAbberationEnabled = enabled;
		ToggleEffect<ChromaticAberration>(enabled);
	}

	public void ToggleBloom(bool enabled)
	{
		enabled = bloomToggle.isOn;
		bloomEnabled = enabled;
		ToggleEffect<Bloom>(enabled);
	}

	// This is for the jumpscare atm, need to find a better way to do this
	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (loseTimeline == null)
		{
			GameObject timelineObject = GameObject.Find("TimeLineData");
			if (timelineObject != null)
				loseTimeline = timelineObject.GetComponent<PlayableDirector>();
		}
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}
}
