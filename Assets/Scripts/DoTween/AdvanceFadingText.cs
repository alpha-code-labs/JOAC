using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections;

public class AdvancedFadingText : MonoBehaviour
{
    [System.Serializable]
    public class TextLine
    {
        public string text;
        public float customFadeIn = -1f; // -1 uses default
        public float customDisplay = -1f;
        public float customFadeOut = -1f;
    }

    [Header("Components")]
    public TextMeshProUGUI textComponent;

    [Header("Text Content")]
    public TextLine[] textLines;

    [Header("Default Timings")]
    public float defaultFadeInDuration = 1f;
    public float defaultDisplayDuration = 2f;
    public float defaultFadeOutDuration = 1f;
    public float delayBetweenLines = 0.5f;

    [Header("Animation Style")]
    public Ease fadeEase = Ease.OutQuart;
    public bool includeScaleEffect = false;
    public bool includeSlideEffect = false;
    public Vector2 slideOffset = new Vector2(0, 50);

    [Header("Options")]
    public bool autoStart = true;
    public bool loop = false;

    private Vector2 originalPosition;
    private Vector3 originalScale;

    void Start()
    {
        if (textComponent == null)
            textComponent = GetComponent<TextMeshProUGUI>();

        originalPosition = textComponent.rectTransform.anchoredPosition;
        originalScale = textComponent.transform.localScale;

        if (autoStart)
            StartTextSequence();
    }

    public void StartTextSequence()
    {
        StartCoroutine(DisplayAdvancedTextSequence());
    }

    IEnumerator DisplayAdvancedTextSequence()
    {
        do
        {
            foreach (TextLine line in textLines)
            {
                // Get timings (use custom or default)
                float fadeIn = line.customFadeIn > 0 ? line.customFadeIn : defaultFadeInDuration;
                float display = line.customDisplay > 0 ? line.customDisplay : defaultDisplayDuration;
                float fadeOut = line.customFadeOut > 0 ? line.customFadeOut : defaultFadeOutDuration;

                // Setup initial state
                textComponent.text = line.text;
                SetupInitialState();

                // Animate in
                yield return AnimateIn(fadeIn).WaitForCompletion();

                // Display duration
                yield return new WaitForSeconds(display);

                // Animate out
                yield return AnimateOut(fadeOut).WaitForCompletion();

                // Delay between lines
                yield return new WaitForSeconds(delayBetweenLines);
            }
        } while (loop);
    }

    void SetupInitialState()
    {
        textComponent.alpha = 0f;

        if (includeScaleEffect)
            textComponent.transform.localScale = Vector3.zero;

        if (includeSlideEffect)
            textComponent.rectTransform.anchoredPosition = originalPosition + (Vector2)slideOffset;
    }

    Tween AnimateIn(float duration)
    {
        Sequence sequence = DOTween.Sequence();

        // Fade in
        sequence.Append(textComponent.DOFade(1f, duration).SetEase(fadeEase));

        // Scale effect
        if (includeScaleEffect)
        {
            sequence.Join(textComponent.transform.DOScale(originalScale, duration).SetEase(fadeEase));
        }

        // Slide effect
        if (includeSlideEffect)
        {
            sequence.Join(textComponent.rectTransform.DOAnchorPos(originalPosition, duration).SetEase(fadeEase));
        }

        return sequence;
    }

    Tween AnimateOut(float duration)
    {
        Sequence sequence = DOTween.Sequence();

        // Fade out
        sequence.Append(textComponent.DOFade(0f, duration).SetEase(fadeEase));

        // Scale effect
        if (includeScaleEffect)
        {
            sequence.Join(textComponent.transform.DOScale(Vector3.zero, duration).SetEase(fadeEase));
        }

        // Slide effect
        if (includeSlideEffect)
        {
            Vector2 exitPosition = originalPosition - (Vector2)slideOffset;
            sequence.Join(textComponent.rectTransform.DOAnchorPos(exitPosition, duration).SetEase(fadeEase));
        }

        return sequence;
    }

    // Public methods for external control
    public void StopSequence()
    {
        StopAllCoroutines();
        textComponent.DOKill();
    }

    public void RestartSequence()
    {
        StopSequence();
        StartTextSequence();
    }
}