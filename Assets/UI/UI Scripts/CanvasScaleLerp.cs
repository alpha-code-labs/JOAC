using System.Collections;
using UnityEngine;

public class CanvasScaleLerp : MonoBehaviour
{
    public Transform canvasTransform; // Assign your Canvas Transform here
    public CanvasGroup canvasGroup; // For fade effect
    public float lerpSpeed = 2f;
    private Vector3 hiddenScale = Vector3.zero; // Scale at start
    private Vector3 visibleScale = Vector3.one; // Normal scale

    private bool isVisible = false; // Track state

    void Start()
    {
        if (canvasTransform == null)
            canvasTransform = transform; // Assign itself if empty

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>(); // Auto-assign

        canvasTransform.localScale = hiddenScale; // Start with scale 0
        canvasGroup.alpha = 0; // Start invisible
        StartCoroutine(ScaleInCanvas()); // Start scaling in
    }

    public void ShowCanvas()
    {
        StopAllCoroutines();
        StartCoroutine(ScaleInCanvas());
        isVisible = true;
    }

    public void HideCanvas()
    {
        StopAllCoroutines();
        StartCoroutine(ScaleOutCanvas());
        isVisible = false;
    }

    public void ToggleCanvas()
    {
        if (isVisible)
            HideCanvas();
        else
            ShowCanvas();
    }

    IEnumerator ScaleInCanvas()
    {
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * lerpSpeed;
            canvasTransform.localScale = Vector3.Lerp(hiddenScale, visibleScale, t);
            canvasGroup.alpha = Mathf.Lerp(0, 1, t);
            yield return null;
        }
        canvasTransform.localScale = visibleScale;
        canvasGroup.alpha = 1;
    }

    IEnumerator ScaleOutCanvas()
    {
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * lerpSpeed;
            canvasTransform.localScale = Vector3.Lerp(visibleScale, hiddenScale, t);
            canvasGroup.alpha = Mathf.Lerp(1, 0, t);
            yield return null;
        }
        canvasTransform.localScale = hiddenScale;
        canvasGroup.alpha = 0;
    }
}


