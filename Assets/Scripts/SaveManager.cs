using System.IO;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public string userID;
    public string playerName;
    public string deviceInfo;
    public string sceneName;
    public string country;
    public bool gameCenterIntroduced = false;
    public string coins;
    public bool triviaSceneDialoguesIntroduced = false;
    public bool chapterCompleted = false;
    public bool isFirstTime = true; // Track if this is a new user
    public bool firebaseInitialized = false; // Track if user data exists in Firebase
}

public class SaveManager : MonoBehaviour
{
    private static string SaveFilePath => Path.Combine(Application.persistentDataPath, "saveData.json");

    // Public method to access save data from other scripts (like FirebaseManager)
    public static SaveData GetCurrentSaveData()
    {
        return LoadAll();
    }

    private static SaveData LoadAll()
    {
        if (File.Exists(SaveFilePath))
        {
            try
            {
                string json = File.ReadAllText(SaveFilePath);
                return JsonUtility.FromJson<SaveData>(json);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error loading save data: " + e.Message);
                return new SaveData(); // Return empty if file is corrupted
            }
        }
        return new SaveData(); // return empty if no file yet
    }

    // Initialize user - call this when the game starts
    public static void InitializeUser()
    {
        Debug.Log("Initializing user");
        SaveData data = LoadAll();

        // If this is a completely new user (no save file exists)
        if (string.IsNullOrEmpty(data.userID))
        {
            CreateNewUser(data);
        }
        // If user exists locally but not synced with Firebase
        else if (!data.firebaseInitialized)
        {
            SyncExistingUserToFirebase(data);
        }
        // User already exists and is synced
        else
        {
            Debug.Log("User already initialized: " + data.userID);
            // Optionally verify Firebase connection and update if needed
            ValidateFirebaseSync(data);
        }
    }

    private static async void CreateNewUser(SaveData data)
    {
        // Generate new user ID
        data.userID = System.Guid.NewGuid().ToString();
        data.playerName = "Player" + Random.Range(1000, 9999);
        data.deviceInfo = SystemInfo.deviceModel;
        data.sceneName = "GamePlay_1"; // Default scene
        data.country = "Unknown";
        data.coins = "0";
        data.isFirstTime = true;
        data.firebaseInitialized = false;

        // Set platform-specific defaults
#if UNITY_EDITOR
        data.country = "Editor";
        data.deviceInfo = "Unity Editor";
#elif UNITY_ANDROID
        data.deviceInfo = SystemInfo.deviceModel;
#elif UNITY_IOS
        data.deviceInfo = SystemInfo.deviceModel;
#else
        data.deviceInfo = "Desktop";
        data.country = "Desktop";
#endif

        // Save locally first
        SaveDataToFile(data);

        // Try to get country from IP (async)
        data.country = await GetCountryFromIPAsync();
        SaveDataToFile(data);

        // Sync to Firebase
        SyncToFirebase(data);

        Debug.Log("New user created: " + data.userID);
    }

    private static void SyncExistingUserToFirebase(SaveData data)
    {
        // User exists locally but needs to be synced to Firebase
        SyncToFirebase(data);
        Debug.Log("Syncing existing user to Firebase: " + data.userID);
    }

    private static void ValidateFirebaseSync(SaveData data)
    {
        // Periodically validate that local data matches Firebase
        // This can help recover from sync issues
        if (FirebaseManager.isReady)
        {
            // Optionally fetch user data from Firebase and compare
            FirebaseManager.FetchUserInfo(data.userID, (userInfo) =>
            {
                if (userInfo.userName.StartsWith("Invalid#"))
                {
                    Debug.LogWarning("User not found in Firebase, re-syncing...");
                    data.firebaseInitialized = false;
                    SyncToFirebase(data);
                }
            });
        }
    }

    private static void SyncToFirebase(SaveData data)
    {
        if (!FirebaseManager.isReady)
        {
            Debug.LogWarning("Firebase not ready, will retry sync later");
            // You could implement a retry mechanism here
            return;
        }

        // Use the new comprehensive CreateCompleteUser method
        FirebaseManager.CreateCompleteUser(
            data.userID,
            data.playerName,
            data.deviceInfo,
            data.sceneName,
            data.gameCenterIntroduced,
            data.triviaSceneDialoguesIntroduced,
            data.chapterCompleted
        );

        // Save coins to leaderboard
        int coinAmount = 0;
        int.TryParse(data.coins, out coinAmount);
        FirebaseManager.SaveCoins(data.userID, coinAmount);

        // Mark as Firebase initialized
        data.firebaseInitialized = true;
        data.isFirstTime = false;
        SaveDataToFile(data);

        Debug.Log($"User synced to Firebase: {data.userID}");
    }

    private static void SaveDataToFile(SaveData data)
    {
        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SaveFilePath, json);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error saving data to file: " + e.Message);
        }
    }

    // Enhanced save methods that also update Firebase
    public static void SaveSceneName(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("Scene name is empty, not saving");
            return;
        }

        SaveData data = LoadAll();
        data.sceneName = sceneName;
        SaveDataToFile(data);

        // Update Firebase if initialized
        if (data.firebaseInitialized && !string.IsNullOrEmpty(data.userID) && FirebaseManager.isReady)
        {
            FirebaseManager.UpdateUserScene(data.userID, sceneName);
        }

        Debug.Log("Scene name saved: " + sceneName);
    }

    public static void SaveCoins(string coins)
    {
        SaveData data = LoadAll();
        int newCoins = 0;
        int.TryParse(coins, out newCoins);

        int total = newCoins;
        data.coins = total.ToString();
        SaveDataToFile(data);

        // Update Firebase if initialized
        if (data.firebaseInitialized && !string.IsNullOrEmpty(data.userID) && FirebaseManager.isReady)
        {
            FirebaseManager.SaveCoins(data.userID, total);
        }

        Debug.Log("Total coins saved: " + total);
    }

    public static void SaveCoinsAbsolute(int totalCoins)
    {
        SaveData data = LoadAll();
        data.coins = totalCoins.ToString();
        SaveDataToFile(data);

        // Update Firebase if initialized
        if (data.firebaseInitialized && !string.IsNullOrEmpty(data.userID) && FirebaseManager.isReady)
        {
            FirebaseManager.SaveCoins(data.userID, totalCoins);
        }

        Debug.Log("Absolute coins saved: " + totalCoins);
    }

    public static void SaveGameCenterIntroduced(bool introduced)
    {
        SaveData data = LoadAll();
        data.gameCenterIntroduced = introduced;
        SaveDataToFile(data);

        // Update Firebase if initialized
        if (data.firebaseInitialized && !string.IsNullOrEmpty(data.userID) && FirebaseManager.isReady)
        {
            FirebaseManager.UpdateUserGameCenterStatus(data.userID, introduced);
        }

        Debug.Log("Game center introduced saved: " + introduced);
    }

    public static void SaveTriviaSceneDialoguesIntroduced(bool introduced)
    {
        SaveData data = LoadAll();
        data.triviaSceneDialoguesIntroduced = introduced;
        SaveDataToFile(data);

        // Update Firebase if initialized
        if (data.firebaseInitialized && !string.IsNullOrEmpty(data.userID) && FirebaseManager.isReady)
        {
            FirebaseManager.UpdateUserTriviaStatus(data.userID, introduced);
        }

        Debug.Log("Trivia scene dialogues introduced saved: " + introduced);
    }

    public static void SaveChapterCompleted(bool completed)
    {
        SaveData data = LoadAll();
        data.chapterCompleted = completed;
        SaveDataToFile(data);

        // Update Firebase if initialized
        if (data.firebaseInitialized && !string.IsNullOrEmpty(data.userID) && FirebaseManager.isReady)
        {
            FirebaseManager.UpdateUserChapterStatus(data.userID, completed);
        }

        Debug.Log("Chapter completed saved: " + completed);
    }

    // Update player name (new method)
    public static void SavePlayerName(string playerName)
    {
        if (string.IsNullOrEmpty(playerName))
        {
            Debug.LogWarning("Player name is empty, not saving");
            return;
        }

        SaveData data = LoadAll();
        data.playerName = playerName;
        SaveDataToFile(data);

        // Update Firebase - requires full user update
        if (data.firebaseInitialized && !string.IsNullOrEmpty(data.userID) && FirebaseManager.isReady)
        {
            SyncToFirebase(data);
        }

        Debug.Log("Player name saved: " + playerName);
    }

    // Load methods remain the same but with better error handling
    public static string LoadCoins()
    {
        SaveData data = LoadAll();
        return data.coins ?? "0";
    }

    public static int LoadCoinsAsInt()
    {
        string coinsStr = LoadCoins();
        int coins = 0;
        int.TryParse(coinsStr, out coins);
        return coins;
    }

    public static string LoadSceneName()
    {
        SaveData data = LoadAll();
        return data.sceneName ?? "GamePlay_1";
    }

    public static bool LoadGameCenterIntroduced()
    {
        SaveData data = LoadAll();
        return data.gameCenterIntroduced;
    }

    public static bool LoadTriviaSceneDialoguesIntroduced()
    {
        SaveData data = LoadAll();
        return data.triviaSceneDialoguesIntroduced;
    }

    public static bool LoadChapterCompleted()
    {
        SaveData data = LoadAll();
        return data.chapterCompleted;
    }

    // Get current user ID
    public static string GetUserID()
    {
        SaveData data = LoadAll();
        return data.userID ?? "";
    }

    // Get current player name
    public static string GetPlayerName()
    {
        SaveData data = LoadAll();
        return data.playerName ?? "Player";
    }

    // Get current device info
    public static string GetDeviceInfo()
    {
        SaveData data = LoadAll();
        return data.deviceInfo ?? SystemInfo.deviceModel;
    }

    // Get current country
    public static string GetCountry()
    {
        SaveData data = LoadAll();
        return data.country ?? "Unknown";
    }

    // Check if user is new
    public static bool IsFirstTimeUser()
    {
        SaveData data = LoadAll();
        return data.isFirstTime;
    }

    // Check if Firebase is initialized
    public static bool IsFirebaseInitialized()
    {
        SaveData data = LoadAll();
        return data.firebaseInitialized;
    }

    // Force sync all data to Firebase (useful for recovery)
    public static void ForceSyncToFirebase()
    {
        SaveData data = LoadAll();
        if (!string.IsNullOrEmpty(data.userID))
        {
            data.firebaseInitialized = false; // Force re-sync
            SyncToFirebase(data);
        }
        else
        {
            Debug.LogWarning("No user ID found, cannot sync to Firebase");
        }
    }

    // Reset all save data (useful for testing)
    public static void ResetSaveData()
    {
        if (File.Exists(SaveFilePath))
        {
            File.Delete(SaveFilePath);
            Debug.Log("Save data reset");
        }
    }

    // Get all save data (for debugging)
    public static SaveData GetAllSaveData()
    {
        return LoadAll();
    }

    // Manual country update (if needed)
    public static void UpdateCountry(string country)
    {
        SaveData data = LoadAll();
        data.country = country;
        SaveDataToFile(data);

        // Re-sync to Firebase to update country
        if (data.firebaseInitialized && !string.IsNullOrEmpty(data.userID) && FirebaseManager.isReady)
        {
            SyncToFirebase(data);
        }

        Debug.Log("Country updated: " + country);
    }

    private static async System.Threading.Tasks.Task<string> GetCountryFromIPAsync()
    {
        try
        {
            UnityWebRequest request = UnityWebRequest.Get("http://ip-api.com/json/?fields=countryCode");

            var operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                await System.Threading.Tasks.Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<IPLocationResponse>(request.downloadHandler.text);
                request.Dispose();
                return response.countryCode ?? "Unknown";
            }
            else
            {
                Debug.LogError("Failed to get country from IP: " + request.error);
                request.Dispose();
                return "Unknown";
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error getting country: " + e.Message);
            return "Unknown";
        }
    }

    [System.Serializable]
    public class IPLocationResponse
    {
        public string countryCode;
    }
}