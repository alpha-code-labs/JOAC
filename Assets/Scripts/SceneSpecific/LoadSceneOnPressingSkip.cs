using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadSceneOnPressingSkip : MonoBehaviour
{
    public string SceneName;
    public bool SaveScene = false;
    public string SaveSceneName;
    private Button _button;
    public string cutsceneName;

    // Start is called before the first frame update
    void Start()
    {
        _button = GetComponent<Button>();

        if (_button == null)
        {
            Debug.Log("button is null");
            return;
        }

        if (SceneName == null)
        {
            Debug.Log("Scenename is null");
            return;
        }

        _button.onClick.AddListener(LoadNextScene);
    }


    void LoadNextScene()
    {
        if (SaveScene)
        {
            SaveManager.SaveSceneName(SaveSceneName);
        }
        switch (cutsceneName)
        {
            case "intro_cutscene":
                FirebaseManager.IntroCutsceneEvent("skipped");
                break;
            case "transition_event_2":
                FirebaseManager.Transition_Event_2_CutsceneEvent("skipped");
                break;
            case "transition_event_3":
                FirebaseManager.Transition_Event_3_CutsceneEvent("skipped");
                break;
            case "transition_event_4":
                FirebaseManager.Transition_Event_4_CutsceneEvent("skipped");
                break;
            case "transition_event_5":
                FirebaseManager.Transition_Event_5_CutsceneEvent("skipped");
                break;
            case "transition_event_6":
                FirebaseManager.Transition_Event_6_CutsceneEvent("skipped");
                break;
            case "transition_event_7":
                FirebaseManager.Transition_Event_7_CutsceneEvent("skipped");
                break;
            case "transition_event_8":
                FirebaseManager.Transition_Event_8_CutsceneEvent("skipped");
                break;
        }
        SceneManager.LoadScene(SceneName);
    }

}
