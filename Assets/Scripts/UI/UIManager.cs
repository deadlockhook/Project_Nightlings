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
using UnityEngine.EventSystems;
using System;

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
		CreditsUI,
		Controls
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
	private GameObject controlsUI;

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

	[Header("DefaultButtons")]
    [SerializeField] private GameObject mainMenuFirstButton;
    [SerializeField] private GameObject pauseMenuFirstButton;
    [SerializeField] private GameObject optionsFirstButton;
    [SerializeField] private GameObject soundOptionsFirstButton;
    [SerializeField] private GameObject videoOptionsFirstButton;
    [SerializeField] private GameObject winFirstButton;
    [SerializeField] private GameObject loseFirstButton;
    [SerializeField] private GameObject nightPickerFirstButton;
    [SerializeField] private GameObject creditsFirstButton;
    [SerializeField] private GameObject controlsFirstButton;

    private bool isPaused = false;
	private Toggle audioVisualToggle;
	private Toggle motionBlurToggle;
	private Toggle chromaticAbberationToggle;
	private Toggle bloomToggle;
	[SerializeField] private Slider sensitivitySlider;

	[Header("UI AudioSource")]
	public AudioSource audioSource;

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
		controlsUI = transform.Find("Controls").gameObject;
		sensitivitySlider = optionsUI.transform.Find("SensitivitySlider").GetComponent<Slider>();
		resolutionDropdown = videoOptionsUI.transform.Find("ResolutionDropdown").GetComponent<TMP_Dropdown>();
		fullscreenToggle = videoOptionsUI.transform.Find("FullscreenToggle").GetComponent<Toggle>();
		motionBlurSlider = videoOptionsUI.transform.Find("MotionBlurSlider").GetComponent<Slider>();

		if (sensitivitySlider != null)
		{
			sensitivitySlider.onValueChanged.AddListener(SetSensitivity);
		}
		if(resolutionDropdown != null)
		{
			resolutionDropdown.onValueChanged.AddListener(SetResolution);
		}
		if (fullscreenToggle != null)
		{
			fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
		}
		if (motionBlurSlider != null)
		{
			motionBlurSlider.onValueChanged.AddListener(SetMotionBlur);
		}

		audioVisualToggle = soundOptionsUI.transform.Find("AudioVisualToggle").GetComponent<Toggle>();
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
    private GameObject GetDefaultButtonForState(UIState state)
    {
        switch (state)
        {
            case UIState.MainMenu: return mainMenuFirstButton;
            case UIState.PauseMenu: return pauseMenuFirstButton;
            case UIState.Options: return optionsFirstButton;
            case UIState.SoundOptions: return soundOptionsFirstButton;
            case UIState.VideoOptions: return videoOptionsFirstButton;
            case UIState.Win: return winFirstButton;
            case UIState.Lose: return loseFirstButton;
            case UIState.NightPicker: return nightPickerFirstButton;
            case UIState.CreditsUI: return creditsFirstButton;
            case UIState.Controls: return controlsFirstButton;
            default: return null;
        }
    }

    private void SelectDefaultButton(UIState state)
    {
        GameObject defaultButton = GetDefaultButtonForState(state);

        if (defaultButton != null && defaultButton.activeInHierarchy)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(defaultButton);
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
		controlsUI.SetActive(false);
		if (nightPickerUI != null) nightPickerUI.SetActive(false);
		if (nightInfoUI != null) nightInfoUI.SetActive(false);
	}

	public void ChangeUIState(UIState state)
	{
		DeactivateAllScreens();
		switch (state)
		{
			case UIState.MainMenu:
				LoadSettings();
                if (SoundManager.Instance.currentMusic != "MainMenu")
				{
					SoundManager.Instance.StopMusic();
					SoundManager.Instance.PlayMusic("MainMenu");
				}
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
				Time.timeScale = 1f;
				mainMenuUI.SetActive(true);
                SelectDefaultButton(UIState.MainMenu);
				break;
			case UIState.PauseMenu:
                Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
				Time.timeScale = 0f;
				pauseMenuUI.SetActive(true);
                SelectDefaultButton(UIState.PauseMenu);
				isPaused = true;
				break;
			case UIState.Gameplay:
				LoadSettings();
				PlayerController playerController = FindObjectOfType<PlayerController>();
				if (playerController != null)
				{
					playerController.SetSensitivity(sensitivity);
				}
				SoundManager.Instance.StopMusic();
				Time.timeScale = 1f;
				gameplayUI.SetActive(true);
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
				isPaused = false;
				break;
			case UIState.Options:
                Time.timeScale = 0f;
				optionsUI.SetActive(true);
                SelectDefaultButton(UIState.Options);
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
				break;
			case UIState.SoundOptions:
                Time.timeScale = 0f;
				soundOptionsUI.SetActive(true);
                SelectDefaultButton(UIState.SoundOptions);
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
				break;
			case UIState.VideoOptions:
                Time.timeScale = 0f;
				videoOptionsUI.SetActive(true);
                SelectDefaultButton(UIState.VideoOptions);
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
				break;
			case UIState.Win:
                Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
				Time.timeScale = 0f;
				winUI.SetActive(true);
                SelectDefaultButton(UIState.Win);
				break;
			case UIState.Lose:
                Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
				Time.timeScale = 0f;
				loseUI.SetActive(true);
                SelectDefaultButton(UIState.Lose);
				break;
			case UIState.NightPicker:
                Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
				Time.timeScale = 0f;
				nightPickerUI.SetActive(true);
                SelectDefaultButton(UIState.NightPicker);
				break;
			case UIState.NightInfo:
                LoadSettings();
				SoundManager.Instance.StopMusic();
				Cursor.lockState = CursorLockMode.Locked;
				Time.timeScale = 1f;
				nightInfoUI.SetActive(true);
				break;
			case UIState.CreditsUI:
                Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
				Time.timeScale = 0f;
				creditsUI.SetActive(true);
                SelectDefaultButton(UIState.CreditsUI);
				break;
			case UIState.Controls:
                Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
				Time.timeScale = 0f;
				controlsUI.SetActive(true);
                SelectDefaultButton(UIState.Controls);
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
			if (optionsUI.activeSelf || soundOptionsUI.activeSelf || videoOptionsUI.activeSelf || controlsUI.activeSelf)
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
   
	private bool wasUsingController = false;

    private void Update()
    {
        if (blackScreen != null && blackScreen.activeSelf)
            return;

        if (winUI.activeSelf || loseUI.activeSelf)
            return;

        HandleEscapeInput();
        DetectInputMethod();
		CheckBackButtonInput();
    }

    private void DetectInputMethod()
    {
        bool controllerInput =
            Mathf.Abs(Input.GetAxis("Horizontal")) > 0.5f ||
            Mathf.Abs(Input.GetAxis("Vertical")) > 0.5f ||
            Input.GetButtonDown("Submit") ||
            Input.GetButtonDown("Cancel") ||
            Input.GetButtonDown("Jump");

        if (controllerInput)
        {
            Cursor.visible = false;
            if (!wasUsingController)
            {
                EventSystem.current.SetSelectedGameObject(null);
                SelectDefaultButton(GetCurrentUIState());
            }
            wasUsingController = true;
        }
        else
        {
            if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0 ||
                Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
            {
                Cursor.visible = true;
                wasUsingController = false;
            }
        }
    }

    private void CheckBackButtonInput()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            Button[] buttons = FindObjectsOfType<Button>();

            foreach (Button button in buttons)
            {
                if (button.name == "BackButton")
                {
                    button.onClick.Invoke();
                    return;
                }
            }
        }
    }

    private UIState GetCurrentUIState()
    {
        if (mainMenuUI.activeSelf) return UIState.MainMenu;
        if (pauseMenuUI.activeSelf) return UIState.PauseMenu;
        if (gameplayUI.activeSelf) return UIState.Gameplay;
        if (optionsUI.activeSelf) return UIState.Options;
        if (soundOptionsUI.activeSelf) return UIState.SoundOptions;
        if (videoOptionsUI.activeSelf) return UIState.VideoOptions;
        if (winUI.activeSelf) return UIState.Win;
        if (loseUI.activeSelf) return UIState.Lose;
        if (nightPickerUI != null && nightPickerUI.activeSelf) return UIState.NightPicker;
        if (nightInfoUI != null && nightInfoUI.activeSelf) return UIState.NightInfo;
        if (creditsUI.activeSelf) return UIState.CreditsUI;
        if (controlsUI.activeSelf) return UIState.Controls;

        return UIState.MainMenu;
    }

    public void OptionsBack()
	{
		if (uiStateHistory.Count > 1)
		{
			UIState previousState = uiStateHistory.Peek();

			while (previousState == UIState.Options || previousState == UIState.SoundOptions || previousState == UIState.VideoOptions || previousState == UIState.Controls)
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

	public void ButtonSFX()
	{
		if (audioSource.isPlaying)
		{
			audioSource.Stop();
		}

		SoundManager.Instance.PlaySound("Button", audioSource);
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
		if (IconManager.Instance != null)
		{
			IconManager.Instance.ClearAllIcons();
		}

		ControlHintManager.Instance.ResetControlHints();

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

	public void GoToControls()
	{
		ChangeUIState(UIState.Controls);
	}

	public void ShowCredits()
	{
		ChangeUIState(UIState.CreditsUI);
	}

	public void QuitGame()
	{
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

		if(night == 0 && ControlHintManager.Instance != null)
		{
			ControlHintManager.Instance.ResetControlHints();
		}

		if (HintManager.Instance != null)
		{
			HintManager.Instance.ResetGameHints();
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

		yield return new WaitForSeconds(5f);

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

	// OPTIONS SECTION -------------------------------
	private GameObject mainCam;
	private Camera mainCamera;
	private Volume volume;

	private float motionBlurIntensity = 0f;
	private bool chromaticAbberationEnabled = false;
	private bool bloomEnabled = false;

	private float sensitivity = 0.1f;

	public TMP_Dropdown resolutionDropdown;
	public Toggle fullscreenToggle;
	public Slider motionBlurSlider;

	private Resolution[] resolutions;

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

	public void SetMotionBlur(float intensity)
	{
		SetupMainCamera();
		motionBlurIntensity = Mathf.Clamp(intensity, 0f, 1f);
		PlayerPrefs.SetFloat("MotionBlurIntensity", motionBlurIntensity);

		if (volume != null && volume.profile.TryGet<MotionBlur>(out MotionBlur motionBlur))
		{
			motionBlur.active = motionBlurIntensity > 0f;
			motionBlur.intensity.value = motionBlurIntensity;
		}
	}

	public void ToggleChromaticAbberation(bool enabled)
	{
		enabled = chromaticAbberationToggle.isOn;
		chromaticAbberationEnabled = enabled;
		PlayerPrefs.SetInt("ChromaticAbberation", enabled ? 1 : 0);
		ToggleEffect<ChromaticAberration>(enabled);
	}

	public void ToggleBloom(bool enabled)
	{
		enabled = bloomToggle.isOn;
		bloomEnabled = enabled;
		PlayerPrefs.SetInt("Bloom", enabled ? 1 : 0);
		ToggleEffect<Bloom>(enabled);
	}

	public void SetSensitivity(float value)
	{
		sensitivity = Mathf.Clamp(value, 0.01f, 1f);
		PlayerPrefs.SetFloat("Sensitivity", sensitivity);

		PlayerController playerController = FindObjectOfType<PlayerController>();
		if (playerController != null)
		{
			playerController.SetSensitivity(sensitivity);
		}
	}

	public void SetResolution(int index)
	{
		Resolution selectedResolution = resolutions[index];
		Screen.SetResolution(selectedResolution.width, selectedResolution.height, Screen.fullScreen);

		PlayerPrefs.SetInt("ResolutionWidth", selectedResolution.width);
		PlayerPrefs.SetInt("ResolutionHeight", selectedResolution.height);
	}

	public void SetFullscreen(bool isFullscreen)
	{
		Screen.SetResolution(Screen.width, Screen.height, isFullscreen);
		PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
	}

	private void LoadSettings()
	{
		// Motion Blur as Slider
		motionBlurIntensity = PlayerPrefs.GetFloat("MotionBlurIntensity", 0f);
		if (motionBlurSlider != null)
		{
			motionBlurSlider.value = motionBlurIntensity;
		}
		SetMotionBlur(motionBlurIntensity);

		// Toggles
		chromaticAbberationEnabled = PlayerPrefs.GetInt("ChromaticAbberation", 0) == 1;
		bloomEnabled = PlayerPrefs.GetInt("Bloom", 0) == 1;

		chromaticAbberationToggle.isOn = chromaticAbberationEnabled;
		bloomToggle.isOn = bloomEnabled;

		ToggleEffect<ChromaticAberration>(chromaticAbberationEnabled);
		ToggleEffect<Bloom>(bloomEnabled);

		// Sensitivity
		sensitivity = PlayerPrefs.GetFloat("Sensitivity", 0.25f);
		if (sensitivitySlider != null)
		{
			sensitivitySlider.value = sensitivity;
		}
		PlayerController playerController = FindObjectOfType<PlayerController>();
		if (playerController != null)
		{
			playerController.SetSensitivity(sensitivity);
		}

		// Resolution and Fullscreen
		int width = PlayerPrefs.GetInt("ResolutionWidth", 1920);
		int height = PlayerPrefs.GetInt("ResolutionHeight", 1080);
		bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;

		Resolution[] allResolutions = Screen.resolutions;
		List<Resolution> validResolutions = new List<Resolution>();

		foreach (var res in allResolutions)
		{
			if (res.width * 9 == res.height * 16)
			{
				if (!validResolutions.Any(r => r.width == res.width && r.height == res.height))
				{
					validResolutions.Add(res);
				}
			}
		}

		validResolutions.Sort((a, b) => a.width.CompareTo(b.width));
		resolutions = validResolutions.ToArray();

		if (width == 1920 && height == 1080 && resolutions.Length > 0)
		{
			Resolution maxResolution = resolutions[resolutions.Length - 1];
			Screen.SetResolution(maxResolution.width, maxResolution.height, isFullscreen);
			PlayerPrefs.SetInt("ResolutionWidth", maxResolution.width);
			PlayerPrefs.SetInt("ResolutionHeight", maxResolution.height);
		}
		else
		{
			Screen.SetResolution(width, height, isFullscreen);
		}

		resolutionDropdown.ClearOptions();
		var options = new List<string>();
		foreach (var res in resolutions)
		{
			options.Add(res.width + " x " + res.height);
		}
		resolutionDropdown.AddOptions(options);

		int currentResolutionIndex = GetCurrentResolutionIndex();
		resolutionDropdown.value = currentResolutionIndex;
		resolutionDropdown.RefreshShownValue();

		fullscreenToggle.isOn = isFullscreen;
	}

	private int GetCurrentResolutionIndex()
	{
		int currentWidth = PlayerPrefs.GetInt("ResolutionWidth", 1920);
		int currentHeight = PlayerPrefs.GetInt("ResolutionHeight", 1080);

		for (int i = 0; i < resolutions.Length; i++)
		{
			if (resolutions[i].width == currentWidth && resolutions[i].height == currentHeight)
			{
				return i;
			}
		}

		return 0;
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
