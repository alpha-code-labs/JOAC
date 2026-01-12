using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    public string reference;
    public string DialogueText;
    public Sprite portrait;
    public AudioClip voiceClip; // Audio for dialogue
    public float displayTime;
}

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue System/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    public DialogueLine[] dialogueLines;
}
