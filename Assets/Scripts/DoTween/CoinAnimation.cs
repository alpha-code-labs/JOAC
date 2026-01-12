using UnityEngine;
using DG.Tweening;

public class CoinAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    public float rotationDuration = 2f;
    public float flipDuration = 1f;
    public float delayBetweenAnimations = 3f;
    
    [Header("Animation Type")]
    public AnimationType animationType = AnimationType.Rotate;
    
    public enum AnimationType
    {
        Rotate,
        Flip,
        Both
    }

    private void Start()
    {
        StartAnimation();
    }

    private void StartAnimation()
    {
        switch (animationType)
        {
            case AnimationType.Rotate:
                StartRotationAnimation();
                break;
            case AnimationType.Flip:
                StartFlipAnimation();
                break;
            case AnimationType.Both:
                StartCombinedAnimation();
                break;
        }
    }

    private void StartRotationAnimation()
    {
        // Continuous slow rotation around Y-axis
        transform.DORotate(new Vector3(0, 360, 0), rotationDuration, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Restart)
            .SetEase(Ease.Linear);
    }

    private void StartFlipAnimation()
    {
        // Periodic flip animation
        Sequence flipSequence = DOTween.Sequence();
        
        flipSequence.Append(transform.DORotate(new Vector3(0, 180, 0), flipDuration / 2, RotateMode.FastBeyond360)
            .SetEase(Ease.InOutQuad))
            .Append(transform.DORotate(new Vector3(0, 360, 0), flipDuration / 2, RotateMode.FastBeyond360)
            .SetEase(Ease.InOutQuad))
            .AppendInterval(delayBetweenAnimations)
            .SetLoops(-1, LoopType.Restart);
    }

    private void StartCombinedAnimation()
    {
        // Continuous slow rotation with periodic flips
        StartRotationAnimation();
        
        // Add periodic flip on top of rotation
        DOTween.Sequence()
            .AppendInterval(delayBetweenAnimations)
            .AppendCallback(() => DoFlip())
            .SetLoops(-1, LoopType.Restart);
    }

    private void DoFlip()
    {
        // Quick flip while maintaining rotation
        transform.DORotate(transform.eulerAngles + new Vector3(360, 0, 0), 0.5f, RotateMode.FastBeyond360)
            .SetEase(Ease.InOutBack);
    }

    // Alternative flip methods you can use:

    public void DoHorizontalFlip()
    {
        transform.DOScaleX(0, flipDuration / 2)
            .OnComplete(() => {
                // Optional: Change sprite here if you have different sides
                transform.DOScaleX(1, flipDuration / 2);
            });
    }

    public void DoVerticalFlip()
    {
        transform.DOScaleY(0, flipDuration / 2)
            .OnComplete(() => {
                transform.DOScaleY(1, flipDuration / 2);
            });
    }

    // Call this to start a one-time flip animation
    public void TriggerFlip()
    {
        transform.DOPunchRotation(new Vector3(0, 0, 360), 1f, 1, 1f);
    }

    private void OnDestroy()
    {
        // Clean up DOTween animations when object is destroyed
        transform.DOKill();
    }
}