using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public enum UIState
    {
        MainMenu,
        PauseMenu,
        Gameplay,
        Options
    }

    [Header("UI Screens")]
    public GameObject mainMenuUI;
    public GameObject pauseMenuUI;
    public GameObject gameplayUI;
    public GameObject optionsUI;

    private bool isPaused = false;

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
        ChangeUIState(UIState.MainMenu);
    }

    public void ChangeUIState(UIState state)
    {
        mainMenuUI.SetActive(false);
        pauseMenuUI.SetActive(false);
        gameplayUI.SetActive(false);
        optionsUI.SetActive(false);

        switch (state)
        {
            case UIState.MainMenu:
                Cursor.lockState = CursorLockMode.None;
                Time.timeScale = 0f;
                mainMenuUI.SetActive(true);
                break;

            case UIState.PauseMenu:
                Cursor.lockState = CursorLockMode.None;
                Time.timeScale = 0f;
                pauseMenuUI.SetActive(true);
                break;

            case UIState.Gameplay:
                Time.timeScale = 1f;
                gameplayUI.SetActive(true);
                Cursor.lockState = CursorLockMode.Locked;
                break;

            case UIState.Options:
                Time.timeScale = 0f;
                optionsUI.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
                break;
        }
    }

    private void Update()
    {
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
        isPaused = true;
        ChangeUIState(UIState.PauseMenu);
    }

    public bool IsGamePaused()
    {
        return isPaused;
    }

    public void ResumeGame()
    {
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
}
