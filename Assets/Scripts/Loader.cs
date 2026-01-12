using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Loader : MonoBehaviour
{
    [Header("UI References")]
    public Slider progressSlider;
    public TextMeshProUGUI progressText;
    public GameObject loadingPanel;
    public Button loadSceneButton;

    [Header("Scene Settings")]
    public string sceneToLoad = "NextScene";
    public float minimumLoadTime = 2f; // Minimum time to show loading screen

    private AsyncOperation asyncOperation;
    private bool isLoading = false;

    void Start()
    {
        // Initialize UI 
        StartLoadingScene();
    }
    public void StartLoadingScene() { if (!isLoading) { StartCoroutine(LoadSceneAsync()); } }
    public void StartLoadingScene(string sceneName) { if (!isLoading) { sceneToLoad = sceneName; StartCoroutine(LoadSceneAsync()); } }
    private IEnumerator LoadSceneAsync()
    {
        isLoading = true;

        // Show loading panel
        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        // Disable load button
        if (loadSceneButton != null)
            loadSceneButton.interactable = false;

        // Start loading the scene
        asyncOperation = SceneManager.LoadSceneAsync(sceneToLoad);
        asyncOperation.allowSceneActivation = false;

        float startTime = Time.time;
        float progress = 0f;

        // Update progress while loading
        while (!asyncOperation.isDone)
        {
            // AsyncOperation.progress goes from 0 to 0.9, then jumps to 1 when complete
            float loadProgress = Mathf.Clamp01(asyncOperation.progress / 0.9f);

            // Ensure minimum loading time
            float timeProgress = (Time.time - startTime) / minimumLoadTime;

            // Use the minimum of both progresses to ensure smooth loading
            progress = Mathf.Min(loadProgress, timeProgress);
            UpdateProgressUI(progress);

            // If loading is complete and minimum time has passed, activate the scene
            if (asyncOperation.progress >= 0.9f && timeProgress >= 1f)
            {
                UpdateProgressUI(1f);
                yield return new WaitForSeconds(0.5f); // Brief pause at 100%
                asyncOperation.allowSceneActivation = true;
            }

            yield return null;
        }

        isLoading = false;
    }

    private void UpdateProgressUI(float progress)
    {
        if (progressSlider != null)
        {
            progressSlider.value = progress;
        }

        if (progressText != null)
        {
            int percentage = Mathf.RoundToInt(progress * 100);
            progressText.text = $"Loading... {percentage}%";
        }
    }

    // Alternative method with callback
    public void LoadSceneWithCallback(string sceneName, System.Action onComplete = null)
    {
        StartCoroutine(LoadSceneAsyncWithCallback(sceneName, onComplete));
    }

    private IEnumerator LoadSceneAsyncWithCallback(string sceneName, System.Action onComplete)
    {
        yield return StartCoroutine(LoadSceneAsyncInternal(sceneName));
        onComplete?.Invoke();
    }

    private IEnumerator LoadSceneAsyncInternal(string sceneName)
    {
        isLoading = true;

        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
        }

        asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        asyncOperation.allowSceneActivation = false;

        float startTime = Time.time;

        while (!asyncOperation.isDone)
        {
            float loadProgress = Mathf.Clamp01(asyncOperation.progress / 0.9f);
            float timeProgress = (Time.time - startTime) / minimumLoadTime;
            float progress = Mathf.Min(loadProgress, timeProgress);
            UpdateProgressUI(progress);

            if (asyncOperation.progress >= 0.9f && timeProgress >= 1f)
            {
                UpdateProgressUI(1f);
                yield return new WaitForSeconds(0.5f); // Brief pause at 100%
                asyncOperation.allowSceneActivation = true;
            }

            yield return null;
        }

        isLoading = false;
    }

    // Method to preload scene without activating it
    public void PreloadScene(string sceneName)
    {
        StartCoroutine(PreloadSceneAsync(sceneName));
    }

    private IEnumerator PreloadSceneAsync(string sceneName)
    {
        AsyncOperation preloadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        preloadOperation.allowSceneActivation = false;

        while (preloadOperation.progress < 0.9f)
        {
            float progress = preloadOperation.progress / 0.9f;
            UpdateProgressUI(progress);
            yield return null;
        }

        UpdateProgressUI(1f);
        Debug.Log($"Scene {sceneName} preloaded successfully");
    }

    // Public method to get loading progress
    public float GetLoadingProgress()
    {
        if (asyncOperation != null)
        {
            return Mathf.Clamp01(asyncOperation.progress / 0.9f);
        }
        return 0f;
    }

    // Public method to check if currently loading
    public bool IsLoading()
    {
        return isLoading;
    }

    void OnDestroy()
    {
        if (loadSceneButton != null)
        {
            loadSceneButton.onClick.RemoveListener(StartLoadingScene);
        }
    }
}