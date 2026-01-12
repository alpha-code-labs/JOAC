using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class ScrollViewAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    public Transform content;
    public float animationDuration = 0.5f;
    public float delayBetweenItems = 0.1f;
    public Ease animationEase = Ease.OutBack;

    [Header("Animation Type")]
    public bool useScale = true;
    public bool useFade = true;
    public bool useSlideFromRight = false;
    public bool useSlideFromBottom = false;

    [Header("Advanced Settings")]
    public bool animateOnStart = true;
    public bool animateOnEnable = false;

    void Start()
    {
        if (animateOnStart)
            AnimateChildrenIn();
    }

    void OnEnable()
    {
        if (animateOnEnable)
            AnimateChildrenIn();
    }

    public void AnimateChildrenIn()
    {
        // Kill any existing animations on children
        for (int i = 0; i < content.childCount; i++)
        {
            content.GetChild(i).DOKill();
        }

        for (int i = 0; i < content.childCount; i++)
        {
            Transform child = content.GetChild(i);
            float delay = i * delayBetweenItems;

            // Setup initial states
            SetupInitialState(child);

            // Create animation sequence
            Sequence animSequence = DOTween.Sequence();

            if (useScale)
            {
                animSequence.Join(child.DOScale(Vector3.one, animationDuration)
                    .SetEase(animationEase));
            }

            if (useFade)
            {
                CanvasGroup canvasGroup = GetOrAddCanvasGroup(child);
                animSequence.Join(canvasGroup.DOFade(1f, animationDuration)
                    .SetEase(Ease.OutQuart));
            }

            if (useSlideFromRight)
            {
                RectTransform rectTransform = child.GetComponent<RectTransform>();
                Vector2 originalPos = rectTransform.anchoredPosition;
                rectTransform.anchoredPosition = new Vector2(originalPos.x + 300f, originalPos.y);

                animSequence.Join(rectTransform.DOAnchorPos(originalPos, animationDuration)
                    .SetEase(animationEase));
            }

            if (useSlideFromBottom)
            {
                RectTransform rectTransform = child.GetComponent<RectTransform>();
                Vector2 originalPos = rectTransform.anchoredPosition;
                rectTransform.anchoredPosition = new Vector2(originalPos.x, originalPos.y - 100f);

                animSequence.Join(rectTransform.DOAnchorPos(originalPos, animationDuration)
                    .SetEase(animationEase));
            }

            // Set the delay for this child's animation
            animSequence.SetDelay(delay);
        }
    }

    public void AnimateChildrenOut(System.Action onComplete = null)
    {
        int animationsRemaining = content.childCount;

        for (int i = 0; i < content.childCount; i++)
        {
            Transform child = content.GetChild(i);
            float delay = i * (delayBetweenItems * 0.5f); // Faster out animation

            Sequence animSequence = DOTween.Sequence();

            if (useScale)
            {
                animSequence.Join(child.DOScale(Vector3.zero, animationDuration * 0.7f)
                    .SetEase(Ease.InBack));
            }

            if (useFade)
            {
                CanvasGroup canvasGroup = GetOrAddCanvasGroup(child);
                animSequence.Join(canvasGroup.DOFade(0f, animationDuration * 0.7f)
                    .SetEase(Ease.InQuart));
            }

            animSequence.SetDelay(delay);
            animSequence.OnComplete(() =>
            {
                animationsRemaining--;
                if (animationsRemaining <= 0)
                    onComplete?.Invoke();
            });
        }
    }

    private void SetupInitialState(Transform child)
    {
        if (useScale)
        {
            child.localScale = Vector3.zero;
        }

        if (useFade)
        {
            CanvasGroup canvasGroup = GetOrAddCanvasGroup(child);
            canvasGroup.alpha = 0f;
        }

        // Note: Position animations are set up in the animation method
        // to preserve original positions
    }

    private CanvasGroup GetOrAddCanvasGroup(Transform child)
    {
        CanvasGroup canvasGroup = child.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = child.gameObject.AddComponent<CanvasGroup>();
        }
        return canvasGroup;
    }

    // Public methods for external control
    public void AnimateIn() => AnimateChildrenIn();
    public void AnimateOut() => AnimateChildrenOut();

    // For dynamically added items
    public void AnimateNewItem(Transform newItem)
    {
        SetupInitialState(newItem);

        Sequence animSequence = DOTween.Sequence();

        if (useScale)
        {
            animSequence.Join(newItem.DOScale(Vector3.one, animationDuration)
                .SetEase(animationEase));
        }

        if (useFade)
        {
            CanvasGroup canvasGroup = GetOrAddCanvasGroup(newItem);
            animSequence.Join(canvasGroup.DOFade(1f, animationDuration));
        }
    }
}