using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    private static UIManager _instance;
    public TMP_Text flashText;
    public TMP_Text runsText;
    public TMP_Text ballLineText;
    public TMP_Text ballLengthText;
    public TMP_Text ballVariationText;

    public TMP_Text remainingRunsText;
    public TMP_Text remainingBallsText;
    public TMP_Text ballSpeedText;
    public Slider playerPositionSlider;
    private float displayDuration = 2f;

    public GameObject GameOverScreen;
    public GameObject WinScreen;
    public GameObject LostScreen;

    public Button ContinueButton;
    public Button MainMenuButton_Win;
    public Button MainMenuButton_Lost;
    public Button RetryButton;

    public LeanAnimator coinAnimator;
    public CoinCollectionAnimator coinCollectionAnimator;
    public int rewardCoins = 30;

    public static UIManager Instance
    {
        get
        {
            return _instance;
        }
    }

    void Awake()
    {
        _instance = this;
    }


    public Slider slider;
    public float sliderScore { get; set; }
    public float playerPositionSliderValue { get; set; }

    void Start()
    {
        ContinueButton.onClick.AddListener(LoadNextScene);
        MainMenuButton_Win.onClick.AddListener(LoadMainMenu);
        MainMenuButton_Lost.onClick.AddListener(LoadMainMenu);
        RetryButton.onClick.AddListener(ResetGame);
        rewardCoins = GameManager.Instance.awarededCoins;
    }
    void Update()
    {
        sliderScore = slider.value;
        playerPositionSliderValue = playerPositionSlider.value;
    }

    public void ShowFlashMessage(string message)
    {
        // Kill any existing tweens to avoid conflicts
        flashText.DOKill();

        // Set the message and activate the gameobject
        flashText.text = message;
        flashText.gameObject.SetActive(true);

        // Get or add CanvasGroup component for alpha control
        CanvasGroup canvasGroup = flashText.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = flashText.gameObject.AddComponent<CanvasGroup>();
        }

        // Start with invisible text
        canvasGroup.alpha = 0f;

        // Create animation sequence
        Sequence flashSequence = DOTween.Sequence();

        // Fade in quickly
        flashSequence.Append(canvasGroup.DOFade(1f, 0.3f).SetEase(Ease.OutQuad));

        // Stay visible for the display duration
        flashSequence.AppendInterval(displayDuration - 0.6f); // Subtract fade times

        // Fade out
        flashSequence.Append(canvasGroup.DOFade(0f, 0.3f).SetEase(Ease.InQuad));

        // Deactivate when complete
        flashSequence.OnComplete(() =>
        {
            flashText.gameObject.SetActive(false);
        });

        // Start the sequence
        flashSequence.Play();
    }

    public void UpdateScore(int remainingRuns, int balls)
    {

        remainingRunsText.text = "" + remainingRuns;
        remainingBallsText.text = "" + balls;

        //scoreText.text = "Need " + remainingRuns + " in " + balls + " balls";
    }

    public void showWinScreen()
    {
        LostScreen.SetActive(false);
        GameOverScreen.SetActive(true);
        WinScreen.SetActive(true);
        coinAnimator.ShowPanel(WinScreen);
        // Start the coin collection animation
        if (coinAnimator != null)
        {
            coinCollectionAnimator.PlayCoinCollectionAnimation(rewardCoins);
        }

        PlayerPrefs.SetString("sceneName", "Transition_Event_2");
        int coins = PlayerPrefs.GetInt("coins");
        coins += rewardCoins;
        PlayerPrefs.SetInt("coins", coins);
    }

    public void showLostScreen()
    {
        WinScreen.SetActive(false);
        GameOverScreen.SetActive(true);
        LostScreen.SetActive(true);
        coinAnimator.ShowPanel(LostScreen);
    }

    public void updateUpcomingBallSpeed(int speed)
    {
        ballSpeedText.text = speed + " km/h";
    }

    public void updateUpcomingBallParameters(int speed, string line, string length, string variation)
    {
        // Update the text values
        ballSpeedText.text = speed + " km/h";
        ballLineText.text = line;
        ballLengthText.text = length;
        ballVariationText.text = variation;

        // Apply punch effect to all text elements
        ApplyPunchEffect(ballSpeedText.transform);
        ApplyPunchEffect(ballLineText.transform);
        ApplyPunchEffect(ballLengthText.transform);
        ApplyPunchEffect(ballVariationText.transform);
    }

    public void ShowRunsScored(int runs)
    {
        runsText.text = "" + runs;
        runsText.gameObject.SetActive(true);

        // Get the CanvasGroup component (add one if it doesn't exist)
        CanvasGroup canvasGroup = runsText.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = runsText.gameObject.AddComponent<CanvasGroup>();
        }

        // Store the original position
        Vector3 originalPosition = runsText.transform.localPosition;

        // Reset initial state
        canvasGroup.alpha = 0f;
        runsText.transform.localPosition = originalPosition;

        // Create the animation sequence
        Sequence animSequence = DOTween.Sequence();

        // Fade in over 0.5 seconds
        animSequence.Append(canvasGroup.DOFade(1f, 0.5f));

        // Slide left and fade out simultaneously (quick animation)
        animSequence.Append(runsText.transform.DOLocalMoveX(originalPosition.x - 100f, 0.3f));
        animSequence.Join(canvasGroup.DOFade(0f, 0.3f));

        // Deactivate the gameobject and reset position when complete
        animSequence.OnComplete(() =>
        {
            runsText.gameObject.SetActive(false);
            runsText.transform.localPosition = originalPosition; // Reset for next use
        });

        // Start the sequence
        animSequence.Play();
    }
    public void LoadNextScene()
    {
        //
        Time.timeScale = 1f;
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu");
    }

    public void ResetGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void ApplyPunchEffect(Transform textTransform)
    {
        // Kill any existing tweens on this transform to avoid conflicts
        textTransform.DOKill();

        // Reset to scale 2.0 first
        textTransform.localScale = Vector3.one * 2.0f;

        // Punch scale effect: jump to 2.05 and back to 2.0
        textTransform.DOPunchScale(Vector3.one * 0.05f, 0.3f, 1, 0.5f)
            .SetEase(Ease.OutBounce);
    }
}
