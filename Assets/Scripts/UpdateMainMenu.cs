using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

[System.Serializable]
public class UpdateMainMenu : MonoBehaviour
{
    [Header("UI References")]
    public Text CoinsText;
    public GameObject FreePlayModeButton;
    public Button LeaderBoardButton;
    public GameObject leaderBoardMenu;
    public Button CloseLeaderBoardButton;
    public ScrollRect scrollRect;

    [Header("Leaderboard Settings")]
    public GameObject userScoreCardPrefab;
    public Transform leaderboardContent; // The content area of your scroll view
    public GameObject loadingIndicator; // Optional loading spinner

    [Header("Animation Settings")]
    public float animationDuration = 0.5f;
    public float delayBetweenItems = 0.1f;
    public Ease animationEase = Ease.OutBack;

    [Header("Player Highlight Settings")]
    public Color highlightColor = Color.yellow;
    public float highlightDuration = 1.0f;
    public float pulseScale = 1.1f;
    public int maxRankForAutoScroll = 100; // Only scroll if player is in top 100

    [Header("Share Settings")]
    public Button shareButton;
    public GameObject shareLoadingIndicator; // Optional loading spinner for share
    public bool shareWithScreenshot = true; // Whether to include screenshot when sharing

    private bool isLeaderBoardMenuOpen = false;
    private List<GameObject> currentLeaderboardItems = new List<GameObject>();
    private GameObject playerLeaderboardCard; // Reference to player's card in leaderboard

    public TMP_InputField usernameInput;
    public Button saveUsernameButton;
    public Button closeUsernameButton;
    public GameObject usernameMenu;
    public Button profileButton;

    public GameObject playerScoreCard;

    void Start()
    {
        InitializeUI();
        SetupEventListeners();
        if (FirebaseManager.Instance != null && FirebaseManager.isReady)
        {
            SaveManager.InitializeUser();
        }
        else
        {
            // If Firebase isn't ready yet, wait for it
            StartCoroutine(WaitForFirebaseAndInitialize());
        }

        playerScoreCard.SetActive(false);
        usernameInput.text = SaveManager.GetPlayerName();
        profileButton.onClick.AddListener(OnProfileButtonClicked);
        saveUsernameButton.onClick.AddListener(OnSaveUsernameButtonClicked);
        closeUsernameButton.onClick.AddListener(OnCloseUsernameButtonClicked);

        // Setup share button
        if (shareButton != null)
        {
            shareButton.onClick.AddListener(OnShareButtonClicked);
        }

        // Hide share loading indicator initially
        if (shareLoadingIndicator != null)
            shareLoadingIndicator.SetActive(false);
    }

    private IEnumerator WaitForFirebaseAndInitialize()
    {
        yield return new WaitUntil(() => FirebaseManager.Instance != null && FirebaseManager.isReady);
        yield return new WaitForSeconds(0.5f); // Small delay to ensure Firebase is fully ready
        SaveManager.InitializeUser();
    }

    void InitializeUI()
    {
        scrollRect.horizontal = false;
        scrollRect.vertical = true;

        int coins = 0;
        if (int.TryParse(SaveManager.LoadCoins(), out coins))
        {
            // Parsing succeeded, use `value`
        }
        else
        {
            coins = 0; // or any default
        }

        if (CoinsText != null)
        {
            CoinsText.text = "" + coins;
        }

        if (SaveManager.LoadChapterCompleted())
        {
            FreePlayModeButton.SetActive(true);
        }
        else
        {
            FreePlayModeButton.SetActive(false);
        }

        // Hide loading indicator initially
        if (loadingIndicator != null)
            loadingIndicator.SetActive(false);
    }

    void SetupEventListeners()
    {
        LeaderBoardButton.onClick.AddListener(OnLeaderBoardButtonClicked);
        CloseLeaderBoardButton.onClick.AddListener(OnCloseLeaderBoardButtonClicked);
    }

    void OnLeaderBoardButtonClicked()
    {
        Debug.Log("LeaderBoardButton clicked");

        if (isLeaderBoardMenuOpen)
        {
            CloseLeaderboard();
            return;
        }

        OpenLeaderboard();
    }

    void OnCloseLeaderBoardButtonClicked()
    {
        CloseLeaderboard();
    }

    void OpenLeaderboard()
    {
        leaderBoardMenu.SetActive(true);
        FirebaseManager.LeaderboardButtonClicked();
        isLeaderBoardMenuOpen = true;

        if (loadingIndicator != null)
            loadingIndicator.SetActive(true);

        ClearLeaderboard();

        FirebaseManager.GetLeaderboardWithUserInfo((leaderboardEntries) =>
        {
            if (loadingIndicator != null)
                loadingIndicator.SetActive(false);

            DisplayLeaderboard(leaderboardEntries);
            FirebaseManager.LeaderboardLoaded();

            // Find the current player's entry and update player score card
            string currentUserId = SaveManager.GetUserID();
            LeaderboardEntry playerEntry = leaderboardEntries.Find(entry => entry.userId == currentUserId);
            if (playerEntry != null)
            {
                UpdatePlayerScoreCard(playerEntry);

                // Check if player is in top 100 and scroll to their position
                if (playerEntry.rank <= maxRankForAutoScroll)
                {
                    StartCoroutine(ScrollToPlayerAndHighlight(playerEntry.rank));
                }
            }
            else
            {
                Debug.LogWarning("Player's own leaderboard entry not found.");
                //make entry from local data
                LeaderboardEntry playerEntry_local = new LeaderboardEntry
                {
                    userId = SaveManager.GetUserID(),
                    playerName = SaveManager.GetPlayerName(),
                    coins = int.Parse(SaveManager.LoadCoins()),
                    rank = 0,
                    country = SystemInfo.deviceModel
                };
                UpdatePlayerScoreCard(playerEntry_local);
            }
        });
    }

    void UpdatePlayerScoreCard(LeaderboardEntry entry)
    {
        if (playerScoreCard == null) return;
        playerScoreCard.SetActive(true);
        TextMeshProUGUI rankTMP = playerScoreCard.transform.Find("Container").transform.Find("Rank")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI coinsTMP = playerScoreCard.transform.Find("Container").transform.Find("Coins")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI playerNameTMP = playerScoreCard.transform.Find("Container").transform.Find("UserName")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI countryTMP = playerScoreCard.transform.Find("Container").transform.Find("Country")?.GetComponent<TextMeshProUGUI>();

        if (rankTMP != null) rankTMP.text = "#" + entry.rank;
        if (coinsTMP != null) coinsTMP.text = entry.coins.ToString();
        if (playerNameTMP != null) playerNameTMP.text = entry.playerName;
        if (countryTMP != null) countryTMP.text = entry.country;
    }

    IEnumerator ScrollToPlayerAndHighlight(int playerRank)
    {
        // Wait for all items to be created and animated in
        yield return new WaitForSeconds((currentLeaderboardItems.Count * delayBetweenItems) + animationDuration + 0.5f);

        // Calculate scroll position based on player rank
        int playerIndex = playerRank - 1; // Convert rank to index (0-based)

        if (playerIndex >= 0 && playerIndex < currentLeaderboardItems.Count)
        {
            // Get the player's leaderboard card
            playerLeaderboardCard = currentLeaderboardItems[playerIndex];

            // Calculate normalized scroll position
            float totalHeight = leaderboardContent.GetComponent<RectTransform>().sizeDelta.y;
            float viewportHeight = scrollRect.viewport.rect.height;

            if (totalHeight > viewportHeight)
            {
                // Calculate the target scroll position
                RectTransform playerCardRect = playerLeaderboardCard.GetComponent<RectTransform>();
                float playerCardPosition = -playerCardRect.anchoredPosition.y + playerCardRect.sizeDelta.y / 2;
                float targetNormalizedPosition = Mathf.Clamp01(playerCardPosition / (totalHeight - viewportHeight));

                // Smooth scroll to player position
                float scrollDuration = 1.0f;
                float startPosition = scrollRect.verticalNormalizedPosition;
                float elapsed = 0;

                while (elapsed < scrollDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / scrollDuration;
                    scrollRect.verticalNormalizedPosition = Mathf.Lerp(startPosition, 1f - targetNormalizedPosition,
                        Mathf.SmoothStep(0, 1, t));
                    yield return null;
                }
            }

            // Highlight the player's card
            yield return new WaitForSeconds(0.5f);
            HighlightPlayerCard();
        }
    }

    void HighlightPlayerCard()
    {
        if (playerLeaderboardCard == null) return;

        // Get or add components for highlighting
        Image cardBackground = playerLeaderboardCard.GetComponent<Image>();
        if (cardBackground == null)
        {
            cardBackground = playerLeaderboardCard.AddComponent<Image>();
        }

        // Store original color
        Color originalColor = cardBackground.color;

        // Create highlight animation sequence
        Sequence highlightSequence = DOTween.Sequence();

        // Pulse scale animation
        highlightSequence.Join(playerLeaderboardCard.transform.DOScale(pulseScale, highlightDuration * 0.3f)
            .SetEase(Ease.OutBack)
            .SetLoops(2, LoopType.Yoyo));

        // Color highlight animation
        highlightSequence.Join(cardBackground.DOColor(highlightColor, highlightDuration * 0.2f)
            .SetEase(Ease.OutQuart)
            .SetLoops(4, LoopType.Yoyo));

        // Glow effect using outline (if available)
        Outline outline = playerLeaderboardCard.GetComponent<Outline>();
        if (outline == null)
        {
            outline = playerLeaderboardCard.AddComponent<Outline>();
        }

        outline.effectColor = highlightColor;
        outline.effectDistance = Vector2.one * 3f;
        outline.enabled = true;

        // Animate outline
        highlightSequence.Join(DOTween.ToAlpha(() => outline.effectColor,
            x => outline.effectColor = x, 0f, highlightDuration)
            .From(1f)
            .SetLoops(3, LoopType.Yoyo));

        // Cleanup after animation
        highlightSequence.OnComplete(() =>
        {
            cardBackground.color = originalColor;
            if (outline != null)
            {
                outline.enabled = false;
            }
        });

        // Add particle effect (optional - requires a particle system prefab)
        // StartCoroutine(PlayHighlightParticles());
    }

    // Optional: Add particle effect for extra visual flair
    IEnumerator PlayHighlightParticles()
    {
        // This would require a particle system prefab
        // You can create a simple star burst or confetti effect
        // GameObject particles = Instantiate(highlightParticlesPrefab, playerLeaderboardCard.transform);
        // yield return new WaitForSeconds(2f);
        // Destroy(particles);
        yield return null;
    }

    void CloseLeaderboard()
    {
        // Reset player card reference
        playerLeaderboardCard = null;

        // Animate out the current items before closing
        AnimateLeaderboardOut(() =>
        {
            leaderBoardMenu.SetActive(false);
            isLeaderBoardMenuOpen = false;
            ClearLeaderboard();
        });
    }

    void DisplayLeaderboard(List<LeaderboardEntry> leaderboardEntries)
    {
        try
        {
            if (leaderboardEntries == null || leaderboardEntries.Count == 0)
            {
                Debug.LogWarning("No leaderboard data received");
                return;
            }

            // Create and animate the leaderboard items
            StartCoroutine(CreateLeaderboardItems(leaderboardEntries));
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error displaying leaderboard: " + e.Message);
        }
    }

    IEnumerator CreateLeaderboardItems(List<LeaderboardEntry> leaderboardEntries)
    {
        for (int i = 0; i < leaderboardEntries.Count; i++)
        {
            // Create the user score card
            GameObject scoreCard = Instantiate(userScoreCardPrefab, leaderboardContent);
            currentLeaderboardItems.Add(scoreCard);

            // Set up the score card data
            SetupScoreCard(scoreCard.transform.Find("Container").gameObject, leaderboardEntries[i]);

            // Animate the item in
            AnimateItemIn(scoreCard, i);

            // Small delay between creating items
            yield return new WaitForSeconds(delayBetweenItems);
        }
    }

    void SetupScoreCard(GameObject scoreCard, LeaderboardEntry entry)
    {
        TextMeshProUGUI rankTMP = scoreCard.transform.Find("Rank")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI playerNameTMP = scoreCard.transform.Find("UserName")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI countryTMP = scoreCard.transform.Find("Country")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI coinsTMP = scoreCard.transform.Find("Coins")?.GetComponent<TextMeshProUGUI>();

        if (rankTMP != null) rankTMP.text = "#" + entry.rank.ToString();
        if (playerNameTMP != null) playerNameTMP.text = entry.playerName;
        if (countryTMP != null) countryTMP.text = entry.country;
        if (coinsTMP != null) coinsTMP.text = entry.coins.ToString();

        // Check if this is the current player's entry
        string currentUserId = SaveManager.GetUserID();
        if (entry.userId == currentUserId)
        {
            // Mark this as the player's card for later reference
            scoreCard.name = "PlayerCard_" + entry.rank;
        }
    }

    void AnimateItemIn(GameObject item, int index)
    {
        // Set initial state
        item.transform.localScale = Vector3.zero;

        // Get or add CanvasGroup for fade animation
        CanvasGroup canvasGroup = item.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = item.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;

        // Create animation sequence
        Sequence animSequence = DOTween.Sequence();

        // Scale animation
        animSequence.Join(item.transform.DOScale(Vector3.one, animationDuration)
            .SetEase(animationEase));

        // Fade animation
        animSequence.Join(canvasGroup.DOFade(1f, animationDuration)
            .SetEase(Ease.OutQuart));

        // Set delay based on index
        animSequence.SetDelay(index * delayBetweenItems);
    }

    void AnimateLeaderboardOut(System.Action onComplete = null)
    {
        if (currentLeaderboardItems.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        int animationsRemaining = currentLeaderboardItems.Count;

        for (int i = 0; i < currentLeaderboardItems.Count; i++)
        {
            if (currentLeaderboardItems[i] == null) continue;

            GameObject item = currentLeaderboardItems[i];
            float delay = i * (delayBetweenItems * 0.5f); // Faster out animation

            Sequence animSequence = DOTween.Sequence();

            // Scale out
            animSequence.Join(item.transform.DOScale(Vector3.zero, animationDuration * 0.7f)
                .SetEase(Ease.InBack));

            // Fade out
            CanvasGroup canvasGroup = item.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                animSequence.Join(canvasGroup.DOFade(0f, animationDuration * 0.7f)
                    .SetEase(Ease.InQuart));
            }

            animSequence.SetDelay(delay);
            animSequence.OnComplete(() =>
            {
                animationsRemaining--;
                if (animationsRemaining <= 0)
                    onComplete?.Invoke();
            });
        }
    }

    void ClearLeaderboard()
    {
        // Destroy all current leaderboard items
        foreach (GameObject item in currentLeaderboardItems)
        {
            if (item != null)
                DestroyImmediate(item);
        }
        currentLeaderboardItems.Clear();
        playerLeaderboardCard = null;
    }

    void OnProfileButtonClicked()
    {
        usernameMenu.SetActive(true);
    }

    void OnSaveUsernameButtonClicked()
    {
        string username = usernameInput.text;
        FirebaseManager.SetUserInfo(SaveManager.GetUserID(), username, SystemInfo.deviceModel);
        usernameMenu.SetActive(false);
        FirebaseManager.UsernameUpdated();
    }

    void OnCloseUsernameButtonClicked()
    {
        usernameMenu.SetActive(false);
    }

    void OnShareButtonClicked()
    {
        if (shareWithScreenshot)
        {
            ShareWithScreenshot();
            FirebaseManager.ShareButtonEvent(true);
        }
        else
        {
            ShareWithoutScreenshot();
            FirebaseManager.ShareButtonEvent(false);
        }
    }

    void ShareWithScreenshot()
    {
        // Show loading indicator
        if (shareLoadingIndicator != null)
            shareLoadingIndicator.SetActive(true);

        // Disable share button temporarily
        if (shareButton != null)
            shareButton.interactable = false;

        StartCoroutine(AndroidHelper.CaptureScreenshot((screenshot) =>
        {
            // Hide loading indicator
            if (shareLoadingIndicator != null)
                shareLoadingIndicator.SetActive(false);

            // Re-enable share button
            if (shareButton != null)
                shareButton.interactable = true;

            if (screenshot != null)
            {
                AndroidHelper.AndroidNativeShare(screenshot);
                // Clean up the screenshot texture
                Destroy(screenshot);
            }
            else
            {
                AndroidHelper.ShowToast("Failed to capture screenshot");
                // Fallback to sharing without screenshot
                ShareWithoutScreenshot();
            }
        }));
    }

    void ShareWithoutScreenshot()
    {
        // Create a simple share without screenshot
        try
        {
            // Get player stats for sharing
            string playerName = SaveManager.GetPlayerName();
            int coins = 0;
            int.TryParse(SaveManager.LoadCoins(), out coins);

            // Create custom share message
            string shareMessage = $"🏏 I'm playing Journey of a Cricketer! 🏏\n\n" +
                                $"Player: {playerName}\n" +
                                $"Coins Earned: {coins}\n\n" +
                                $"Join me on this amazing cricket journey!\n\n" +
                                $"Download now: https://play.google.com/store/apps/details?id=com.AlphaCodeLabs.games.android.JOAC";

            // Use NativeShare for text-only sharing
            NativeShare nativeShare = new NativeShare();
            nativeShare.Clear();
            nativeShare.SetSubject("Journey of a Cricketer - My Progress!");
            nativeShare.SetTitle("Journey of a Cricketer - My Progress!");
            nativeShare.SetText(shareMessage);
            nativeShare.SetUrl("https://play.google.com/store/apps/details?id=com.AlphaCodeLabs.games.android.JOAC");
            nativeShare.Share();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Share failed: " + e.Message);
            AndroidHelper.ShowToast("Share failed. Please try again.");
        }
    }

    // Alternative method to share leaderboard position if player is in top ranks
    public void ShareLeaderboardPosition()
    {
        string currentUserId = SaveManager.GetUserID();

        FirebaseManager.GetLeaderboardWithUserInfo((leaderboardEntries) =>
        {
            LeaderboardEntry playerEntry = leaderboardEntries.Find(entry => entry.userId == currentUserId);

            if (playerEntry != null && playerEntry.rank <= 100) // Only share if in top 100
            {
                string shareMessage = $"🏆 AMAZING NEWS! 🏆\n\n" +
                                    $"I'm ranked #{playerEntry.rank} in Journey of a Cricketer!\n\n" +
                                    $"Player: {playerEntry.playerName}\n" +
                                    $"Country: {playerEntry.country}\n" +
                                    $"Coins: {playerEntry.coins}\n\n" +
                                    $"Can you beat my score? Download and challenge me!\n\n" +
                                    $"https://play.google.com/store/apps/details?id=com.AlphaCodeLabs.games.android.JOAC";

                NativeShare nativeShare = new NativeShare();
                nativeShare.Clear();
                nativeShare.SetSubject($"I'm ranked #{playerEntry.rank} in Journey of a Cricketer!");
                nativeShare.SetTitle($"I'm ranked #{playerEntry.rank} in Journey of a Cricketer!");
                nativeShare.SetText(shareMessage);
                nativeShare.SetUrl("https://play.google.com/store/apps/details?id=com.AlphaCodeLabs.games.android.JOAC");
                nativeShare.Share();
            }
            else
            {
                // Share general progress instead
                ShareWithoutScreenshot();
            }
        });
    }

    void OnDestroy()
    {
        // Clean up DOTween animations
        DOTween.KillAll();
    }
}