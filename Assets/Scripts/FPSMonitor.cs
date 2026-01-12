using UnityEngine;
using System.Collections.Generic;

public class FPSDisplay : MonoBehaviour
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    float deltaTime = 0.0f;

    // Collections to store FPS samples with timestamps
    private Queue<float> fpsHistory = new Queue<float>();
    private Queue<float> timeHistory = new Queue<float>();

    // Min/Max values for the last 30 seconds
    private float minFPS = float.MaxValue;
    private float maxFPS = float.MinValue;

    // How far back to track (7 seconds)
    private const float TRACKING_DURATION = 7.0f;

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

        // Calculate current FPS
        float currentFPS = 1.0f / deltaTime;
        float currentTime = Time.unscaledTime;

        // Add current sample to history
        fpsHistory.Enqueue(currentFPS);
        timeHistory.Enqueue(currentTime);

        // Remove samples older than TRACKING_DURATION seconds
        while (timeHistory.Count > 0 && currentTime - timeHistory.Peek() > TRACKING_DURATION)
        {
            fpsHistory.Dequeue();
            timeHistory.Dequeue();
        }

        // Calculate min/max from current history
        UpdateMinMaxFPS();
    }

    void UpdateMinMaxFPS()
    {
        if (fpsHistory.Count == 0)
        {
            minFPS = maxFPS = 1.0f / deltaTime;
            return;
        }

        minFPS = float.MaxValue;
        maxFPS = float.MinValue;

        foreach (float fps in fpsHistory)
        {
            if (fps < minFPS) minFPS = fps;
            if (fps > maxFPS) maxFPS = fps;
        }
    }

    void OnGUI()
    {
        int w = Screen.width, h = Screen.height;
        GUIStyle style = new GUIStyle();

        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / 50;
        style.normal.textColor = Color.white;

        float currentFPS = 1.0f / deltaTime;

        // Display current FPS
        Rect rect1 = new Rect(20, 100, w, h * 2 / 100);
        string currentText = string.Format("{0:0.} FPS", currentFPS);
        GUI.Label(rect1, currentText, style);

        // Display min FPS
        Rect rect2 = new Rect(20, 430, w, h * 2 / 100);
        string minText = string.Format("Min: {0:0.} FPS", minFPS);
        GUI.Label(rect2, minText, style);

        // Display max FPS
        Rect rect3 = new Rect(20, 560, w, h * 2 / 100);
        string maxText = string.Format("Max: {0:0.} FPS", maxFPS);
        GUI.Label(rect3, maxText, style);
    }
#endif
}