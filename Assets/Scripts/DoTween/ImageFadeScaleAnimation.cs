using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ImageFadeScaleAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    public float fadeInDuration = 1f;
    public float scaleInDuration = 1f;
    public float holdDuration = 3f;
    public float fadeOutDuration = 1f;

    [Header("Scale Settings")]
    public float startScale = 0.5f; // How small it starts (0.5 = 50% of original size)

    [Header("Animation Control")]
    public bool playOnStart = true;
    public bool loopAnimation = false;
    public float delayBeforeStart = 0f;

    [Header("Easing")]
    public Ease fadeInEase = Ease.OutQuad;
    public Ease scaleInEase = Ease.OutBack;
    public Ease fadeOutEase = Ease.InQuad;

    private Vector3 originalScale;
    private CanvasGroup canvasGroup;
    private Image imageComponent;
    private bool isAnimating = false;

    private void Start()
    {
        SetupComponents();

        if (playOnStart)
        {
            if (delayBeforeStart > 0)
            {
                DOVirtual.DelayedCall(delayBeforeStart, () => StartAnimation());
            }
            else
            {
                StartAnimation();
            }
        }
    }

    private void SetupComponents()
    {
        // Store original scale
        originalScale = transform.localScale;

        // Get or add CanvasGroup for fade control
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Get Image component (optional, for additional control)
        imageComponent = GetComponent<Image>();

        // Set initial state
        ResetToStartState();
    }

    private void ResetToStartState()
    {
        // Start scaled down and faded out
        transform.localScale = originalScale * startScale;
        canvasGroup.alpha = 0f;
        isAnimating = false;
    }

    public void StartAnimation()
    {
        if (isAnimating) return;

        isAnimating = true;
        ResetToStartState();

        // Create the animation sequence
        Sequence animationSequence = DOTween.Sequence();

        // Phase 1: Fade in and scale up simultaneously
        animationSequence.Append(
            DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 1f, fadeInDuration)
                .SetEase(fadeInEase)
        );

        animationSequence.Join(
            transform.DOScale(originalScale, scaleInDuration)
                .SetEase(scaleInEase)
        );

        // Phase 2: Hold at full visibility and scale
        animationSequence.AppendInterval(holdDuration);

        // Phase 3: Fade out
        animationSequence.Append(
            DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 0f, fadeOutDuration)
                .SetEase(fadeOutEase)
        );

        // On complete
        animationSequence.OnComplete(() =>
        {
            isAnimating = false;
            OnAnimationComplete();
        });

        // Loop if needed
        if (loopAnimation)
        {
            animationSequence.SetLoops(-1, LoopType.Restart);
        }
    }

    private void OnAnimationComplete()
    {
        // Override this method or add UnityEvents here
        // You can add custom logic when animation completes
    }

    // Alternative animation styles

    public void StartBounceAnimation()
    {
        if (isAnimating) return;

        isAnimating = true;
        ResetToStartState();

        Sequence bounceSequence = DOTween.Sequence();

        // Fade in first
        bounceSequence.Append(canvasGroup.DOFade(1f, fadeInDuration * 0.5f));

        // Then bounce scale in
        bounceSequence.Append(transform.DOScale(originalScale * 1.2f, scaleInDuration * 0.3f).SetEase(Ease.OutQuad));
        bounceSequence.Append(transform.DOScale(originalScale * 0.9f, scaleInDuration * 0.2f).SetEase(Ease.InQuad));
        bounceSequence.Append(transform.DOScale(originalScale, scaleInDuration * 0.5f).SetEase(Ease.OutBack));

        // Hold
        bounceSequence.AppendInterval(holdDuration);

        // Fade out
        bounceSequence.Append(canvasGroup.DOFade(0f, fadeOutDuration));

        bounceSequence.OnComplete(() =>
        {
            isAnimating = false;
            OnAnimationComplete();
        });
    }

    public void StartElasticAnimation()
    {
        if (isAnimating) return;

        isAnimating = true;
        ResetToStartState();

        Sequence elasticSequence = DOTween.Sequence();

        // Simultaneous fade and elastic scale
        elasticSequence.Append(canvasGroup.DOFade(1f, fadeInDuration).SetEase(fadeInEase));
        elasticSequence.Join(transform.DOScale(originalScale, scaleInDuration).SetEase(Ease.OutElastic));

        // Hold
        elasticSequence.AppendInterval(holdDuration);

        // Scale down with fade out
        elasticSequence.Append(transform.DOScale(originalScale * 0.8f, fadeOutDuration * 0.5f).SetEase(Ease.InBack));
        elasticSequence.Join(canvasGroup.DOFade(0f, fadeOutDuration).SetEase(fadeOutEase));

        elasticSequence.OnComplete(() =>
        {
            isAnimating = false;
            OnAnimationComplete();
        });
    }

    public void StartPunchAnimation()
    {
        if (isAnimating) return;

        isAnimating = true;
        ResetToStartState();

        Sequence punchSequence = DOTween.Sequence();

        // Quick fade and scale in
        punchSequence.Append(canvasGroup.DOFade(1f, fadeInDuration * 0.5f));
        punchSequence.Join(transform.DOScale(originalScale, scaleInDuration * 0.5f).SetEase(Ease.OutBack));

        // Punch effect
        punchSequence.Append(transform.DOPunchScale(Vector3.one * 0.1f, 0.5f, 1, 1f));

        // Hold
        punchSequence.AppendInterval(holdDuration - 0.5f);

        // Fade out
        punchSequence.Append(canvasGroup.DOFade(0f, fadeOutDuration));

        punchSequence.OnComplete(() =>
        {
            isAnimating = false;
            OnAnimationComplete();
        });
    }

    // Control methods
    public void StopAnimation()
    {
        transform.DOKill();
        canvasGroup.DOKill();
        isAnimating = false;
    }

    public void ResetAndRestart()
    {
        StopAnimation();
        StartAnimation();
    }

    public void SetVisible()
    {
        StopAnimation();
        transform.localScale = originalScale;
        canvasGroup.alpha = 1f;
    }

    public void SetHidden()
    {
        StopAnimation();
        ResetToStartState();
    }

    // Property setters for runtime modification
    public void SetHoldDuration(float newDuration)
    {
        holdDuration = newDuration;
    }

    public void SetStartScale(float newStartScale)
    {
        startScale = newStartScale;
    }

    public void SetLooping(bool shouldLoop)
    {
        loopAnimation = shouldLoop;
    }

    private void OnDestroy()
    {
        // Clean up DOTween animations
        transform.DOKill();
        if (canvasGroup != null)
        {
            canvasGroup.DOKill();
        }
    }

    // Public method to check if animation is running
    public bool IsAnimating()
    {
        return isAnimating;
    }
}