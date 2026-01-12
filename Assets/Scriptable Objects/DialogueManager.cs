using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject dialogueBox;
    public TextMeshProUGUI dialogueText;
    public Image portraitImage;
    public AudioSource audioSource;
    public Button skipButton;

    [Header("Animation Settings")]
    public RectTransform dialogueBoxTransform;
    public Vector2 hiddenPosition = new Vector2(-500, 0);
    public Vector2 visiblePosition = new Vector2(0, 0);
    public float slideSpeed = 5f;
    public float typeSpeed = 0.05f;
    public float autoAdvanceTime = 10f;

    private DialogueData currentDialogue;
    private int currentLineIndex = 0;
    private bool isDialogueActive = false;
    private Coroutine typewriterCoroutine;
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

        if(skipButton != null)
        {
            skipButton.onClick.AddListener(SkipDialogue);
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
        StartCoroutine(SlideOutDialogueBox());
    }

    private void DisplayLine()
    {
        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);

        DialogueLine line = currentDialogue.dialogueLines[currentLineIndex];
        portraitImage.sprite = line.portrait;

        if (line.voiceClip != null)
        {
            audioSource.Stop();
            audioSource.clip = line.voiceClip;
            audioSource.Play();
        }

        string actualText = line.DialogueText;
        if(hindiDialogues)
            actualText=UnicodeToKrutidev.UnicodeToKrutiDev(actualText);
        typewriterCoroutine = StartCoroutine(TypeText(actualText));
        inactivityTimer = 0f;
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
        inactivityTimer += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape))
        {
            inactivityTimer = 0f;
        }

        if (Input.GetKeyDown(KeyCode.Space)) NextDialogue();
        if (Input.GetKeyDown(KeyCode.Escape)) SkipDialogue();

        if (inactivityTimer >= autoAdvanceTime)
        {
            NextDialogue();
        }
    }
}



