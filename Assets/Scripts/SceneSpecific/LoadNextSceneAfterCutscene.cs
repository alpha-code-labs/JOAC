using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadNextSceneAfterCutscene : MonoBehaviour
{
    public string SceneName;
    public bool saveScene = false;
    public string saveSceneName;
    public bool resetData = true;
    public string cutsceneName;



    // Start is called before the first frame update
    void Start()
    {
        if (resetData)
        {
            PlayerPrefs.DeleteAll();
        }
        if (saveScene)
        {
            SaveManager.SaveSceneName(saveSceneName);
        }

        switch (cutsceneName)
        {
            case "intro_cutscene":
                FirebaseManager.IntroCutsceneEvent("finished");
                break;
            case "transition_event_2":
                FirebaseManager.Transition_Event_2_CutsceneEvent("finished");
                break;
            case "transition_event_3":
                FirebaseManager.Transition_Event_3_CutsceneEvent("finished");
                break;
            case "transition_event_4":
                FirebaseManager.Transition_Event_4_CutsceneEvent("finished");
                break;
            case "transition_event_5":
                FirebaseManager.Transition_Event_5_CutsceneEvent("finished");
                break;
            case "transition_event_6":
                FirebaseManager.Transition_Event_6_CutsceneEvent("finished");
                break;
            case "transition_event_7":
                FirebaseManager.Transition_Event_7_CutsceneEvent("finished");
                break;
            case "transition_event_8":
                FirebaseManager.Transition_Event_8_CutsceneEvent("finished");
                break;
        }
        SceneManager.LoadScene(SceneName);

    }


}
