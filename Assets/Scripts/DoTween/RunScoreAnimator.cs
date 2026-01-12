using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class RunScoreAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    public GameObject scoreTextPrefab;
    public Transform canvasTransform; // UI Canvas
    public Font textFont; // Optional: assign a custom font
    public float popupDuration = 1.5f;
    public float moveDistance = 300f; // Distance to move left
    public AnimationCurve scaleCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.2f, 1.2f), new Keyframe(0.5f, 1f), new Keyframe(1f, 1f));
    public AnimationCurve moveCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 0), new Keyframe(1f, 1));
    public AnimationCurve fadeCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(0.5f, 1), new Keyframe(1f, 0));

    [Header("Positioning")]
    public Vector2 startPosition = new Vector2(0, -100); // Relative to screen center

    [Header("Visual Effects")]
    public Color[] runColors = { Color.white, Color.green, Color.blue, Color.yellow, Color.red, Color.magenta };
    public float shadowOffset = 3f;

    private static RunScoreAnimator instance;
    public static RunScoreAnimator Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<RunScoreAnimator>();
            return instance;
        }
    }

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    public void ShowRunsScored(int runs)
    {
        Debug.Log($"[RunScoreAnimator] ShowRunsScored called with runs: {runs}");

        if (runs <= 0)
        {
            Debug.Log($"[RunScoreAnimator] Skipping animation - runs is {runs} (not positive)");
            return; // Don't show animation for 0 runs or wickets
        }

        if (canvasTransform == null)
        {
            Debug.LogError("[RunScoreAnimator] Canvas Transform is not assigned!");
            return;
        }

        Debug.Log("[RunScoreAnimator] Starting animation coroutine...");
        StartCoroutine(AnimateRunScore(runs));
    }

    private IEnumerator AnimateRunScore(int runs)
    {
        // Create the score text object
        GameObject scoreObj = Instantiate(scoreTextPrefab, canvasTransform);
        if (scoreObj == null) yield break;

        TextMeshProUGUI scoreText = scoreObj.GetComponent<TextMeshProUGUI>();
        if (scoreText == null)
        {
            Destroy(scoreObj);
            yield break;
        }

        // Set up the text
        scoreText.text = runs.ToString();
        scoreText.fontSize = 80f;
        scoreText.fontStyle = FontStyles.Bold;

        // Set color based on runs scored
        Color textColor = runs < runColors.Length ? runColors[runs] : runColors[runColors.Length - 1];
        scoreText.color = textColor;

        // Add outline/shadow effect for 3D look
        if (scoreText.fontMaterial != null)
        {
            scoreText.fontMaterial.SetFloat("_OutlineWidth", 0.2f);
            scoreText.fontMaterial.SetColor("_OutlineColor", Color.black);
        }

        // Position the text
        RectTransform rectTransform = scoreObj.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Destroy(scoreObj);
            yield break;
        }

        rectTransform.anchorMin = new Vector2(0.5f, 0.8f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.8f);
        rectTransform.anchoredPosition = startPosition;

        Vector2 endPosition = startPosition + Vector2.left * moveDistance;
        float elapsedTime = 0f;
        Vector2 originalPosition = rectTransform.anchoredPosition;
        Vector3 originalScale = Vector3.zero;

        while (elapsedTime < popupDuration)
        {
            // Check if objects still exist
            if (scoreObj == null || rectTransform == null || scoreText == null)
                yield break;

            float normalizedTime = elapsedTime / popupDuration;

            // Scale animation (popup effect)
            float scaleValue = scaleCurve.Evaluate(normalizedTime);
            rectTransform.localScale = originalScale + Vector3.one * scaleValue;

            // Movement animation (slide to left)
            float moveValue = moveCurve.Evaluate(normalizedTime);
            Vector2 currentPos = Vector2.Lerp(originalPosition, endPosition, moveValue);
            rectTransform.anchoredPosition = currentPos;

            // Fade animation
            float fadeValue = fadeCurve.Evaluate(normalizedTime);
            Color currentColor = textColor;
            currentColor.a = fadeValue;
            scoreText.color = currentColor;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Cleanup - check if still exists before destroying
        if (scoreObj != null)
            Destroy(scoreObj);
    }
}

// Extension script for UIManager
public static class UIManagerExtension
{
    public static void UpdateScoreWithAnimation(this UIManager uiManager, int targetRemaining, int ballsRemaining, int runsScored)
    {
        // Show the run animation
        if (RunScoreAnimator.Instance != null)
            RunScoreAnimator.Instance.ShowRunsScored(runsScored);

        // Update the actual score UI
        uiManager.UpdateScore(targetRemaining, ballsRemaining);
    }
}