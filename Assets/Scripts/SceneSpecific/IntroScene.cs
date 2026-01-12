using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections;

public class IntroScene : MonoBehaviour
{

    public VideoPlayer videoPlayer; // Assign in Inspector
    public VideoClip videoClip1;    // First clip
    public VideoClip videoClip2;    // Second clip

    public Button skipButton; // Skip button for the second video
    public Image fadeImage; // Image for fade in/out effect
    public CanvasGroup fadeGroup; // CanvasGroup for fade effect
    public float fadeDuration = 1f; // Duration for fade effect
    public string nextSceneName = "NextScene"; // Name of the next scene to load


    private bool isFirstVideoPlaying = true;

    void Start()
    {
        // Start the intro scene with fade in
        StartCoroutine(FadeIn());
        // Set the skip button behavior
        skipButton.gameObject.SetActive(false); // Initially hide the skip button
        skipButton.onClick.AddListener(SkipVideo);
        PlayFirstVideo();
        videoPlayer.loopPointReached += OnVideoFinished; // Listen for end of video
    }

    void PlayFirstVideo()
    {
        videoPlayer.clip = videoClip1;
        videoPlayer.Play();
        isFirstVideoPlaying = true;
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        if (isFirstVideoPlaying)
        {
            StartCoroutine(FadeOut());
            // Play second video
            videoPlayer.clip = videoClip2;
            videoPlayer.Play();
            isFirstVideoPlaying = false;
            skipButton.gameObject.SetActive(true);
        }
        else
        {
            // Disable VideoPlayer after second video
            SkipVideo();
        }
    }


    private void SkipVideo()
    {
        // Stop the second video if skipped
        videoPlayer.Stop();
        StartCoroutine(FadeOut());
        skipButton.gameObject.SetActive(false);
        SceneManager.LoadScene(nextSceneName);
    }


    private IEnumerator FadeIn()
    {
        // Fade from black to transparent (alpha = 0)
        float timeElapsed = 0f;
        while (timeElapsed < fadeDuration)
        {
            fadeGroup.alpha = 1 - (timeElapsed / fadeDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        fadeGroup.alpha = 0;
    }

    private IEnumerator FadeOut()
    {
        // Fade from transparent to black (alpha = 1)
        float timeElapsed = 0f;
        while (timeElapsed < fadeDuration)
        {
            fadeGroup.alpha = timeElapsed / fadeDuration;
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        fadeGroup.alpha = 1;
    }
}