using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroTimelineManager : MonoBehaviour
{

    public GameObject Timeline_1;
    public GameObject Timeline_2;

    // Start is called before the first frame update
    void Start()
    {
        Timeline_1.SetActive(false);
        Timeline_2.SetActive(false);
        bool gameCenterIntroduced = SaveManager.LoadGameCenterIntroduced();
        if (gameCenterIntroduced)
        {
            Timeline_2.SetActive(true);
        }
        else
        {
            Timeline_1.SetActive(true);
            StartCoroutine(LogCutsceneStarted());
        }
    }

    IEnumerator LogCutsceneStarted()
    {
        yield return new WaitForSeconds(2f);
        FirebaseManager.IntroCutsceneEvent("started");
        Debug.Log("Intro cutscene started");
    }

}
