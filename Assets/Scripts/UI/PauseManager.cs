using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    [Header("Pause Menu UI")]
    public GameObject pauseMenuPanel;
    public GameObject pauseButton;

    [Header("Scene Management")]
    public string mainMenuSceneName = "MainMenu";

    private bool isPaused = false;
    private List<AudioSource> gameAudioSources = new List<AudioSource>();

    void Awake()
    {
        // Singleton pattern - only one PauseManager should exist
        if (Instance == null)
        {
            Instance = this;
            // Don't destroy this object when loading new scenes (if needed for transitions)
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Make sure pause menu is hidden at start
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        // Auto-find all AudioSources in the scene
        FindAllAudioSources();
    }

    void Update()
    {
        // Allow pausing with ESC key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; // Freeze the game

        // Show pause menu
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);

        // Hide pause button
        if (pauseButton != null)
            pauseButton.SetActive(false);

        // Pause audio sources
        PauseAudioSources();

        Debug.Log("Game Paused");
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // Resume normal time

        // Hide pause menu
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        // Show pause button
        if (pauseButton != null)
            pauseButton.SetActive(true);

        // Resume audio sources
        ResumeAudioSources();

        Debug.Log("Game Resumed");
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f; // Reset time scale before loading new scene
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f; // Reset time scale
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f; // Reset time scale

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void FindAllAudioSources()
    {
        // Find all AudioSources in the scene automatically
        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();
        gameAudioSources.Clear();

        foreach (AudioSource audioSource in allAudioSources)
        {
            // Exclude UI audio sources (optional - you can modify this logic)
            if (!audioSource.transform.IsChildOf(transform))
            {
                gameAudioSources.Add(audioSource);
            }
        }
    }

    private void PauseAudioSources()
    {
        foreach (AudioSource audioSource in gameAudioSources)
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Pause();
            }
        }
    }

    private void ResumeAudioSources()
    {
        foreach (AudioSource audioSource in gameAudioSources)
        {
            if (audioSource != null)
            {
                audioSource.UnPause();
            }
        }
    }

    // Method to manually add specific audio sources if needed
    public void AddAudioSource(AudioSource audioSource)
    {
        if (audioSource != null && !gameAudioSources.Contains(audioSource))
        {
            gameAudioSources.Add(audioSource);
        }
    }

    // Method to remove specific audio sources if needed
    public void RemoveAudioSource(AudioSource audioSource)
    {
        if (gameAudioSources.Contains(audioSource))
        {
            gameAudioSources.Remove(audioSource);
        }
    }

    // Property to check if game is paused
    public bool IsPaused
    {
        get { return isPaused; }
    }

    void OnDestroy()
    {
        // Reset time scale when this object is destroyed
        Time.timeScale = 1f;
    }
}