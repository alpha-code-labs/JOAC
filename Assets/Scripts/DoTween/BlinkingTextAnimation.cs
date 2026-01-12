using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class BlinkingTextButton : MonoBehaviour
{
    [Header("Blink Settings")]
    public float blinkDuration = 0.5f;
    public float minAlpha = 0.3f;
    public float maxAlpha = 1f;
    public bool startBlinkingOnAwake = true;

    [Header("Optional: Custom Colors")]
    public bool useColorBlink = false;
    public Color color1 = Color.white;
    public Color color2 = Color.red;

    private TextMeshProUGUI buttonText;
    private Button button;
    private Tween blinkTween;

    void Start()
    {
        // Get the TextMeshProUGUI component (works for both Button with TMP child or TMP component directly)
        buttonText = GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText == null)
            buttonText = GetComponent<TextMeshProUGUI>();

        button = GetComponent<Button>();

        if (buttonText == null)
        {
            Debug.LogError("No TextMeshProUGUI component found on " + gameObject.name);
            return;
        }

        if (startBlinkingOnAwake)
        {
            StartBlinking();
        }
    }

    public void StartBlinking()
    {
        StopBlinking(); // Stop any existing blink

        if (useColorBlink)
        {
            // Color-based blinking
            blinkTween = buttonText.DOColor(color2, blinkDuration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }
        else
        {
            // Alpha-based blinking
            blinkTween = buttonText.DOFade(minAlpha, blinkDuration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }
    }

    public void StopBlinking()
    {
        if (blinkTween != null)
        {
            blinkTween.Kill();

            // Reset to original state
            if (useColorBlink)
            {
                buttonText.color = color1;
            }
            else
            {
                Color originalColor = buttonText.color;
                originalColor.a = maxAlpha;
                buttonText.color = originalColor;
            }
        }
    }

    public void ToggleBlinking()
    {
        if (blinkTween != null && blinkTween.IsActive())
        {
            StopBlinking();
        }
        else
        {
            StartBlinking();
        }
    }

    // Call this when button is clicked to stop blinking
    public void OnButtonClick()
    {
        StopBlinking();
        // Add your button click logic here
    }

    void OnDestroy()
    {
        StopBlinking();
    }
}