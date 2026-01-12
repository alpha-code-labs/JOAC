using UnityEngine;
using Firebase;
using Firebase.Extensions;
using Firebase.Messaging;
using UnityEngine.Android;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using Firebase.Analytics;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine.Networking;
using System.Text;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }
    public static bool isReady = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    FirebaseApp app;
    private string projectId = "joac-bceb5"; // Replace with your Firebase Project ID
    private string firestoreBaseUrl = $"https://firestore.googleapis.com/v1/projects/joac-bceb5/databases/(default)/documents";

    // Offline queue for failed requests
    private Queue<FirestoreOperation> offlineQueue = new Queue<FirestoreOperation>();
    private bool isOnline = true;

    private async void Start()
    {
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();

        if (dependencyStatus == DependencyStatus.Available)
        {
            app = FirebaseApp.DefaultInstance;

            // Register messaging callbacks
            FirebaseMessaging.TokenReceived += OnTokenReceived;
            FirebaseMessaging.MessageReceived += OnMessageReceived;
            isReady = true;

            RequestNotificationPermission();

            FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);

#if UNITY_EDITOR
            Debug.Log("Running in Unity Editor - Some Firebase features may be limited");
#elif DEVELOPMENT_BUILD
            Debug.Log("Development build detected");
#endif
#if DEVELOPMENT_BUILD
            Debug.Log("Firebase Initialized with Firestore REST API");
#endif
            // Start processing offline queue
            StartCoroutine(ProcessOfflineQueue());
        }
        else
        {
            AndroidHelper.ShowToast("Firebase Error: " + dependencyStatus.ToString());
        }
    }

    void OnTokenReceived(object sender, TokenReceivedEventArgs token)
    {
        Debug.Log("Received Registration Token: " + token.Token);
    }

    void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        Debug.Log("Received a message from: " + e.Message.From);
        if (e.Message.Notification != null)
        {
            Debug.Log("Title: " + e.Message.Notification.Title);
            Debug.Log("Body: " + e.Message.Notification.Body);
        }
    }

    public static void RequestNotificationPermission()
    {
        if (!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
        {
            Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
        }
        ValidateAndRefreshToken();
    }

    public static void ValidateAndRefreshToken()
    {
        FirebaseMessaging.GetTokenAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
            {
                string token = task.Result;
            }
            else
            {
                FirestoreDBError("Firebase error: " + task.Exception);
            }
        });
    }

    // Save user coins to leaderboard collection
    public static void SaveCoins(string userID, int coins)
    {
        Instance.StartCoroutine(Instance.SaveCoinsCoroutine(userID, coins));
    }

    private IEnumerator SaveCoinsCoroutine(string userID, int coins)
    {
        var leaderboardData = new FirestoreLeaderboardData
        {
            fields = new FirestoreLeaderboardFields
            {
                coins = new FirestoreDoubleValue { doubleValue = coins },
                lastUpdated = new FirestoreTimestampValue { timestampValue = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ") }
            }
        };

        yield return StartCoroutine(WriteDocument($"leaderboard/{userID}", JsonUtility.ToJson(leaderboardData)));
    }

    // Create complete user with all fields
    public static void CreateCompleteUser(string userID, string userName, string deviceInfo,
        string sceneName, bool gameCenterIntroduced = false, bool triviaIntroduced = false, bool chapterCompleted = false)
    {
        Instance.StartCoroutine(Instance.CreateCompleteUserCoroutine(userID, userName, deviceInfo,
            sceneName, gameCenterIntroduced, triviaIntroduced, chapterCompleted));
    }

    private IEnumerator CreateCompleteUserCoroutine(string userID, string userName, string deviceInfo,
        string sceneName, bool gameCenterIntroduced, bool triviaIntroduced, bool chapterCompleted)
    {
        // Validate mandatory fields
        if (!ValidateUserData(userID, userName, deviceInfo, Application.platform.ToString(), sceneName))
        {
            yield break;
        }

        string country = null;

        // Get country (optional)
#if UNITY_EDITOR
        country = "Editor";
#elif UNITY_ANDROID
        try 
        {
            country = AndroidHelper.GetCountryName();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Could not get country name: " + e.Message);
        }
#endif

        var userData = new FirestoreUserData
        {
            fields = new FirestoreUserFields
            {
                userName = new FirestoreStringValue { stringValue = userName },
                deviceInfo = new FirestoreStringValue { stringValue = deviceInfo },
                platform = new FirestoreStringValue { stringValue = Application.platform.ToString() },
                currentScene = new FirestoreStringValue { stringValue = sceneName },
                gameCenterIntroduced = new FirestoreBoolValue { booleanValue = gameCenterIntroduced },
                triviaSceneDialoguesIntroduced = new FirestoreBoolValue { booleanValue = triviaIntroduced },
                chapterCompleted = new FirestoreBoolValue { booleanValue = chapterCompleted },
                lastUpdated = new FirestoreTimestampValue { timestampValue = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ") }
            }
        };

        // Add optional country field
        if (!string.IsNullOrEmpty(country))
        {
            userData.fields.country = new FirestoreStringValue { stringValue = country };
        }

        // Set Firebase Analytics properties
        FirebaseAnalytics.SetUserProperty("user_id", userID);
        FirebaseAnalytics.SetUserProperty("user_name", userName);
        FirebaseAnalytics.SetUserProperty("device_info", deviceInfo);

        yield return StartCoroutine(WriteDocument($"users/{userID}", JsonUtility.ToJson(userData)));

        Debug.Log($"Complete user data stored for: {userID}");
    }

    // Set user information in users collection (legacy method for backward compatibility)
    public static void SetUserInfo(string userID, string userName, string deviceInfo)
    {
        // Get current save data to populate other fields
        SaveData localData = SaveManager.GetCurrentSaveData();

        CreateCompleteUser(userID, userName, deviceInfo,
            localData?.sceneName ?? "GamePlay_1",
            localData?.gameCenterIntroduced ?? false,
            localData?.triviaSceneDialoguesIntroduced ?? false,
            localData?.chapterCompleted ?? false);
    }

    // Update specific user scene
    public static void UpdateUserScene(string userID, string sceneName)
    {
        Instance.StartCoroutine(Instance.UpdateUserSceneCoroutine(userID, sceneName));
    }

    private IEnumerator UpdateUserSceneCoroutine(string userID, string sceneName)
    {
        var sceneData = new
        {
            fields = new
            {
                currentScene = new { stringValue = sceneName },
                lastUpdated = new { timestampValue = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ") }
            }
        };

        yield return StartCoroutine(UpdateDocument($"users/{userID}", JsonUtility.ToJson(sceneData)));
    }

    // Update game center status
    public static void UpdateUserGameCenterStatus(string userID, bool introduced)
    {
        Instance.StartCoroutine(Instance.UpdateUserGameCenterStatusCoroutine(userID, introduced));
    }

    private IEnumerator UpdateUserGameCenterStatusCoroutine(string userID, bool introduced)
    {
        var statusData = new
        {
            fields = new
            {
                gameCenterIntroduced = new { booleanValue = introduced },
                lastUpdated = new { timestampValue = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ") }
            }
        };

        yield return StartCoroutine(UpdateDocument($"users/{userID}", JsonUtility.ToJson(statusData)));
    }

    // Update trivia status
    public static void UpdateUserTriviaStatus(string userID, bool introduced)
    {
        Instance.StartCoroutine(Instance.UpdateUserTriviaStatusCoroutine(userID, introduced));
    }

    private IEnumerator UpdateUserTriviaStatusCoroutine(string userID, bool introduced)
    {
        var statusData = new
        {
            fields = new
            {
                triviaSceneDialoguesIntroduced = new { booleanValue = introduced },
                lastUpdated = new { timestampValue = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ") }
            }
        };

        yield return StartCoroutine(UpdateDocument($"users/{userID}", JsonUtility.ToJson(statusData)));
    }

    // Update chapter completion status
    public static void UpdateUserChapterStatus(string userID, bool completed)
    {
        Instance.StartCoroutine(Instance.UpdateUserChapterStatusCoroutine(userID, completed));
    }

    private IEnumerator UpdateUserChapterStatusCoroutine(string userID, bool completed)
    {
        var statusData = new
        {
            fields = new
            {
                chapterCompleted = new { booleanValue = completed },
                lastUpdated = new { timestampValue = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ") }
            }
        };

        yield return StartCoroutine(UpdateDocument($"users/{userID}", JsonUtility.ToJson(statusData)));
    }

    // Get leaderboard data (coins)
    public static void GetLeaderboard(Action<List<(string userID, float coins)>> onLeaderboardFetched)
    {
        Instance.StartCoroutine(Instance.GetLeaderboardCoroutine(onLeaderboardFetched));
    }

    private IEnumerator GetLeaderboardCoroutine(Action<List<(string userID, float coins)>> onLeaderboardFetched)
    {
        bool taskCompleted = false;
        List<(string userID, float coins)> results = new List<(string, float)>();

        StartCoroutine(TimeoutCoroutine());

        string collectionPath = "leaderboard";
        string url = $"{firestoreBaseUrl}/{collectionPath}?orderBy=coins desc";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            taskCompleted = true;

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var response = JsonUtility.FromJson<FirestoreCollectionResponse>(request.downloadHandler.text);

                    if (response?.documents != null)
                    {
                        foreach (var doc in response.documents)
                        {
                            string userID = ExtractDocumentId(doc.name);
                            if (doc.fields?.coins != null)
                            {
                                float coins = doc.fields.coins.doubleValue;
                                results.Add((userID, coins));
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    FirestoreDBError("Error parsing leaderboard: " + e.Message);
                }
            }
            else
            {
                if (request.responseCode == 404)
                {
                    Debug.Log("No leaderboard data found");
                }
                else
                {
                    FirestoreDBError($"Error fetching leaderboard: {request.error}");
                }
            }
        }

        onLeaderboardFetched?.Invoke(results);

        IEnumerator TimeoutCoroutine()
        {
            yield return new WaitForSecondsRealtime(10f);
            if (!taskCompleted)
            {
                AndroidHelper.ShowToast("Server timeout 10f");
                FirestoreDBError("Server timeout 10f");
                onLeaderboardFetched?.Invoke(new List<(string, float)>());
            }
        }
    }

    // Get user information
    public static void FetchUserInfo(string userID, Action<UserInfo> onUserFetched)
    {
        Instance.StartCoroutine(Instance.FetchUserInfoCoroutine(userID, onUserFetched));
    }

    private IEnumerator FetchUserInfoCoroutine(string userID, Action<UserInfo> onUserFetched)
    {
        string url = $"{firestoreBaseUrl}/users/{userID}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            UserInfo userInfo = new UserInfo
            {
                userID = userID,
                userName = "Invalid#" + userID,
                country = "Unknown",
                deviceInfo = "Unknown",
                platform = "Unknown",
                currentScene = "GamePlay_1",
                gameCenterIntroduced = false,
                triviaSceneDialoguesIntroduced = false,
                chapterCompleted = false
            };

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var response = JsonUtility.FromJson<FirestoreUserDocument>(request.downloadHandler.text);

                    if (response?.fields != null)
                    {
                        if (response.fields.userName != null)
                            userInfo.userName = response.fields.userName.stringValue;
                        if (response.fields.country != null)
                            userInfo.country = response.fields.country.stringValue;
                        if (response.fields.deviceInfo != null)
                            userInfo.deviceInfo = response.fields.deviceInfo.stringValue;
                        if (response.fields.platform != null)
                            userInfo.platform = response.fields.platform.stringValue;
                        if (response.fields.currentScene != null)
                            userInfo.currentScene = response.fields.currentScene.stringValue;
                        if (response.fields.gameCenterIntroduced != null)
                            userInfo.gameCenterIntroduced = response.fields.gameCenterIntroduced.booleanValue;
                        if (response.fields.triviaSceneDialoguesIntroduced != null)
                            userInfo.triviaSceneDialoguesIntroduced = response.fields.triviaSceneDialoguesIntroduced.booleanValue;
                        if (response.fields.chapterCompleted != null)
                            userInfo.chapterCompleted = response.fields.chapterCompleted.booleanValue;
                    }
                }
                catch (Exception e)
                {
                    FirestoreDBError("Error parsing user data: " + e.Message);
                }
            }
            else if (request.responseCode != 404)
            {
                FirestoreDBError($"Error fetching user: {request.error}");
            }

            onUserFetched?.Invoke(userInfo);
        }
    }

    // Get combined leaderboard with user info
    public static void GetLeaderboardWithUserInfo(Action<List<LeaderboardEntry>> onLeaderboardFetched)
    {
        Instance.StartCoroutine(Instance.GetLeaderboardWithUserInfoCoroutine(onLeaderboardFetched));
    }

    private IEnumerator GetLeaderboardWithUserInfoCoroutine(Action<List<LeaderboardEntry>> onLeaderboardFetched)
    {
        List<LeaderboardEntry> finalResults = new List<LeaderboardEntry>();
        bool leaderboardCompleted = false;

        // First get the leaderboard data
        GetLeaderboard((leaderboardData) =>
        {
            StartCoroutine(ProcessLeaderboardWithUsers(leaderboardData, (results) =>
            {
                finalResults = results;
                leaderboardCompleted = true;
            }));
        });

        // Wait for completion
        yield return new WaitUntil(() => leaderboardCompleted);

        onLeaderboardFetched?.Invoke(finalResults);
    }

    private IEnumerator ProcessLeaderboardWithUsers(List<(string userID, float coins)> leaderboardData, Action<List<LeaderboardEntry>> onComplete)
    {
        List<LeaderboardEntry> results = new List<LeaderboardEntry>();
        int completedRequests = 0;

        foreach (var entry in leaderboardData)
        {
            FetchUserInfo(entry.userID, (userInfo) =>
            {
                results.Add(new LeaderboardEntry
                {
                    userId = entry.userID,
                    playerName = userInfo.userName,
                    country = userInfo.country,
                    coins = int.Parse(entry.coins.ToString()),
                    rank = completedRequests + 1
                });
                completedRequests++;
            });
        }

        // Wait for all user info requests to complete
        yield return new WaitUntil(() => completedRequests == leaderboardData.Count);

        // Sort by coins (descending)
        results.Sort((a, b) => b.coins.CompareTo(a.coins));
        //update ranks
        for (int i = 0; i < results.Count; i++)
        {
            results[i].rank = i + 1;
        }

        onComplete?.Invoke(results);
    }

    private IEnumerator WriteDocument(string documentPath, string jsonData)
    {
        Debug.Log($"[WriteDocument] Starting write to: {documentPath}");
        Debug.Log($"[WriteDocument] JSON Data: {jsonData}");
        Debug.Log($"[WriteDocument] Online status: {isOnline}");

        if (!isOnline)
        {
            Debug.Log("[WriteDocument] Offline - queuing operation");
            QueueOfflineOperation(documentPath, jsonData);
            yield break;
        }

        string url = $"{firestoreBaseUrl}/{documentPath}";
        Debug.Log($"[WriteDocument] Full URL: {url}");

        using (UnityWebRequest request = new UnityWebRequest(url, "PATCH"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            Debug.Log("[WriteDocument] Sending request...");
            Debug.Log($"[WriteDocument] URL: {url}");
            yield return request.SendWebRequest();

            Debug.Log($"[WriteDocument] Request completed. Result: {request.result}");
            Debug.Log($"[WriteDocument] Response Code: {request.responseCode}");
            Debug.Log($"[WriteDocument] Response Text: {request.downloadHandler.text}");

            if (request.result != UnityWebRequest.Result.Success)
            {

                Debug.LogError($"[WriteDocument] Request failed: {request.error}");
                Debug.LogError($"[WriteDocument] Response: {request.downloadHandler.text}");

                if (IsNetworkError(request))
                {
                    isOnline = false;
                    QueueOfflineOperation(documentPath, jsonData);
                    Debug.Log("[WriteDocument] Network error detected, going offline");
                }
                else
                {
                    FirestoreDBError($"Error writing to Firestore: {request.error} - {request.downloadHandler.text}");
                }
            }
            else
            {
                isOnline = true;
                Debug.Log("[WriteDocument] Successfully wrote to Firestore!");
            }
        }
    }

    // Helper method for partial document updates (more efficient than full overwrites)
    private IEnumerator UpdateDocument(string documentPath, string jsonData)
    {
        Debug.Log($"[UpdateDocument] Starting update to: {documentPath}");
        Debug.Log($"[UpdateDocument] JSON Data: {jsonData}");
        Debug.Log($"[UpdateDocument] Online status: {isOnline}");

        if (!isOnline)
        {
            Debug.Log("[UpdateDocument] Device is offline, queuing operation");
            QueueOfflineOperation(documentPath, jsonData);
            yield break;
        }

        string url = $"{firestoreBaseUrl}/{documentPath}";

        using (UnityWebRequest request = new UnityWebRequest(url, "PATCH"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            Debug.Log($"[UpdateDocument] Sending request...");
            Debug.Log($"[UpdateDocument] URL: {url}");
            yield return request.SendWebRequest();

            Debug.Log($"[UpdateDocument] Request completed. Result: {request.result}");
            Debug.Log($"[UpdateDocument] Response Code: {request.responseCode}");
            Debug.Log($"[UpdateDocument] Response Text: {request.downloadHandler.text}");

            if (request.result != UnityWebRequest.Result.Success)
            {
                if (IsNetworkError(request))
                {
                    Debug.Log("[UpdateDocument] Network error detected, going offline");
                    isOnline = false;
                    QueueOfflineOperation(documentPath, jsonData);
                }
                else
                {
                    Debug.LogError($"[UpdateDocument] Request failed: {request.error}");
                    FirestoreDBError($"Error updating Firestore document: {request.error}");
                }
            }
            else
            {
                isOnline = true;
            }
        }
    }

    private void QueueOfflineOperation(string documentPath, string jsonData)
    {
        offlineQueue.Enqueue(new FirestoreOperation
        {
            documentPath = documentPath,
            jsonData = jsonData,
            timestamp = DateTime.UtcNow
        });
    }

    private IEnumerator ProcessOfflineQueue()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f); // Check every 5 seconds

            if (offlineQueue.Count > 0 && isOnline)
            {
                var operation = offlineQueue.Dequeue();
                yield return StartCoroutine(WriteDocument(operation.documentPath, operation.jsonData));
            }
        }
    }

    private bool IsNetworkError(UnityWebRequest request)
    {
        return request.result == UnityWebRequest.Result.ConnectionError ||
               request.result == UnityWebRequest.Result.DataProcessingError ||
               request.responseCode == 0;
    }

    private string ExtractDocumentId(string documentName)
    {
        if (string.IsNullOrEmpty(documentName)) return "";

        int lastSlashIndex = documentName.LastIndexOf('/');
        return lastSlashIndex >= 0 ? documentName.Substring(lastSlashIndex + 1) : documentName;
    }

    private bool ValidateUserData(string userID, string userName, string deviceInfo, string platform, string currentScene)
    {
        if (string.IsNullOrEmpty(userID))
        {
            FirestoreDBError("UserID is required");
            return false;
        }
        if (string.IsNullOrEmpty(userName))
        {
            FirestoreDBError("UserName is required");
            return false;
        }
        if (string.IsNullOrEmpty(deviceInfo))
        {
            FirestoreDBError("DeviceInfo is required");
            return false;
        }
        if (string.IsNullOrEmpty(platform))
        {
            FirestoreDBError("Platform is required");
            return false;
        }
        if (string.IsNullOrEmpty(currentScene))
        {
            FirestoreDBError("CurrentScene is required");
            return false;
        }
        return true;
    }

    #region Event Logging

    #region Cutscene Analytics Events

    /// <summary>
    /// Log when a cutscene starts playing
    /// </summary>
    /// <param name="cutsceneName">Name/ID of the cutscene</param>
    public static void LogCutsceneStarted(string cutsceneName)
    {
        Debug.Log($"[Analytics] Cutscene Started: {cutsceneName}");

        FirebaseAnalytics.LogEvent(
            "cutscene_started",
            new Parameter[] {
            new Parameter("cutscene_name", cutsceneName ?? "unknown"),
            }
        );
    }

    public static void IntroCutsceneEvent(string eventName)
    {
        Debug.Log($"[Analytics] Intro Cutscene Event: {eventName}");

        FirebaseAnalytics.LogEvent(
            "intro_cutscene_event",
            new Parameter[] {
            new Parameter("event_name", eventName ?? "unknown"),
            }
        );
    }

    public static void Transition_Event_2_CutsceneEvent(string eventName)
    {
        Debug.Log($"[Analytics] Transition Event 2 Cutscene Event: {eventName}");

        FirebaseAnalytics.LogEvent(
            "transition_event_2_cutscene_event",
            new Parameter[] {
            new Parameter("event_name", eventName ?? "unknown"),
            }
        );
    }

    public static void Transition_Event_3_CutsceneEvent(string eventName)
    {
        Debug.Log($"[Analytics] Transition Event 3 Cutscene Event: {eventName}");

        FirebaseAnalytics.LogEvent(
            "transition_event_3_cutscene_event",
            new Parameter[] {
            new Parameter("event_name", eventName ?? "unknown"),
            }
        );
    }

    public static void Transition_Event_4_CutsceneEvent(string eventName)
    {
        Debug.Log($"[Analytics] Transition Event 4 Cutscene Event: {eventName}");

        FirebaseAnalytics.LogEvent(
            "transition_event_4_cutscene_event",
            new Parameter[] {
            new Parameter("event_name", eventName ?? "unknown"),
            }
        );
    }

    public static void Transition_Event_5_CutsceneEvent(string eventName)
    {
        Debug.Log($"[Analytics] Transition Event 5 Cutscene Event: {eventName}");

        FirebaseAnalytics.LogEvent(
            "transition_event_5_cutscene_event",
            new Parameter[] {
            new Parameter("event_name", eventName ?? "unknown"),
            }
        );
    }

    public static void Transition_Event_6_CutsceneEvent(string eventName)
    {
        Debug.Log($"[Analytics] Transition Event 6 Cutscene Event: {eventName}");

        FirebaseAnalytics.LogEvent(
            "transition_event_6_cutscene_event",
            new Parameter[] {
            new Parameter("event_name", eventName ?? "unknown"),
            }
        );
    }

    public static void Transition_Event_7_CutsceneEvent(string eventName)
    {
        Debug.Log($"[Analytics] Transition Event 7 Cutscene Event: {eventName}");

        FirebaseAnalytics.LogEvent(
            "transition_event_7_cutscene_event",
            new Parameter[] {
            new Parameter("event_name", eventName ?? "unknown"),
            }
        );
    }

    public static void Transition_Event_8_CutsceneEvent(string eventName)
    {
        Debug.Log($"[Analytics] Transition Event 8 Cutscene Event: {eventName}");

        FirebaseAnalytics.LogEvent(
            "transition_event_8_cutscene_event",
            new Parameter[] {
            new Parameter("event_name", eventName ?? "unknown"),
            }
        );
    }


    public static void LogCutsceneEvent(string cutsceneName, string eventName)
    {
        Debug.Log($"[Analytics] Cutscene Event: {cutsceneName} - {eventName}");

        FirebaseAnalytics.LogEvent(
            "cutscene_event",
            new Parameter[] {
            new Parameter("cutscene_name", cutsceneName ?? "unknown"),
            new Parameter("event_name", eventName ?? "unknown"),
            }
        );
    }

    public static void ShareButtonEvent(bool shareWithScreenshot)
    {
        Debug.Log($"[Analytics] Share Button Event");
        FirebaseAnalytics.LogEvent("share_button_event",
        new Parameter[] {
            new Parameter("share_with_screenshot", shareWithScreenshot.ToString()),
        }
        );
    }



    /// <summary>
    /// Log when user clicks skip button during cutscene
    /// </summary>
    /// <param name="cutsceneName">Name/ID of the cutscene</param>
    public static void LogCutsceneSkipped(string cutsceneName)
    {
        Debug.Log($"[Analytics] Cutscene Skipped: {cutsceneName}");

        FirebaseAnalytics.LogEvent(
            "cutscene_skipped",
            new Parameter[] {
            new Parameter("cutscene_name", cutsceneName ?? "unknown"),
            }
        );
    }

    /// <summary>
    /// Log when a cutscene finishes playing (watched completely)
    /// </summary>
    /// <param name="cutsceneName">Name/ID of the cutscene</param>
    public static void LogCutsceneFinished(string cutsceneName)
    {
        Debug.Log($"[Analytics] Cutscene Finished: {cutsceneName}");

        FirebaseAnalytics.LogEvent(
            "cutscene_finished",
            new Parameter[] {
            new Parameter("cutscene_name", cutsceneName ?? "unknown"),
            }
        );
    }

    public static void LeaderboardButtonClicked()
    {
        Debug.Log($"[Analytics] Leaderboard Button Clicked");
        FirebaseAnalytics.LogEvent("leaderboard_button_clicked");
    }

    public static void LeaderboardLoaded()
    {
        Debug.Log($"[Analytics] Leaderboard Loaded");
        FirebaseAnalytics.LogEvent("leaderboard_loaded");
    }

    public static void UsernameUpdated()
    {
        Debug.Log($"[Analytics] Username Updated");
        FirebaseAnalytics.LogEvent("username_updated");
    }
    /// <summary>
    /// Log additional cutscene analytics with duration (optional)
    /// </summary>
    /// <param name="cutsceneName">Name/ID of the cutscene</param>
    /// <param name="watchDuration">How long user watched before skipping/finishing</param>
    /// <param name="totalDuration">Total duration of cutscene</param>
    public static void LogCutsceneAnalytics(string cutsceneName, float watchDuration, float totalDuration)
    {
        float watchPercentage = totalDuration > 0 ? (watchDuration / totalDuration) * 100 : 0;

        Debug.Log($"[Analytics] Cutscene Analytics: {cutsceneName} - {watchPercentage:F1}% watched");

        FirebaseAnalytics.LogEvent(
            "cutscene_analytics",
            new Parameter[] {
            new Parameter("cutscene_name", cutsceneName ?? "unknown"),
            new Parameter("watch_duration", watchDuration),
            new Parameter("total_duration", totalDuration),
            new Parameter("watch_percentage", watchPercentage)
            }
        );
    }

    #endregion
    public static void FirestoreDBError(string Exception)
    {
        Debug.Log(Exception);
        FirebaseAnalytics.LogEvent(
            "firestore_db_error",
            new Parameter[] {
            new Parameter("exception_text", Exception),
            }
        );
    }

    #endregion
}

// Data classes for Unity JsonUtility serialization
[System.Serializable]
public class FirestoreCollectionResponse
{
    public FirestoreDocument[] documents;
}

[System.Serializable]
public class FirestoreDocument
{
    public string name;
    public FirestoreLeaderboardFields fields; // Updated for leaderboard structure
}

// Leaderboard document structure
[System.Serializable]
public class FirestoreLeaderboardFields
{
    public FirestoreDoubleValue coins;
    public FirestoreTimestampValue lastUpdated;
}

[System.Serializable]
public class FirestoreLeaderboardData
{
    public FirestoreLeaderboardFields fields;
}

// Consolidated User document structure with all fields
[System.Serializable]
public class FirestoreUserFields
{
    public FirestoreStringValue userName;
    public FirestoreStringValue deviceInfo;
    public FirestoreStringValue country; // Optional
    public FirestoreStringValue platform;
    public FirestoreStringValue currentScene;
    public FirestoreBoolValue gameCenterIntroduced;
    public FirestoreBoolValue triviaSceneDialoguesIntroduced;
    public FirestoreBoolValue chapterCompleted;
    public FirestoreTimestampValue lastUpdated;
}

[System.Serializable]
public class FirestoreUserData
{
    public FirestoreUserFields fields;
}

[System.Serializable]
public class FirestoreUserDocument
{
    public string name;
    public FirestoreUserFields fields;
}

// Firestore value types
[System.Serializable]
public class FirestoreDoubleValue
{
    public float doubleValue;
}

[System.Serializable]
public class FirestoreStringValue
{
    public string stringValue;
}

[System.Serializable]
public class FirestoreBoolValue
{
    public bool booleanValue;
}

[System.Serializable]
public class FirestoreTimestampValue
{
    public string timestampValue;
}

[System.Serializable]
public class FirestoreOperation
{
    public string documentPath;
    public string jsonData;
    public DateTime timestamp;
}

// Enhanced helper classes for easier data handling
[System.Serializable]
public class UserInfo
{
    public string userID;
    public string userName;
    public string deviceInfo;
    public string country;
    public string platform;
    public string currentScene;
    public bool gameCenterIntroduced;
    public bool triviaSceneDialoguesIntroduced;
    public bool chapterCompleted;
}

[System.Serializable]
public class LeaderboardEntry
{
    public string playerName;
    public string country;
    public int coins;
    public int rank;
    public string userId;
}

public enum LevelEndResult
{
    back_to_leaderboard,
    quit_to_main_menu,
    retry,
    next_level
}

public enum ShareType
{
    share_button,
    share_level_end_screen_score,
    share_level_leaderboard_score,
    share_global_leaderboard_score,
    share_for_coins
}