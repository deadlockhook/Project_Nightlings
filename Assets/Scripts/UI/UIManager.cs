using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
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

	[Header("Loading Screen")]
	public GameObject loadingScreen;
	public float loadingDisplayDuration = 0.5f;
	public float loadingFadeDuration = 1.0f;


	private bool isPaused = false;
	private Toggle audioVisualToggle;
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
		winUI = transform.Find("Win").gameObject;
		loseUI = transform.Find("Lose").gameObject;
		audioVisualToggle = optionsUI.transform.Find("AudioVisualToggle").GetComponent<Toggle>();

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
		if (state == UIState.Options)
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
		if (optionsImage != null)
		{
			optionsImage.color = previousSceneColor;
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
                Back();
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

    public void Back()
	{
		if (uiStateHistory.Count > 1)
		{
			uiStateHistory.Pop();
			ChangeUIState(uiStateHistory.Peek());
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

	[HideInInspector] public bool iconsEnabled = false;

    public void ToggleIconsEnabled(bool enabled)
    {
        enabled = audioVisualToggle.isOn;
        iconsEnabled = enabled;

        foreach (var icon in activeIcons.Values)
        {
            icon.SetActive(iconsEnabled);
        }
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
}