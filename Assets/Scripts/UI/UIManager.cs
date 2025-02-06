using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;

public class UIManager : MonoBehaviour
{
	public static UIManager Instance { get; private set; }
	public GameObject bellIconPrefab;
	//public Transform worldSpaceCanvas;
	private Dictionary<int, GameObject> activeIcons = new Dictionary<int, GameObject>();
	private Stack<UIState> uiStateHistory = new Stack<UIState>();
	private List<GameObject> clocks;

	public enum UIState
	{
		MainMenu,
		PauseMenu,
		Gameplay,
		Options,
		SoundOptions,
		VideoOptions,
        Win,
		Lose
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

    [Header("Loading Screen")]
	public GameObject loadingScreen;
	public float loadingDisplayDuration = 0.5f;
	public float loadingFadeDuration = 1.0f;


	private bool isPaused = false;
	private Toggle audioVisualToggle;
	private Toggle motionBlurToggle;
	private Toggle chromaticAbberationToggle;
	private Toggle bloomToggle;
    private Color previousSceneColor;

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

		audioVisualToggle = soundOptionsUI.transform.Find("AudioVisualToggle").GetComponent<Toggle>();
        motionBlurToggle = videoOptionsUI.transform.Find("MotionBlurToggle").GetComponent<Toggle>();
		chromaticAbberationToggle = videoOptionsUI.transform.Find("ChromaticAbberationToggle").GetComponent<Toggle>();
		bloomToggle = videoOptionsUI.transform.Find("BloomToggle").GetComponent<Toggle>();

        clocks = new List<GameObject>();
		clocks.AddRange(GameObject.FindGameObjectsWithTag("Clock"));

        ChangeUIState(UIState.MainMenu);
	}
	private void CapturePreviousUIImage(Image uiImage)
	{
		if (uiImage != null)
		{
			previousSceneColor = uiImage.color;
		}
	}

	public void ChangeUIState(UIState state)
	{
		if (state == UIState.Options || state == UIState.SoundOptions|| state == UIState.VideoOptions)
		{
			GameObject activeUI = GetActiveUIPanel();
			if (activeUI != null)
			{
				CapturePreviousUIImage(activeUI.GetComponent<Image>());
			}
		}

		mainMenuUI.SetActive(false);
		pauseMenuUI.SetActive(false);
		gameplayUI.SetActive(false);
		optionsUI.SetActive(false);
        soundOptionsUI.SetActive(false);
        videoOptionsUI.SetActive(false);
        winUI.SetActive(false);
		loseUI.SetActive(false);

		switch (state)
		{
			case UIState.MainMenu:
				if (SoundManager.Instance.currentMusic != "MainMenu")
				{
					SoundManager.Instance.StopMusic();
					SoundManager.Instance.PlayMusic("MainMenu");
				}
				Cursor.lockState = CursorLockMode.None;
				Time.timeScale = 0f;
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
				ApplyPreviousSceneColor();
				break;
			case UIState.SoundOptions:
                Time.timeScale = 0f;
                soundOptionsUI.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
                ApplyPreviousSceneColor();
                break;
            case UIState.VideoOptions:
                Time.timeScale = 0f;
                videoOptionsUI.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
                ApplyPreviousSceneColor();
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
		return null;
	}

	private void ApplyPreviousSceneColor()
	{
		Image optionsImage = optionsUI.GetComponent<Image>();
		Image soundOptionsImage = soundOptionsUI.GetComponent<Image>();
		Image videoOptionsImage = videoOptionsUI.GetComponent<Image>();

		if (optionsImage != null)
		{
			optionsImage.color = previousSceneColor;
		}
		if (soundOptionsImage != null)
		{
			soundOptionsImage.color = previousSceneColor;
		}
		if (videoOptionsImage != null)
		{
			videoOptionsImage.color = previousSceneColor;
		}
	}

    private void Update()
    {
        if (winUI.activeSelf || loseUI.activeSelf)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (optionsUI.activeSelf)
            {
                OptionsBack();
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

        if (!iconsEnabled)
        {
            foreach (var eventIndex in activeIcons.Keys.ToList())
            {
                DeactivateIcon(eventIndex);
            }
        }
        else
        {
            ReactivateIcons();
        }
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
		ChangeUIState(UIState.MainMenu);
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

    public void QuitGame()
	{
		Debug.Log("Quitting game");
		Application.Quit();
	}

	public void WinGame()
	{
		isPaused = true;
		ChangeUIState(UIState.Win);
	}

	public void LoseGame()
	{
		isPaused = true;
		ChangeUIState(UIState.Lose);
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

	public void RestartScene()
	{
		Time.timeScale = 1f;
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
		ChangeUIStateWithLoading(UIState.Gameplay);
	}

    public void ShowIcon(GameObject iconPrefab, Vector3 position, int eventIndex)
    {
        if (activeIcons.ContainsKey(eventIndex))
            return;

        GameObject icon = Instantiate(iconPrefab, position, Quaternion.identity);
        activeIcons.Add(eventIndex, icon);
    }

    public void HideIcon(int eventIndex)
    {
        if (!activeIcons.ContainsKey(eventIndex))
            return;

        Destroy(activeIcons[eventIndex]);
        activeIcons.Remove(eventIndex);
    }

    public void DeactivateIcon(int eventIndex)
    {
        if (!activeIcons.ContainsKey(eventIndex))
            return;
        if (activeIcons[eventIndex] != null)
            activeIcons[eventIndex].SetActive(false);
    }
    private void ReactivateIcons()
    {
        foreach (var icon in activeIcons.Values)
        {
            if (!icon.activeSelf)
            {
                icon.SetActive(true);
            }
        }
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

    // OPTIONS SECTION
	private GameObject mainCam;
	private Camera mainCamera;
	private Volume volume;

    [HideInInspector] public bool iconsEnabled = false;
	private bool motionBlurEnabled = false;
	private bool chromaticAbberationEnabled = false;
	private bool bloomEnabled = false;
    public void ToggleIconsEnabled(bool enabled)
    {
        enabled = audioVisualToggle.isOn;
        iconsEnabled = enabled;

        foreach (var icon in activeIcons.Values)
        {
            icon.SetActive(iconsEnabled);
        }
    }

	public void ToggleMotionBlur(bool enabled)
	{
		enabled = motionBlurToggle.isOn;
        motionBlurEnabled = enabled;
		mainCam = GameObject.Find("Main Camera");
        mainCamera = mainCam.GetComponent<Camera>();
		volume = mainCam.GetComponent<Volume>();

        if (motionBlurEnabled)
        {
            volume.profile.TryGet(out MotionBlur motionBlur);
            if (motionBlur != null)
            {
                motionBlur.active = true;
            }
        }
        else
        {
            volume.profile.TryGet(out MotionBlur motionBlur);
            if (motionBlur != null)
            {
                motionBlur.active = false;
            }
        }
    }
    public void ToggleChromaticAbberation(bool enabled)
    {
        enabled = chromaticAbberationToggle.isOn;
        chromaticAbberationEnabled = enabled;
        mainCam = GameObject.Find("Main Camera");
        mainCamera = mainCam.GetComponent<Camera>();
        volume = mainCam.GetComponent<Volume>();

        if (chromaticAbberationEnabled)
        {
            volume.profile.TryGet(out ChromaticAberration chromatic);
            if (chromatic != null)
            {
                chromatic.active = true;
            }
        }
        else
        {
            volume.profile.TryGet(out ChromaticAberration chromatic);
            if (chromatic != null)
            {
                chromatic.active = false;
            }
        }
    }

	public void ToggleBloom(bool enabled)
	{
        enabled = bloomToggle.isOn;
        bloomEnabled = enabled;
        mainCam = GameObject.Find("Main Camera");
        mainCamera = mainCam.GetComponent<Camera>();
        volume = mainCam.GetComponent<Volume>();

        if (bloomEnabled)
        {
            volume.profile.TryGet(out Bloom bloom);
            if (bloom != null)
            {
                bloom.active = true;
            }
        }
        else
        {
            volume.profile.TryGet(out Bloom bloom);
            if (bloom != null)
            {
                bloom.active = false;
            }
        }

    }

}