using UnityEngine;
using System.Collections;

public class FirebaseTester : MonoBehaviour
{
    [Header("Testing Controls")]
    public bool testOnStart = true;
    public bool clearSaveDataFirst = false;

    private void Start()
    {
        if (testOnStart)
        {
            StartCoroutine(RunTests());
        }
    }

    private IEnumerator RunTests()
    {
        Debug.Log("=== FIREBASE TEST STARTING ===");

        // Clear save data if requested
        if (clearSaveDataFirst)
        {
            Debug.Log("Clearing existing save data...");
            ClearSaveData();
            yield return new WaitForSeconds(1f);
        }

        // Test 1: Check internet connectivity
        Debug.Log("Test 1: Testing internet connectivity...");
        yield return StartCoroutine(TestInternetConnection());

        // Test 2: Wait for Firebase
        Debug.Log("Test 2: Waiting for Firebase initialization...");
        yield return new WaitUntil(() => FirebaseManager.Instance != null);
        Debug.Log("✓ Firebase Manager is ready");
        yield return new WaitForSeconds(1f);

        // Test 3: Check Firebase base URL
        Debug.Log($"Test 3: Firebase Project ID check...");
        // This will be visible in the WriteDocument logs

        // Test 4: Initialize User
        Debug.Log("Test 4: Initializing user...");
        SaveManager.InitializeUser();
        yield return new WaitForSeconds(5f); // Give more time for Firebase operations

        // Test 5: Check local data
        Debug.Log("Test 5: Checking local save data...");
        string userID = SaveManager.GetUserID();
        string playerName = SaveManager.GetPlayerName();
        string coins = SaveManager.LoadCoins();

        Debug.Log($"✓ User ID: {userID}");
        Debug.Log($"✓ Player Name: {playerName}");
        Debug.Log($"✓ Coins: {coins}");

        if (string.IsNullOrEmpty(userID))
        {
            Debug.LogError("❌ User ID is empty - initialization failed!");
            yield break;
        }

        // Test 6: Manual Firebase write test
        Debug.Log("Test 6: Testing direct Firebase write...");
        FirebaseManager.SaveCoins(userID, 999);
        yield return new WaitForSeconds(3f);

        Debug.Log("=== FIREBASE TEST COMPLETED ===");
        Debug.Log("Check your Firebase Console to see if data appeared.");
        Debug.Log("Also check the Console logs above for any error messages.");
    }

    private IEnumerator TestInternetConnection()
    {
        using (UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequest.Get("https://www.google.com"))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.Log("✓ Internet connection is working");
            }
            else
            {
                Debug.LogError("❌ No internet connection: " + request.error);
            }
        }
    }

    private void ClearSaveData()
    {
        string savePath = System.IO.Path.Combine(Application.persistentDataPath, "saveData.json");
        if (System.IO.File.Exists(savePath))
        {
            System.IO.File.Delete(savePath);
            Debug.Log("Save data cleared");
        }
    }

    // Manual test buttons for Inspector
    [ContextMenu("Run Test")]
    public void RunTestManual()
    {
        StartCoroutine(RunTests());
    }

    [ContextMenu("Clear Save Data")]
    public void ClearSaveDataManual()
    {
        ClearSaveData();
    }

    [ContextMenu("Print Current User Info")]
    public void PrintUserInfo()
    {
        Debug.Log($"Current User ID: {SaveManager.GetUserID()}");
        Debug.Log($"Current Player Name: {SaveManager.GetPlayerName()}");
        Debug.Log($"Current Coins: {SaveManager.LoadCoins()}");
        Debug.Log($"Is First Time: {SaveManager.IsFirstTimeUser()}");
    }

    [ContextMenu("Test Direct Firebase Write")]
    public void TestDirectWrite()
    {
        if (FirebaseManager.Instance != null)
        {
            string testUserID = "test-user-" + System.DateTime.Now.Ticks;
            Debug.Log($"Testing direct write with user ID: {testUserID}");
            FirebaseManager.SetUserInfo(testUserID, "TestPlayer", "TestDevice");
            FirebaseManager.SaveCoins(testUserID, 123);
        }
        else
        {
            Debug.LogError("Firebase Manager not ready!");
        }
    }
}