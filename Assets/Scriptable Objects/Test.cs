using System.Collections;
using UnityEngine;
using UnityEngine.UI;
public class NPCDialogue : MonoBehaviour
{
    public DialogueManager dialogueManager;
    public DialogueData Test;
    public CanvasScaleLerp canvasLerp;
    public GameObject canvas;
    public Transform Player;
    public Animator animator;

    void Start()
    {
        // Start hidden (Optional)
        //canvasLerp.ScaleOut();
        //canvasLerp.ToggleCanvas();
        //dialogueManager.StartDialogue(Test);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            //canvas.SetActive(true);
            //Player.rotation = Quaternion.Euler(0, -20, 0);
           /// animator.SetBool("Turn",true);
        }


    }
}