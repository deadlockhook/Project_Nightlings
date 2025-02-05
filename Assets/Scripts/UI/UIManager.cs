using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
	public static UIManager Instance { get; private set; }
    public GameObject bellIconPrefab;
    //public Transform worldSpaceCanvas;
    private Dictionary<int, GameObject> activeIcons = new Dictionary<int, GameObject>();
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

	private bool isPaused = false;
	private Toggle audioVisualToggle;

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

        ChangeUIState(UIState.MainMenu);
	}

	public void ChangeUIState(UIState state)
	{
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

    }

    private void Update()
	{
		if (winUI.activeSelf || loseUI.activeSelf)
		{
			return;
		}

		if (Input.GetKeyDown(KeyCode.Escape) && !mainMenuUI.activeSelf)
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
		ChangeUIState(UIState.Gameplay);
	}
    
	[HideInInspector] public bool iconsEnabled = false;

    public void ToggleIconsEnabled(bool enabled)
    {
		enabled = audioVisualToggle.isOn;
        iconsEnabled = enabled;

        if (!iconsEnabled)
        {
            foreach (var icon in activeIcons.Values)
            {
                Destroy(icon);
            }
            activeIcons.Clear();
        }
    }

    public void ShowIcon(GameObject iconPrefab, Vector3 position, int eventIndex)
    {
        if (!iconsEnabled || activeIcons.ContainsKey(eventIndex))
            return;

        GameObject icon = Instantiate(iconPrefab, position, Quaternion.identity);
        activeIcons.Add(eventIndex, icon);
    }

    public void HideIcon(int eventIndex)
    {
        if (!iconsEnabled || !activeIcons.ContainsKey(eventIndex))
            return;

        Destroy(activeIcons[eventIndex]);
        activeIcons.Remove(eventIndex);
    }

}
