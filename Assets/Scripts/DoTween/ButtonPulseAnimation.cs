using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ButtonPulseAnimation : MonoBehaviour
{
    [Header("Pulse Settings")]
    public float pulseDuration = 1f;
    public float pulseScale = 1.1f;
    public float delayBetweenPulses = 2f;

    [Header("Pulse Type")]
    public PulseType pulseType = PulseType.Scale;

    [Header("Color Pulse Settings (if using Color pulse)")]
    public Color pulseColor = Color.yellow;
    public bool useImageComponent = true; // true for Image, false for Button

    [Header("Glow Settings (if using Glow pulse)")]
    public GameObject glowEffect; // Optional glow GameObject

    public enum PulseType
    {
        Scale,
        Color,
        Glow,
        ScaleAndColor,
        All
    }

    private Vector3 originalScale;
    private Color originalColor;
    private Image imageComponent;
    private Button buttonComponent;
    private Graphic targetGraphic;

    private void Start()
    {
        // Store original values
        originalScale = transform.localScale;

        // Get components
        imageComponent = GetComponent<Image>();
        buttonComponent = GetComponent<Button>();

        // Determine which graphic to use for color changes
        if (useImageComponent && imageComponent != null)
        {
            targetGraphic = imageComponent;
        }
        else if (buttonComponent != null && buttonComponent.targetGraphic != null)
        {
            targetGraphic = buttonComponent.targetGraphic;
        }

        if (targetGraphic != null)
        {
            originalColor = targetGraphic.color;
        }

        StartPulseAnimation();
    }

    private void StartPulseAnimation()
    {
        switch (pulseType)
        {
            case PulseType.Scale:
                StartScalePulse();
                break;
            case PulseType.Color:
                StartColorPulse();
                break;
            case PulseType.Glow:
                StartGlowPulse();
                break;
            case PulseType.ScaleAndColor:
                StartScalePulse();
                StartColorPulse();
                break;
            case PulseType.All:
                StartScalePulse();
                StartColorPulse();
                StartGlowPulse();
                break;
        }
    }

    private void StartScalePulse()
    {
        Sequence scaleSequence = DOTween.Sequence();

        scaleSequence.Append(transform.DOScale(originalScale * pulseScale, pulseDuration / 2)
            .SetEase(Ease.InOutSine))
            .Append(transform.DOScale(originalScale, pulseDuration / 2)
            .SetEase(Ease.InOutSine))
            .AppendInterval(delayBetweenPulses)
            .SetLoops(-1, LoopType.Restart);
    }

    private void StartColorPulse()
    {
        if (targetGraphic == null) return;

        Sequence colorSequence = DOTween.Sequence();

        colorSequence.Append(targetGraphic.DOColor(pulseColor, pulseDuration / 2)
            .SetEase(Ease.InOutSine))
            .Append(targetGraphic.DOColor(originalColor, pulseDuration / 2)
            .SetEase(Ease.InOutSine))
            .AppendInterval(delayBetweenPulses)
            .SetLoops(-1, LoopType.Restart);
    }

    private void StartGlowPulse()
    {
        if (glowEffect == null) return;

        // Ensure glow starts invisible
        glowEffect.SetActive(true);
        CanvasGroup glowCanvasGroup = glowEffect.GetComponent<CanvasGroup>();
        if (glowCanvasGroup == null)
        {
            glowCanvasGroup = glowEffect.AddComponent<CanvasGroup>();
        }
        glowCanvasGroup.alpha = 0f;

        Sequence glowSequence = DOTween.Sequence();

        glowSequence.Append(glowCanvasGroup.DOFade(1f, pulseDuration / 2)
            .SetEase(Ease.InOutSine))
            .Append(glowCanvasGroup.DOFade(0f, pulseDuration / 2)
            .SetEase(Ease.InOutSine))
            .AppendInterval(delayBetweenPulses)
            .SetLoops(-1, LoopType.Restart);
    }

    // Alternative pulse methods for different effects

    public void DoPunchPulse()
    {
        transform.DOPunchScale(Vector3.one * 0.1f, 0.5f, 1, 1f);
    }

    public void DoElasticPulse()
    {
        transform.DOScale(originalScale * pulseScale, pulseDuration / 2)
            .SetEase(Ease.OutElastic)
            .OnComplete(() =>
            {
                transform.DOScale(originalScale, pulseDuration / 2)
                    .SetEase(Ease.InElastic);
            });
    }

    public void DoHeartbeatPulse()
    {
        Sequence heartbeat = DOTween.Sequence();

        heartbeat.Append(transform.DOScale(originalScale * 1.05f, 0.1f))
            .Append(transform.DOScale(originalScale, 0.1f))
            .Append(transform.DOScale(originalScale * 1.1f, 0.1f))
            .Append(transform.DOScale(originalScale, 0.3f))
            .AppendInterval(1f)
            .SetLoops(-1, LoopType.Restart);
    }

    public void DoBreathingPulse()
    {
        transform.DOScale(originalScale * pulseScale, pulseDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    // Interactive methods
    public void OnButtonHover()
    {
        transform.DOScale(originalScale * 1.05f, 0.2f)
            .SetEase(Ease.OutBack);
    }

    public void OnButtonExit()
    {
        transform.DOScale(originalScale, 0.2f)
            .SetEase(Ease.OutBack);
    }

    public void OnButtonClick()
    {
        transform.DOPunchScale(Vector3.one * 0.15f, 0.3f, 1, 1f);
    }

    // Control methods
    public void StopPulse()
    {
        transform.DOKill();
        if (targetGraphic != null)
        {
            targetGraphic.DOKill();
        }
        if (glowEffect != null)
        {
            glowEffect.GetComponent<CanvasGroup>()?.DOKill();
        }

        // Reset to original state
        transform.localScale = originalScale;
        if (targetGraphic != null)
        {
            targetGraphic.color = originalColor;
        }
    }

    public void ResumePulse()
    {
        StopPulse();
        StartPulseAnimation();
    }

    private void OnDestroy()
    {
        // Clean up DOTween animations
        transform.DOKill();
        if (targetGraphic != null)
        {
            targetGraphic.DOKill();
        }
        if (glowEffect != null)
        {
            glowEffect.GetComponent<CanvasGroup>()?.DOKill();
        }
    }
}