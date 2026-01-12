using System.Collections;
using UnityEngine;

public class SetRefreshRate : MonoBehaviour
{
    public static void SetRefreshRateHighest()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

                // 👇 This ensures we're on the Android UI thread
                currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    try
                    {
                        using (AndroidJavaObject window = currentActivity.Call<AndroidJavaObject>("getWindow"))
                        using (AndroidJavaObject display = currentActivity.Call<AndroidJavaObject>("getDisplay"))
                        {
                            AndroidJavaObject[] displayModes = display.Call<AndroidJavaObject[]>("getSupportedModes");

                            int maxModeId = 0;
                            float maxRefreshRate = 60f;

                            foreach (AndroidJavaObject mode in displayModes)
                            {
                                float refreshRate = mode.Call<float>("getRefreshRate");
                                int modeId = mode.Call<int>("getModeId");

                                if (refreshRate > maxRefreshRate)
                                {
                                    maxRefreshRate = refreshRate;
                                    maxModeId = modeId;
                                }

                                mode.Dispose();
                            }

                            using (AndroidJavaObject layoutParams = window.Call<AndroidJavaObject>("getAttributes"))
                            {
                                layoutParams.Set("preferredDisplayModeId", maxModeId);
                                window.Call("setAttributes", layoutParams);
                            }

                            Debug.Log($"✅ Set display mode to max refresh rate: {maxRefreshRate} Hz (Mode ID: {maxModeId})");
                        }
                    }
                    catch (System.Exception inner)
                    {
                        Debug.LogWarning($"⚠ Exception on UI thread: {inner.Message}");
                    }
                }));
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"⚠ Failed to set highest refresh rate: {e.Message}");
        }
#else
        Debug.Log("SetRefreshRateHighest can only be called on an Android device.");
#endif
    }

    void Start()
    {
        SetRefreshRateHighest();
    }
}
