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
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
	public static UIManager Instance { get; private set; }
	private Stack<UIState> uiStateHistory = new Stack<UIState>();
	private List<GameObject> clocks;
	private ActivityDirector activityDirector;

	public PlayableDirector loseTimeline;

	public PlayerControlActions playerControlActions;

	private bool allowCursorToggle = true;
	private UIState previousUIState;
	[HideInInspector] public bool wasUsingController = false;
	private Vector3 previousMousePosition;
	private Vector3 currentMousePosition;
	private float cursorStateChangeCooldown = 0.5f;
	private float lastCursorStateChangeTime;
	private bool lastCursorVisible;
	private float mouseSensitivity = 1f;

	public enum UIState
	{
		MainMenu,
		PauseMenu,
		Gameplay,
		Options,
		SoundOptions,
		VideoOptions,
		Win,
		FinalWin,
		Lose,
		NightPicker,
		DifficultySelection,
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

	[Header("Final Win Screen")]
	public GameObject finalWinUI;
	public CanvasGroup finalWinImageCanvasGroup;
	public CanvasGroup finalWinTextCanvasGroup;
	[SerializeField] private GameObject finalWinFirstButton;

	[Header("Night Picker and Info UI")]
	public GameObject nightPickerUI;
	public GameObject nightInfoUI;

	[Header("Difficulty Selection")]
	public GameObject difficultySelectionUI;
	[SerializeField] private GameObject difficultySelectionFirstButton;
	[SerializeField] private TextMeshProUGUI difficultyDescriptionText;

	private readonly string[] difficultyDescriptions = new string[]
	{
		"Standard difficulty",
		"Nightlings are more aggressive",
		"Nightlings are even more aggressive, less toys",
		"Extremely aggressive nightlings, even less toys, lower stamina, no activity icons",
		"Nightlings are relentless, almost no toys, very low stamina, no candy, no activity icons, very dark"
	};

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

	private int pendingNightIndex;
	public static int SelectedDifficulty {get; private set;}

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

		playerControlActions = new PlayerControlActions();
		playerControlActions.Enable();
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

		ForceInputDetection();

		if (nightPickerUI != null)
		{
			NightButton[] nightButtons = nightPickerUI.GetComponentsInChildren<NightButton>(true);
			foreach (NightButton button in nightButtons)
			{
				button.UpdateButtonState();
			}
		}
	}

	private void ForceInputDetection()
	{
		bool controllerActive = false;
		for (int i = 0; i < 20; i++)
		{
			if (Input.GetKey((KeyCode)((int)KeyCode.JoystickButton0 + i)))
			{
				controllerActive = true;
				break;
			}
		}

		bool mouseActive = Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0;
		bool keyboardActive = Input.anyKey;

		if (controllerActive)
		{
			wasUsingController = true;
			SetCursorState(false);
			SelectDefaultButton(GetCurrentUIState());
		}
		else if (mouseActive || keyboardActive)
		{
			wasUsingController = false;
			SetCursorState(true);
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
			case UIState.DifficultySelection: return difficultySelectionFirstButton;
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
			if (EventSystem.current.currentSelectedGameObject != null)
			{
				var currentSelected = EventSystem.current.currentSelectedGameObject;
				var currentGlow = currentSelected.GetComponent<UITextGlow>();
				if (currentGlow != null)
				{
					currentGlow.OnDeselect(null);
				}
			}

			EventSystem.current.SetSelectedGameObject(null);
			EventSystem.current.SetSelectedGameObject(defaultButton);

			Button button = defaultButton.GetComponent<Button>();
			if (button != null)
			{
				button.Select();
				button.OnSelect(null);

				var glow = defaultButton.GetComponent<UITextGlow>();
				if (glow != null)
				{
					glow.OnSelect(null);
				}
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
		controlsUI.SetActive(false);
		if (nightPickerUI != null) nightPickerUI.SetActive(false);
		if (nightInfoUI != null) nightInfoUI.SetActive(false);
		if (difficultySelectionUI != null) difficultySelectionUI.SetActive(false);
		if (finalWinUI != null) finalWinUI.SetActive(false);
	}

	public void ChangeUIState(UIState state)
	{
		DeactivateAllScreens();

		previousUIState = uiStateHistory.Count > 0 ? uiStateHistory.Peek() : UIState.Gameplay;

		EventSystem.current.SetSelectedGameObject(null);

		switch (state)
		{
			case UIState.MainMenu:
				LoadSettings();
				if (SoundManager.Instance.currentMusic != "MainMenu")
				{
					SoundManager.Instance.StopMusic();
					SoundManager.Instance.PlayMusic("MainMenu");
				}
				allowCursorToggle = true;
				mainMenuUI.SetActive(true);
				StartCoroutine(SelectDefaultButtonAfterFrame(UIState.MainMenu));
				break;
			case UIState.PauseMenu:
				allowCursorToggle = true;
				Time.timeScale = 0f;
				pauseMenuUI.SetActive(true);
				StartCoroutine(SelectDefaultButtonAfterFrame(UIState.PauseMenu));
				isPaused = true;
				break;
			case UIState.Gameplay:
				playerControlActions.Enable();
				LoadSettings();
				PlayerController playerController = FindObjectOfType<PlayerController>();
				if (playerController != null)
				{
					playerController.SetSensitivity(sensitivity);
				}
				SoundManager.Instance.StopMusic();
				allowCursorToggle = false;
				Time.timeScale = 1f;
				gameplayUI.SetActive(true);
				isPaused = false;
				break;
			case UIState.Options:
				allowCursorToggle = true;
				Time.timeScale = 0f;
				optionsUI.SetActive(true);
				StartCoroutine(SelectDefaultButtonAfterFrame(UIState.Options));
				break;
			case UIState.SoundOptions:
				allowCursorToggle = true;
				Time.timeScale = 0f;
				soundOptionsUI.SetActive(true);
				StartCoroutine(SelectDefaultButtonAfterFrame(UIState.SoundOptions));
				break;
			case UIState.VideoOptions:
				allowCursorToggle = true;
				Time.timeScale = 0f;
				videoOptionsUI.SetActive(true);
				StartCoroutine(SelectDefaultButtonAfterFrame(UIState.VideoOptions));
				break;
			case UIState.Win:
				allowCursorToggle = true;
				Time.timeScale = 0f;
				winUI.SetActive(true);
				StartCoroutine(SelectDefaultButtonAfterFrame(UIState.Win));
				break;
			case UIState.Lose:
				allowCursorToggle = true;
				Time.timeScale = 0f;
				loseUI.SetActive(true);
				StartCoroutine(SelectDefaultButtonAfterFrame(UIState.Lose));
				break;
			case UIState.NightPicker:
				allowCursorToggle = true;
				Time.timeScale = 0f;
				nightPickerUI.SetActive(true);
				StartCoroutine(SelectDefaultButtonAfterFrame(UIState.NightPicker));
				break;
			case UIState.DifficultySelection:
				allowCursorToggle = true;
				Time.timeScale = 0f;
				difficultySelectionUI.SetActive(true);
				StartCoroutine(SelectDefaultButtonAfterFrame(UIState.DifficultySelection));
				break;
			case UIState.NightInfo:
				LoadSettings();
				SoundManager.Instance.StopMusic();
				allowCursorToggle = true;
				Time.timeScale = 1f;
				nightInfoUI.SetActive(true);
				break;
			case UIState.CreditsUI:
				allowCursorToggle = true;
				Time.timeScale = 0f;
				creditsUI.SetActive(true);
				StartCoroutine(SelectDefaultButtonAfterFrame(UIState.CreditsUI));
				break;
			case UIState.Controls:
				allowCursorToggle = true;
				Time.timeScale = 0f;
				controlsUI.SetActive(true);
				StartCoroutine(SelectDefaultButtonAfterFrame(UIState.Controls));
				break;
			case UIState.FinalWin:
				allowCursorToggle = true;
				Time.timeScale = 0f;
				finalWinUI.SetActive(true);
				StartCoroutine(SelectDefaultButtonAfterFrame(UIState.FinalWin));
				break;
		}
		uiStateHistory.Push(state);

		UpdateCursorVisibility();
	}

	private IEnumerator SelectDefaultButtonAfterFrame(UIState state)
	{
		yield return null;

		GameObject defaultButton = GetDefaultButtonForState(state);
		if (defaultButton != null && defaultButton.activeInHierarchy)
		{
			if (EventSystem.current.currentSelectedGameObject != null)
			{
				var currentSelected = EventSystem.current.currentSelectedGameObject;
				var currentGlow = currentSelected.GetComponent<UITextGlow>();
				if (currentGlow != null)
				{
					currentGlow.OnDeselect(null);
				}
			}

			EventSystem.current.SetSelectedGameObject(null);
			EventSystem.current.SetSelectedGameObject(defaultButton);

			Button button = defaultButton.GetComponent<Button>();
			if (button != null)
			{
				button.Select();
				button.OnSelect(null);

				var glow = defaultButton.GetComponent<UITextGlow>();
				if (glow != null)
				{
					glow.OnSelect(null);
				}
			}
		}
	}

	private void SetCursorState(bool visible)
	{
		if (Time.time - lastCursorStateChangeTime < cursorStateChangeCooldown && visible == lastCursorVisible)
			return;

		Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
		Cursor.visible = visible;
		lastCursorVisible = visible;
		lastCursorStateChangeTime = Time.time;

		if (!visible && EventSystem.current.currentSelectedGameObject == null)
		{
			SelectDefaultButton(GetCurrentUIState());
		}
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
		if (GetCurrentUIState() == UIState.FinalWin)
			return;

		if (CutsceneManager.Instance != null && CutsceneManager.Instance.IsPlayingCutscene)
			return;

		bool canUseEscape = true;
		if ((loadingScreen != null && loadingScreen.activeSelf) ||
			(nightInfoUI != null && nightInfoUI.activeSelf))
		{
			canUseEscape = false;
		}
		if (playerControlActions.Player.Escape.WasPressedThisFrame() && canUseEscape)
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

	private void Update()
	{
		DetectInputMethod();
		UpdateInputBasedSensitivity();

		UpdateCursorVisibility();

		if (blackScreen != null && blackScreen.activeSelf)
			return;

		if (winUI.activeSelf || loseUI.activeSelf)
		{
			if (wasUsingController && EventSystem.current.currentSelectedGameObject == null)
			{
				SelectDefaultButton(GetCurrentUIState());
			}
			return;
		}

		if ((finalWinUI != null && finalWinUI.activeSelf) || GetCurrentUIState() == UIState.FinalWin)
		{
			if (wasUsingController && EventSystem.current.currentSelectedGameObject == null)
			{
				SelectDefaultButton(GetCurrentUIState());
			}
			return;
		}

		HandleEscapeInput();
		CheckBackButtonInput();

		if (wasUsingController && EventSystem.current.currentSelectedGameObject == null)
		{
			SelectDefaultButton(GetCurrentUIState());
		}
	}

	private void UpdateCursorVisibility()
	{
		UIState currentState = GetCurrentUIState();
		bool isGameplayState = (currentState == UIState.Gameplay || currentState == UIState.NightInfo);

		if (isGameplayState)
		{
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
		}
		else
		{
			Cursor.visible = !wasUsingController;
			Cursor.lockState = wasUsingController ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}

	private void DetectInputMethod()
	{
		bool controllerInput = CheckControllerInput();
		currentMousePosition = Input.mousePosition;
		Vector3 mouseDelta = currentMousePosition - previousMousePosition;
		bool significantMouseMove = mouseDelta.magnitude > mouseSensitivity;
		bool mouseClicked = CheckMouseClick();
		bool keyboardInput = CheckAnyKeyboardInput();

		if (wasUsingController && (significantMouseMove || mouseClicked || keyboardInput))
		{
			wasUsingController = false;
			EventSystem.current.SetSelectedGameObject(null);
			UpdateInputBasedSensitivity();
		}
		else if (!wasUsingController && controllerInput)
		{
			wasUsingController = true;
			SelectDefaultButton(GetCurrentUIState());
			UpdateInputBasedSensitivity();
		}

		previousMousePosition = currentMousePosition;
	}

	private bool CheckAnyKeyboardInput()
	{
		foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
		{
			if (Input.GetKeyDown(keyCode) && IsKeyboardKey(keyCode))
			{
				return true;
			}
		}
		return false;
	}

	private bool IsKeyboardKey(KeyCode key)
	{
		return key < KeyCode.JoystickButton0 &&
			!IsMouseKey(key);
	}

	private bool IsMouseKey(KeyCode key)
	{
		return key == KeyCode.Mouse0 ||
			key == KeyCode.Mouse1 ||
			key == KeyCode.Mouse2;
	}

	private bool CheckMouseClick()
	{
		return Input.GetMouseButtonDown(0) ||
			Input.GetMouseButtonDown(1) ||
			Input.GetMouseButtonDown(2);
	}

	private bool CheckControllerInput()
	{
		for (int i = 0; i < 20; i++)
		{
			if (Input.GetKey((KeyCode)((int)KeyCode.JoystickButton0 + i)))
			{
				return true;
			}
		}

		if (Gamepad.current != null)
		{
			Vector2 rightStick = Gamepad.current.rightStick.ReadValue();
			if (rightStick.magnitude > 0.1f)
			{
				return true;
			}
		}
		if (Gamepad.current != null)
		{
			Vector2 rightStick = Gamepad.current.leftStick.ReadValue();
			if (rightStick.magnitude > 0.1f)
			{
				return true;
			}
		}

		return false;
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

	public void GoToNightPicker()
	{
		ChangeUIState(UIState.NightPicker);
	}

	public void PauseGame()
	{
		if (winUI.activeSelf || loseUI.activeSelf)
			return;

		isPaused = true;
		playerControlActions.Disable();
		ChangeUIState(UIState.PauseMenu);
	}

	public void ResumeGame()
	{
		if (winUI.activeSelf || loseUI.activeSelf)
			return;

		Input.ResetInputAxes();
		StartCoroutine(PreventJumpOnResume());
		ChangeUIState(UIState.Gameplay);
		isPaused = false;
	}

	private IEnumerator PreventJumpOnResume()
	{
		playerControlActions.Disable();
		yield return new WaitForSeconds(0.5f);
		playerControlActions.Enable();
	}

	public void GoToMainMenu()
	{
		Time.timeScale = 1f;
		if (IconManager.Instance != null)
		{
			IconManager.Instance.ClearAllIcons();
		}

		ControlHintManager.Instance.ResetControlHints();

		StartCoroutine(LoadMainMenuWithLoading());
	}

	private IEnumerator LoadMainMenuWithLoading()
	{
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("MainMenu");
		asyncLoad.allowSceneActivation = false;

		while (!asyncLoad.isDone)
		{
			if (asyncLoad.progress >= 0.9f)
				asyncLoad.allowSceneActivation = true;

			yield return null;
		}

		ChangeUIStateWithLoading(UIState.MainMenu);
		Time.timeScale = 1f;
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
		if(activeNight == 2)
		{
			StartCoroutine(FinalWinSequence());
		}
		else
		{
			StartCoroutine(WinSequence());
		}
	}

	private IEnumerator WinSequence()
	{
		ChangeUIState(UIState.Win);

		yield return null;

		ForceInputDetection();

		if (winFirstButton != null && winFirstButton.activeInHierarchy)
		{
			EventSystem.current.SetSelectedGameObject(null);
			EventSystem.current.SetSelectedGameObject(winFirstButton);

			Button button = winFirstButton.GetComponent<Button>();
			if (button != null)
			{
				button.Select();
				button.OnSelect(null);

				var glow = winFirstButton.GetComponent<UITextGlow>();
				if (glow != null)
				{
					glow.OnSelect(null);
				}
			}
		}
	}

	private IEnumerator FinalWinSequence()
	{
		SoundManager.Instance.StopMusic();
		SoundManager.Instance.PauseAllSounds();
		SoundManager.Instance.StopThunderSounds();

		ChangeUIState(UIState.FinalWin);

		if(finalWinImageCanvasGroup != null)
			finalWinImageCanvasGroup.alpha = 0f;
		if(finalWinTextCanvasGroup != null)
			finalWinTextCanvasGroup.alpha = 0f;

		yield return new WaitForSecondsRealtime(3f);

		float fadeDuration = 2f;
		float timer = 0f;
		while(timer < fadeDuration)
		{
			timer += Time.unscaledDeltaTime;
			if(finalWinImageCanvasGroup != null)
				finalWinImageCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
			yield return null;
		}
		if(finalWinImageCanvasGroup != null)
			finalWinImageCanvasGroup.alpha = 1f;

		SoundManager.Instance.PlayMusic("SunRise");

		timer = 0f;
		while(timer < fadeDuration)
		{
			timer += Time.unscaledDeltaTime;
			if(finalWinTextCanvasGroup != null)
				finalWinTextCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
			yield return null;
		}
		if(finalWinTextCanvasGroup != null)
			finalWinTextCanvasGroup.alpha = 1f;

		if(finalWinFirstButton != null && finalWinFirstButton.activeInHierarchy)
		{
			EventSystem.current.SetSelectedGameObject(null);
			EventSystem.current.SetSelectedGameObject(finalWinFirstButton);
			Button button = finalWinFirstButton.GetComponent<Button>();
			if (button != null)
			{
				button.Select();
				button.OnSelect(null);
			}
		}
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

		Time.timeScale = 0f;

		yield return null;

		ForceInputDetection();
		UpdateCursorVisibility();

		if (loseFirstButton != null && loseFirstButton.activeInHierarchy)
		{
			EventSystem.current.SetSelectedGameObject(null);
			EventSystem.current.SetSelectedGameObject(loseFirstButton);

			Button button = loseFirstButton.GetComponent<Button>();
			if (button != null)
			{
				button.Select();
				button.OnSelect(null);

				var glow = loseFirstButton.GetComponent<UITextGlow>();
				if (glow != null)
				{
					glow.OnSelect(null);
				}
			}
		}
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

		yield return new WaitUntil(() => SceneManager.GetActiveScene().isLoaded);

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

		pendingNightIndex = night;
		ChangeUIState(UIState.DifficultySelection);
	}

	public void DifficultySelected(int difficulty)
	{
		SelectedDifficulty = difficulty;
		PlayerPrefs.SetInt("SelectedDifficulty", difficulty);
		if (pendingNightIndex == 0 && ControlHintManager.Instance != null)
		{
			ControlHintManager.Instance.ResetControlHints();
		}
		/*if (HintManager.Instance != null)
		{
			HintManager.Instance.ResetGameHints();
		}*/
		StartCoroutine(LoadMainAndShowNightInfo(pendingNightIndex));
	}

	private IEnumerator LoadMainAndShowNightInfo(int night)
	{
		Time.timeScale = 1f;
		isPaused = false;
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
				nightInfoText.text = "?, 12:00am";
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

	[HideInInspector] public float sensitivity = 0.25f;

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

	public void UpdateInputBasedSensitivity()
	{
		PlayerController playerController = FindObjectOfType<PlayerController>();
		if (playerController != null)
		{
			float adjustedSensitivity = wasUsingController ? sensitivity * 4f : sensitivity;
			playerController.SetSensitivity(adjustedSensitivity);
		}
	}

	public void SetSensitivity(float value)
	{
		sensitivity = Mathf.Clamp(value, 0.01f, 1f);
		PlayerPrefs.SetFloat("Sensitivity", sensitivity);
		UpdateInputBasedSensitivity();
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
		SetSensitivity(sensitivity);
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

	public void ToggleInputMethod()
	{
		wasUsingController = !wasUsingController;

		if (wasUsingController)
		{
			if (allowCursorToggle)
			{
				SetCursorState(false);
			}
			SelectDefaultButton(GetCurrentUIState());
		}
		else
		{
			if (allowCursorToggle)
			{
				SetCursorState(true);
			}
			EventSystem.current.SetSelectedGameObject(null);
		}
	}

	public void ShowDifficultyDescription(int difficulty)
	{
		if (difficultyDescriptionText != null && difficulty >= 0 && difficulty < difficultyDescriptions.Length)
			difficultyDescriptionText.text = difficultyDescriptions[difficulty];
	}

	public void ClearDifficultyDescription()
	{
		if (difficultyDescriptionText != null)
			difficultyDescriptionText.text = string.Empty;
	}
}
