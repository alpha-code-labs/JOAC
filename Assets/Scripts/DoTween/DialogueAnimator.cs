using TMPro;
using UnityEngine;
using DG.Tweening;
using System.Collections;

public class DialogueTextAnimator : MonoBehaviour
{
    [Header("Text Settings")]
    [TextArea]
    public string fullText;
    public float characterDelay = 0.05f;
    public bool autoStart = true;

    private TMP_Text tmpText;
    private Coroutine typingCoroutine;

    private void Awake()
    {
        tmpText = GetComponent<TMP_Text>();
        if (tmpText == null)
        {
            Debug.LogError("DialogueTextAnimator requires a TextMeshProUGUI component.");
        }
    }

    private void OnEnable()
    {
        if (autoStart)
        {
            StartTyping();
        }
    }

    public void StartTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeText());
    }

    private IEnumerator TypeText()
    {
        tmpText.text = "";
        for (int i = 0; i < fullText.Length; i++)
        {
            tmpText.text += fullText[i];
            yield return new WaitForSeconds(characterDelay);
        }
    }

    public void SetTextAndStart(string newText)
    {
        fullText = newText;
        StartTyping();
    }
}
