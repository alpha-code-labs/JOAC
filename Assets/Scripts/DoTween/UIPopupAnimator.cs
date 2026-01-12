using UnityEngine;
using DG.Tweening;
using System.Collections;

public class UIPopupAnimator : MonoBehaviour
{
    public RectTransform targetUI;
    public float initialDelay = 0f;
    public float popDuration = 0.3f;
    public float waitDuration = 1.0f;
    public float moveDuration = 0.5f;
    public float moveOffset = 300f;

    private Vector2 initialAnchoredPos;

    void OnEnable()
    {
        if (targetUI == null) return;
        StartCoroutine(InitializeAfterLayout());
    }

    private IEnumerator InitializeAfterLayout()
    {
        yield return new WaitForEndOfFrame();

        initialAnchoredPos = targetUI.anchoredPosition;
        Debug.Log("initialAnchoredPos: " + initialAnchoredPos);
        PlayPopupSequence();
    }

    public void PlayPopupSequence()
    {
        // Reset position and scale
        targetUI.localScale = Vector3.zero;
        targetUI.anchoredPosition = initialAnchoredPos;

        // Create the sequence
        Sequence popupSequence = DOTween.Sequence();

        // Add initial delay if specified
        if (initialDelay > 0f)
        {
            popupSequence.PrependInterval(initialDelay);
        }

        popupSequence
            .Append(targetUI.DOScale(Vector3.one, popDuration).SetEase(Ease.OutBack))
            .OnComplete(() => Debug.Log("Pop animation completed")) // Debug callback
            .AppendInterval(waitDuration)
            .OnComplete(() => Debug.Log("Wait duration completed")) // Debug callback
            .Append(targetUI.DOAnchorPosY(initialAnchoredPos.y + moveOffset, moveDuration).SetEase(Ease.InOutCubic))
            .OnComplete(() => Debug.Log("Move animation completed")); // Debug callback
    }
}