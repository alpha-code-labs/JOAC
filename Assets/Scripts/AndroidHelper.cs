using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;

public class AndroidHelper
{
    public static void ShowToast(string message)
    {
        Debug.Log("ShowToast: " + message);
        if (Application.platform == RuntimePlatform.Android)
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>(
                    "makeText", activity, message, toastClass.GetStatic<int>("LENGTH_SHORT"));
                toastObject.Call("show");
            }));
        }
    }

    public static string GetGAID()
    {
        try
        {
            using (AndroidJavaClass advertisingIdClient = new AndroidJavaClass("com.google.android.gms.ads.identifier.AdvertisingIdClient"))
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaObject adInfo = advertisingIdClient.CallStatic<AndroidJavaObject>("getAdvertisingIdInfo", context))
            {
                string adId = adInfo.Call<string>("getId");
                bool limitAdTracking = adInfo.Call<bool>("isLimitAdTrackingEnabled");

                //Debug.Log("GAID: " + adId);
                return adId;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to get GAID are you not android?: " + e.Message);
            return "Null GAID";
        }

    }
    public static string GetDeviceInfo()
    {
        return GetGAID() + "_" + SystemInfo.deviceModel + "_" + SystemInfo.operatingSystem + "_" + DateTime.UtcNow.Ticks;
    }
    public static string GetCountryName()
    {
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaObject telephonyManager = currentActivity.Call<AndroidJavaObject>("getSystemService", "phone"))
            {
                if (telephonyManager != null)
                {
                    string simCountry = telephonyManager.Call<string>("getSimCountryIso");

                    if (!string.IsNullOrEmpty(simCountry))
                    {
                        string countryCode = simCountry.ToUpper(); // convert to "IN", "US", etc.
                                                                   //ShowToast("SIM Country detected: " + countryCode);
                        string countryName = new RegionInfo(countryCode).EnglishName; // → "India"
                        return countryName;
                    }
                }

                ShowToast("SIM Country not available, fallback to Unknown");
                return "Unknown";
            }
        }
        catch /* (System.Exception e) */
        {
            //Debug.LogWarning("SIM Country detection failed: " + e.Message);
            return "Unknown";
        }
    }
    public static IEnumerator CaptureScreenshot(Action<Texture2D> callback)
    {
        yield return new WaitForEndOfFrame();
        Texture2D screenImage = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        screenImage.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenImage.Apply();
        callback?.Invoke(screenImage);
    }
    public static void AndroidNativeShare(Texture2D image)
    {
        NativeShare nativeShare = new NativeShare();
        nativeShare.Clear();
        //WhatsApp and Instagram Ignores the Subject and Title
        nativeShare.SetSubject("Experience the journey of a Cricketer? Download Journey of a Cricketer on Google Play Store!");
        nativeShare.SetTitle("Experience the journey of a Cricketer? Download Journey of a Cricketer on Google Play Store!");

        nativeShare.SetText("Experience the journey of a Cricketer? Download Journey of a Cricketer on Google Play Store!");
        //Adding A file Ignores URL,Text, Subject, Title on Instagram and Reddit
        nativeShare.AddFile(image);

        nativeShare.SetUrl("https://play.google.com/store/apps/details?id=com.AlphaCodeLabs.games.android.JOAC");
        nativeShare.Share();
    }
}