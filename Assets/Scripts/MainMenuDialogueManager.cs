using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class SceneDialogueConfig
{
    public string sceneName;
    public DialogueData dialogueData;
    public bool showDialogue = true;
    [Tooltip("Show this dialogue only once per scene")]
    public bool showOnlyOnce = true;
}

public class MainMenuDialogueManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject dialogueBox;
    public TextMeshProUGUI dialogueText;
    public Image portraitImage;
    public AudioSource audioSource;
    public Button skipButton;
    public CanvasGroup backgroundPanelGroup;

    [Header("Animation Settings")]
    public RectTransform dialogueBoxTransform;
    public Vector2 hiddenPosition = new Vector2(-500, 0);
    public Vector2 visiblePosition = new Vector2(0, 0);
    public float slideSpeed = 5f;
    public float typeSpeed = 0.05f;
    public float autoAdvanceTime = 10f;

    [Header("Display Timing")]
    [Tooltip("Default minimum time if dialogue line doesn't specify displayTime")]
    public float defaultDisplayTime = 1f;

    private DialogueData currentDialogue;
    private int currentLineIndex = 0;
    private bool isDialogueActive = false;
    private Coroutine typewriterCoroutine;
    private float inactivityTimer;
    private float dialogueDisplayTimer; // Track how long current dialogue has been displayed
    private float currentLineDisplayTime; // The required display time for current line

    [Header("Progress-Based Dialogue Settings")]
    [SerializeField] private List<SceneDialogueConfig> sceneDialogues = new List<SceneDialogueConfig>();
    [SerializeField] private DialogueData fallbackDialogue; // Default dialogue if no scene-specific dialogue found
    public bool hindiDialogues = false;

    [Header("Timing Settings")]
    public float startDelay = 2f;

    // PlayerPrefs keys for tracking shown dialogues
    private const string DIALOGUE_SHOWN_PREFIX = "DialogueShown_";

    void Start()
    {
        dialogueBoxTransform.anchoredPosition = hiddenPosition;
        dialogueBox.SetActive(false);

        if (skipButton != null)
        {
            skipButton.onClick.AddListener(SkipDialogue);
        }

        // Start dialogue based on player progress
        Invoke("StartProgressBasedDialogue", startDelay);
    }

    void StartProgressBasedDialogue()
    {
        string currentSceneName = SaveManager.LoadSceneName();
        DialogueData dialogueToShow = GetDialogueForScene(currentSceneName);

        if (dialogueToShow != null && ShouldShowDialogue(currentSceneName))
        {
            StartDialogue(dialogueToShow);

            // Mark dialogue as shown for this scene if it's set to show only once
            SceneDialogueConfig config = GetSceneConfig(currentSceneName);
            if (config != null && config.showOnlyOnce)
            {
                MarkDialogueAsShown(currentSceneName);
            }
        }
        else
        {
            Debug.Log($"No dialogue to show for scene: {currentSceneName}");
        }
    }

    private DialogueData GetDialogueForScene(string sceneName)
    {
        // Find dialogue configuration for current scene
        foreach (SceneDialogueConfig config in sceneDialogues)
        {
            if (config.sceneName == sceneName && config.showDialogue)
            {
                return config.dialogueData;
            }
        }

        // Return fallback dialogue if no scene-specific dialogue found
        return fallbackDialogue;
    }

    private SceneDialogueConfig GetSceneConfig(string sceneName)
    {
        foreach (SceneDialogueConfig config in sceneDialogues)
        {
            if (config.sceneName == sceneName)
            {
                return config;
            }
        }
        return null;
    }

    private bool ShouldShowDialogue(string sceneName)
    {
        SceneDialogueConfig config = GetSceneConfig(sceneName);

        if (config == null)
        {
            // If no specific config, check if we have fallback dialogue and it hasn't been shown
            return fallbackDialogue != null && !HasDialogueBeenShown("fallback");
        }

        // Don't show if dialogue is disabled for this scene
        if (!config.showDialogue)
        {
            return false;
        }

        // If set to show only once, check if already shown
        if (config.showOnlyOnce && HasDialogueBeenShown(sceneName))
        {
            return false;
        }

        return true;
    }

    private bool HasDialogueBeenShown(string sceneName)
    {
        return PlayerPrefs.GetInt(DIALOGUE_SHOWN_PREFIX + sceneName, 0) == 1;
    }

    private void MarkDialogueAsShown(string sceneName)
    {
        PlayerPrefs.SetInt(DIALOGUE_SHOWN_PREFIX + sceneName, 1);
        PlayerPrefs.Save();
    }

    // Public method to manually trigger dialogue for a specific scene
    public void ShowDialogueForScene(string sceneName)
    {
        DialogueData dialogueToShow = GetDialogueForScene(sceneName);
        if (dialogueToShow != null)
        {
            StartDialogue(dialogueToShow);
        }
    }

    // Public method to reset dialogue flags (useful for testing)
    public void ResetDialogueFlags()
    {
        foreach (SceneDialogueConfig config in sceneDialogues)
        {
            PlayerPrefs.DeleteKey(DIALOGUE_SHOWN_PREFIX + config.sceneName);
        }
        PlayerPrefs.DeleteKey(DIALOGUE_SHOWN_PREFIX + "fallback");
        PlayerPrefs.Save();
        Debug.Log("All dialogue flags reset");
    }

    // Method to add new scene dialogue configuration at runtime
    public void AddSceneDialogue(string sceneName, DialogueData dialogue, bool showOnlyOnce = true)
    {
        SceneDialogueConfig newConfig = new SceneDialogueConfig
        {
            sceneName = sceneName,
            dialogueData = dialogue,
            showDialogue = true,
            showOnlyOnce = showOnlyOnce
        };
        sceneDialogues.Add(newConfig);
    }

    // Method to update existing scene dialogue configuration
    public void UpdateSceneDialogue(string sceneName, DialogueData newDialogue, bool? showDialogue = null, bool? showOnlyOnce = null)
    {
        SceneDialogueConfig config = GetSceneConfig(sceneName);
        if (config != null)
        {
            config.dialogueData = newDialogue;
            if (showDialogue.HasValue) config.showDialogue = showDialogue.Value;
            if (showOnlyOnce.HasValue) config.showOnlyOnce = showOnlyOnce.Value;
        }
    }

    public void StartDialogue(DialogueData dialogue)
    {
        if (isDialogueActive) return;

        currentDialogue = dialogue;
        currentLineIndex = 0;
        isDialogueActive = true;
        dialogueBox.SetActive(true);
        inactivityTimer = 0f;
        dialogueDisplayTimer = 0f; // Reset display timer
        StartCoroutine(SlideInDialogueBox());
    }

    public void NextDialogue()
    {
        if (!isDialogueActive) return;

        // Check if minimum display time has passed for current line
        if (dialogueDisplayTimer < currentLineDisplayTime)
        {
            Debug.Log($"Dialogue line needs to display for {currentLineDisplayTime} seconds. Current: {dialogueDisplayTimer:F1}s");
            return;
        }

        currentLineIndex++;
        inactivityTimer = 0f;

        if (currentLineIndex >= currentDialogue.dialogueLines.Length)
        {
            StartCoroutine(SlideOutDialogueBox());
        }
        else
        {
            DisplayLine();
        }
    }

    public void SkipDialogue()
    {
        Debug.Log("Skipping dialogues...");

        // Allow skip even before minimum time for better UX
        StartCoroutine(SlideOutDialogueBox());
    }

    private void DisplayLine()
    {
        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);

        DialogueLine line = currentDialogue.dialogueLines[currentLineIndex];
        portraitImage.sprite = line.portrait;

        // Get display time for this specific line
        currentLineDisplayTime = GetDisplayTimeForLine(line);
        Debug.Log($"Displaying line {currentLineIndex + 1}: Display time = {currentLineDisplayTime}s");

        if (line.voiceClip != null)
        {
            audioSource.Stop();
            audioSource.clip = line.voiceClip;
            audioSource.Play();
        }

        string actualText = line.DialogueText;
        if (hindiDialogues)
            actualText = UnicodeToKrutidev.UnicodeToKrutiDev(actualText);
        typewriterCoroutine = StartCoroutine(TypeText(actualText));
        inactivityTimer = 0f;
        dialogueDisplayTimer = 0f; // Reset timer when new line is displayed
    }

    private float GetDisplayTimeForLine(DialogueLine line)
    {
        // Try to get displayTime from the dialogue line
        // Assuming DialogueLine has a displayTime field
        // If your DialogueLine structure is different, adjust this accordingly

        try
        {
            // Using reflection to check if displayTime field exists
            var field = line.GetType().GetField("displayTime");
            if (field != null)
            {
                float displayTime = (float)field.GetValue(line);
                return displayTime > 0 ? displayTime : defaultDisplayTime;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Could not access displayTime field: {e.Message}");
        }

        // Fallback to default if field doesn't exist or is 0
        return defaultDisplayTime;
    }

    private IEnumerator TypeText(string text)
    {
        dialogueText.text = "";
        foreach (char letter in text)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typeSpeed);
        }
    }

    private IEnumerator SlideInDialogueBox()
    {
        // Ensure panel is visible before sliding in
        backgroundPanelGroup.alpha = 0f;
        float fadeDuration = 0.3f;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            backgroundPanelGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }
        backgroundPanelGroup.alpha = 1f;

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * slideSpeed;
            dialogueBoxTransform.anchoredPosition = Vector2.Lerp(hiddenPosition, visiblePosition, t);
            yield return null;
        }

        DisplayLine();
    }



    private IEnumerator SlideOutDialogueBox()
    {
        isDialogueActive = false;

        // Fade out the background panel first
        float fadeDuration = 0.3f;
        float elapsed = 0f;
        float startAlpha = backgroundPanelGroup.alpha;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            backgroundPanelGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeDuration);
            yield return null;
        }
        backgroundPanelGroup.alpha = 0f;

        // Then slide out the dialogue box
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * slideSpeed;
            dialogueBoxTransform.anchoredPosition = Vector2.Lerp(visiblePosition, hiddenPosition, t);
            yield return null;
        }

        dialogueBox.SetActive(false);
    }


    void Update()
    {
        if (!isDialogueActive) return;

        inactivityTimer += Time.deltaTime;
        dialogueDisplayTimer += Time.deltaTime; // Track how long dialogue has been displayed

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape))
        {
            inactivityTimer = 0f;
        }

        if (Input.GetKeyDown(KeyCode.Space)) NextDialogue();
        if (Input.GetKeyDown(KeyCode.Escape)) SkipDialogue();

        // Only auto-advance if minimum display time has passed for current line
        if (inactivityTimer >= autoAdvanceTime && dialogueDisplayTimer >= currentLineDisplayTime)
        {
            NextDialogue();
        }
    }

    // Debug methods for inspector
    [System.Serializable]
    public class DebugInfo
    {
        [Header("Current Progress Info")]
        public string currentScene;
        public bool wouldShowDialogue;
        public string dialogueName;
        public float currentDisplayTime;
        public float requiredDisplayTime;
        public bool canAdvance;

        public void UpdateInfo(MainMenuDialogueManager manager)
        {
            currentScene = SaveManager.LoadSceneName();
            DialogueData dialogue = manager.GetDialogueForScene(currentScene);
            wouldShowDialogue = manager.ShouldShowDialogue(currentScene);
            dialogueName = dialogue != null ? dialogue.name : "None";
            currentDisplayTime = manager.dialogueDisplayTimer;
            requiredDisplayTime = manager.currentLineDisplayTime;
            canAdvance = manager.dialogueDisplayTimer >= manager.currentLineDisplayTime;
        }
    }

    [Header("Debug Info")]
    public DebugInfo debugInfo = new DebugInfo();

    void OnValidate()
    {
        if (Application.isPlaying)
        {
            debugInfo.UpdateInfo(this);
        }
    }
}