using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening; // Add DOTween namespace

public class DialogueManager1 : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject dialogueBox;
    public TextMeshProUGUI dialogueText;
    public Image portraitImage;
    public AudioSource audioSource;
    public Button skipButton;
    public Button nextButton;

    [Header("Animation Settings")]
    public RectTransform dialogueBoxTransform;
    public Vector2 hiddenPosition = new Vector2(-500, 0);
    public Vector2 visiblePosition = new Vector2(0, 0);
    public float slideSpeed = 5f;
    public float typeSpeed = 0.05f;
    public float autoAdvanceTime = 10f;

    [Header("DOTween Text Animation Settings")]
    public float textAnimationDuration = 2f; // Duration for full text to appear
    public Ease textEaseType = Ease.Linear; // Easing for text animation
    public bool useScrambleEffect = false; // Optional scramble effect
    public string scrambleChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    private DialogueData currentDialogue;
    private int currentLineIndex = 0;
    private bool isDialogueActive = false;
    private Tween currentTextTween; // Store current text tween
    private float inactivityTimer;

    [Header("Dialogue Settings")]
    public DialogueData defaultDialogue;
    public bool startDialogueOnSceneLoad = false;
    public bool showOnSubsequentLoads = false;
    public bool hindiDialogues = false;

    void Start()
    {
        dialogueBoxTransform.anchoredPosition = hiddenPosition;
        dialogueBox.SetActive(false);
        int controlCenterIntroduced = 0;
        if (PlayerPrefs.HasKey("controlCenterIntroduced"))
        {
            controlCenterIntroduced = PlayerPrefs.GetInt("controlCenterIntroduced");
        }

        //skip dialogue if control center has been introduced
        if (!showOnSubsequentLoads && controlCenterIntroduced == 1) return;

        if (startDialogueOnSceneLoad && defaultDialogue != null)
        {
            Invoke("StartSceneDialogue", 2f);
        }

        if (skipButton != null)
        {
            skipButton.onClick.AddListener(SkipDialogue);
        }

        if (nextButton != null)
        {
            nextButton.onClick.AddListener(LoadNextDialogue);
        }
    }

    void StartSceneDialogue()
    {
        StartDialogue(defaultDialogue);
    }

    public void StartDialogue(DialogueData dialogue)
    {
        if (isDialogueActive) return;

        currentDialogue = dialogue;
        currentLineIndex = 0;
        isDialogueActive = true;
        dialogueBox.SetActive(true);
        inactivityTimer = 0f;
        StartCoroutine(SlideInDialogueBox());
    }

    public void NextDialogue()
    {
        if (!isDialogueActive) return;

        // If text is still animating, complete it instantly
        if (currentTextTween != null && currentTextTween.IsActive())
        {
            currentTextTween.Complete();
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

        // Kill any active text animation
        if (currentTextTween != null && currentTextTween.IsActive())
        {
            currentTextTween.Kill();
        }

        StartCoroutine(SlideOutDialogueBox());
    }

    private void DisplayLine()
    {
        // Kill previous text animation if it exists
        if (currentTextTween != null && currentTextTween.IsActive())
        {
            currentTextTween.Kill();
        }

        DialogueLine line = currentDialogue.dialogueLines[currentLineIndex];
        //portraitImage.sprite = line.portrait;

        if (line.voiceClip != null)
        {
            audioSource.Stop();
            audioSource.clip = line.voiceClip;
            audioSource.Play();
        }

        string actualText = line.DialogueText;
        if (hindiDialogues)
        {
            actualText = UnicodeToKrutidev.UnicodeToKrutiDev(actualText);
        }

        AnimateTextWithDOTween(actualText);
        inactivityTimer = 0f;
    }

    private void AnimateTextWithDOTween(string targetText)
    {
        // Set the full text but hide all characters initially
        dialogueText.text = targetText;
        dialogueText.maxVisibleCharacters = 0;

        if (useScrambleEffect)
        {
            // For scramble effect, we'll use a custom approach
            AnimateTextWithScramble(targetText);
        }
        else
        {
            // Animate visible characters for typewriter effect
            currentTextTween = DOTween.To(
                () => dialogueText.maxVisibleCharacters,
                x => dialogueText.maxVisibleCharacters = x,
                targetText.Length,
                textAnimationDuration
            ).SetEase(textEaseType);
        }

        // Optional: Add callbacks
        currentTextTween.OnComplete(() =>
        {
            // Text animation completed
            Debug.Log("Text animation completed");
        });
    }

    // Custom scramble effect for TextMeshPro
    private void AnimateTextWithScramble(string targetText)
    {
        string scrambledText = "";
        float scrambleDuration = textAnimationDuration * 0.3f; // 30% of time for scrambling
        float revealDuration = textAnimationDuration * 0.7f;   // 70% of time for revealing

        // Create scrambled version
        for (int i = 0; i < targetText.Length; i++)
        {
            if (targetText[i] == ' ')
                scrambledText += ' ';
            else
                scrambledText += scrambleChars[Random.Range(0, scrambleChars.Length)];
        }

        // Set scrambled text initially
        dialogueText.text = scrambledText;
        dialogueText.maxVisibleCharacters = targetText.Length;

        // Create sequence for scramble effect
        Sequence scrambleSequence = DOTween.Sequence();

        // Phase 1: Scramble animation
        scrambleSequence.Append(
            DOTween.To(() => 0f, x =>
            {
                // Update some characters randomly during scramble
                if (Random.value < 0.3f) // 30% chance to update each frame
                {
                    char[] chars = dialogueText.text.ToCharArray();
                    for (int i = 0; i < chars.Length; i++)
                    {
                        if (chars[i] != ' ' && Random.value < 0.1f) // 10% chance per character
                        {
                            chars[i] = scrambleChars[Random.Range(0, scrambleChars.Length)];
                        }
                    }
                    dialogueText.text = new string(chars);
                }
            }, 1f, scrambleDuration)
        );

        // Phase 2: Reveal correct text character by character
        scrambleSequence.Append(
            DOTween.To(() => 0, revealedChars =>
            {
                char[] displayChars = scrambledText.ToCharArray();
                for (int i = 0; i < revealedChars && i < targetText.Length; i++)
                {
                    displayChars[i] = targetText[i];
                }
                dialogueText.text = new string(displayChars);
            }, targetText.Length, revealDuration).SetEase(textEaseType)
        );

        currentTextTween = scrambleSequence;
    }
    // Alternative method: Character-by-character with more control
    private void AnimateTextCharByChar(string targetText)
    {
        dialogueText.text = "";

        // Calculate duration per character
        float charDuration = textAnimationDuration / targetText.Length;

        // Create a sequence for more complex animations
        Sequence textSequence = DOTween.Sequence();

        for (int i = 0; i <= targetText.Length; i++)
        {
            int index = i; // Capture for closure
            textSequence.AppendCallback(() =>
            {
                if (index <= targetText.Length)
                {
                    dialogueText.text = targetText.Substring(0, index);

                    // Optional: Add character pop effect
                    if (index > 0 && index <= targetText.Length)
                    {
                        dialogueText.transform.DOPunchScale(Vector3.one * 0.02f, 0.1f, 1, 0.5f);
                    }
                }
            });

            if (i < targetText.Length)
            {
                textSequence.AppendInterval(charDuration);
            }
        }

        currentTextTween = textSequence;
    }

    // Enhanced version with character effects for TextMeshPro
    private void AnimateTextWithEffects(string targetText)
    {
        dialogueText.text = targetText;
        dialogueText.maxVisibleCharacters = 0;

        // Animate visible characters
        currentTextTween = DOTween.To(
            () => dialogueText.maxVisibleCharacters,
            x => dialogueText.maxVisibleCharacters = x,
            targetText.Length,
            textAnimationDuration
        ).SetEase(textEaseType);

        // Optional: Add wave effect to revealed characters
        currentTextTween.OnUpdate(() =>
        {
            // Create a subtle wave effect on visible characters
            dialogueText.ForceMeshUpdate();
            var textInfo = dialogueText.textInfo;

            for (int i = 0; i < dialogueText.maxVisibleCharacters && i < textInfo.characterCount; i++)
            {
                if (!textInfo.characterInfo[i].isVisible) continue;

                var meshInfo = textInfo.meshInfo[textInfo.characterInfo[i].materialReferenceIndex];
                int vertexIndex = textInfo.characterInfo[i].vertexIndex;

                // Simple wave effect
                float waveHeight = Mathf.Sin(Time.time * 3f + i * 0.3f) * 2f;

                Vector3 offset = new Vector3(0, waveHeight, 0);
                meshInfo.vertices[vertexIndex + 0] += offset;
                meshInfo.vertices[vertexIndex + 1] += offset;
                meshInfo.vertices[vertexIndex + 2] += offset;
                meshInfo.vertices[vertexIndex + 3] += offset;
            }

            dialogueText.UpdateVertexData();
        });
    }

    private IEnumerator SlideInDialogueBox()
    {
        // Use DOTween for smoother slide animation
        dialogueBoxTransform.DOAnchorPos(visiblePosition, 1f / slideSpeed)
            .SetEase(Ease.OutQuart)
            .OnComplete(() => DisplayLine());

        yield return new WaitForSeconds(1f / slideSpeed);
    }

    private IEnumerator SlideOutDialogueBox()
    {
        isDialogueActive = false;
        GameManager.Instance.canStartBalling = true;

        // Kill any active text animation
        if (currentTextTween != null && currentTextTween.IsActive())
        {
            currentTextTween.Kill();
        }

        // Use DOTween for smoother slide animation
        dialogueBoxTransform.DOAnchorPos(hiddenPosition, 1f / slideSpeed)
            .SetEase(Ease.InQuart)
            .OnComplete(() => dialogueBox.SetActive(false));

        yield return new WaitForSeconds(1f / slideSpeed);
    }

    void Update()
    {
        inactivityTimer += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape))
        {
            inactivityTimer = 0f;
        }

        if (Input.GetKeyDown(KeyCode.Space)) NextDialogue();
        if (Input.GetKeyDown(KeyCode.Escape)) SkipDialogue();

        if (inactivityTimer >= autoAdvanceTime)
        {
            //NextDialogue();
        }
    }

    void LoadNextDialogue()
    {
        NextDialogue();
    }

    void OnDestroy()
    {
        // Clean up DOTween animations
        if (currentTextTween != null && currentTextTween.IsActive())
        {
            currentTextTween.Kill();
        }

        // Kill all tweens on this transform
        dialogueBoxTransform.DOKill();
    }
}