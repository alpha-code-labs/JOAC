using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening; // Add DOTween namespace

public class UIManagerGamePlay_1 : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text flashText;
    public TMP_Text hitScoreText;
    public TMP_Text leftBallsText;

    [Header("Flash Animation Settings")]
    public float fadeInDuration = 0.3f;
    public float holdDuration = 2f;
    public float fadeOutDuration = 0.5f;
    public Ease fadeInEase = Ease.OutBack;
    public Ease fadeOutEase = Ease.InQuart;

    [Header("Optional Flash Effects")]
    public bool useScaleEffect = true;
    public Vector3 scaleUpAmount = new Vector3(1.2f, 1.2f, 1f);
    public bool usePunchEffect = false;
    public float punchStrength = 0.3f;

    private Sequence currentFlashSequence;
    private static UIManagerGamePlay_1 _instance;

    public static UIManagerGamePlay_1 Instance
    {
        get
        {
            return _instance;
        }
    }

    void Awake()
    {
        _instance = this;

        // Initialize flash text as invisible
        if (flashText != null)
        {
            flashText.alpha = 0f;
            flashText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (PauseManager.Instance != null && PauseManager.Instance.IsPaused)
            return;
    }
    public void UpdateScore(int hitBalls, int leftBalls)
    {
        hitScoreText.text = "" + hitBalls;
        leftBallsText.text = "" + leftBalls;
    }

    public void ShowFlashMessage(string message)
    {
        // Kill any existing flash animation
        if (currentFlashSequence != null && currentFlashSequence.IsActive())
        {
            currentFlashSequence.Kill();
        }

        FlashMessageWithDOTween(message);
    }

    private void FlashMessageWithDOTween(string message)
    {
        // Set the message and prepare the text
        flashText.text = message;
        flashText.gameObject.SetActive(true);
        flashText.alpha = 0f;
        flashText.transform.localScale = Vector3.one;

        // Create the flash sequence
        currentFlashSequence = DOTween.Sequence();

        // Phase 1: Fade in with optional scale effect
        if (useScaleEffect)
        {
            flashText.transform.localScale = Vector3.zero;
            currentFlashSequence.Append(flashText.DOFade(1f, fadeInDuration).SetEase(fadeInEase));
            currentFlashSequence.Join(flashText.transform.DOScale(scaleUpAmount, fadeInDuration * 0.7f).SetEase(fadeInEase));
            currentFlashSequence.Append(flashText.transform.DOScale(Vector3.one, fadeInDuration * 0.3f).SetEase(Ease.OutBounce));
        }
        else
        {
            currentFlashSequence.Append(flashText.DOFade(1f, fadeInDuration).SetEase(fadeInEase));
        }

        // Optional: Add punch effect during fade in
        if (usePunchEffect)
        {
            currentFlashSequence.Join(flashText.transform.DOPunchScale(Vector3.one * punchStrength, fadeInDuration, 2, 0.5f));
        }

        // Phase 2: Hold the message
        currentFlashSequence.AppendInterval(holdDuration);

        // Phase 3: Fade out
        currentFlashSequence.Append(flashText.DOFade(0f, fadeOutDuration).SetEase(fadeOutEase));

        // Cleanup: Hide the game object when done
        currentFlashSequence.OnComplete(() =>
        {
            flashText.gameObject.SetActive(false);
        });
    }

    // Alternative flash method with different effects
    public void ShowFlashMessageWithSlide(string message)
    {
        // Kill any existing flash animation
        if (currentFlashSequence != null && currentFlashSequence.IsActive())
        {
            currentFlashSequence.Kill();
        }

        // Set the message and prepare the text
        flashText.text = message;
        flashText.gameObject.SetActive(true);
        flashText.alpha = 1f;

        // Store original position
        Vector3 originalPos = flashText.transform.localPosition;
        Vector3 startPos = originalPos + new Vector3(0, 100f, 0); // Start above
        Vector3 endPos = originalPos + new Vector3(0, -100f, 0);  // End below

        flashText.transform.localPosition = startPos;

        // Create slide sequence
        currentFlashSequence = DOTween.Sequence();

        // Slide in from top
        currentFlashSequence.Append(flashText.transform.DOLocalMove(originalPos, fadeInDuration).SetEase(Ease.OutBack));

        // Hold
        currentFlashSequence.AppendInterval(holdDuration);

        // Slide out to bottom with fade
        currentFlashSequence.Append(flashText.transform.DOLocalMove(endPos, fadeOutDuration).SetEase(Ease.InBack));
        currentFlashSequence.Join(flashText.DOFade(0f, fadeOutDuration));

        // Cleanup
        currentFlashSequence.OnComplete(() =>
        {
            flashText.gameObject.SetActive(false);
            flashText.transform.localPosition = originalPos; // Reset position
            flashText.alpha = 1f; // Reset alpha
        });
    }

    // Quick flash method for immediate feedback
    public void ShowQuickFlash(string message, float duration = 1f)
    {
        // Kill any existing flash animation
        if (currentFlashSequence != null && currentFlashSequence.IsActive())
        {
            currentFlashSequence.Kill();
        }

        // Set the message
        flashText.text = message;
        flashText.gameObject.SetActive(true);
        flashText.alpha = 0f;

        // Quick flash sequence
        currentFlashSequence = DOTween.Sequence();
        currentFlashSequence.Append(flashText.DOFade(1f, 0.1f));
        currentFlashSequence.AppendInterval(duration);
        currentFlashSequence.Append(flashText.DOFade(0f, 0.2f));
        currentFlashSequence.OnComplete(() => flashText.gameObject.SetActive(false));
    }

    // Pulsing flash effect
    public void ShowPulsingFlash(string message, int pulseCount = 3)
    {
        // Kill any existing flash animation
        if (currentFlashSequence != null && currentFlashSequence.IsActive())
        {
            currentFlashSequence.Kill();
        }

        flashText.text = message;
        flashText.gameObject.SetActive(true);
        flashText.alpha = 0f;

        currentFlashSequence = DOTween.Sequence();

        // Create pulsing effect
        for (int i = 0; i < pulseCount; i++)
        {
            currentFlashSequence.Append(flashText.DOFade(1f, 0.2f));
            currentFlashSequence.Append(flashText.DOFade(0.3f, 0.2f));
        }

        // Final fade out
        currentFlashSequence.Append(flashText.DOFade(0f, fadeOutDuration));
        currentFlashSequence.OnComplete(() => flashText.gameObject.SetActive(false));
    }

    void OnDestroy()
    {
        // Clean up DOTween animations
        if (currentFlashSequence != null && currentFlashSequence.IsActive())
        {
            currentFlashSequence.Kill();
        }

        // Kill any tweens on the flash text
        if (flashText != null)
        {
            flashText.DOKill();
        }
    }
}