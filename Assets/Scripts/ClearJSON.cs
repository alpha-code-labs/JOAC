using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearJSON : MonoBehaviour
{
    public bool clearJSON = false;
    // Start is called before the first frame update
    void Start()
    {
        if (clearJSON)
        {
            SaveManager.SaveCoins("0");
            SaveManager.SaveGameCenterIntroduced(false);
            SaveManager.SaveTriviaSceneDialoguesIntroduced(false);
            SaveManager.SaveChapterCompleted(false);
            SaveManager.SaveSceneName("GamePlay_1");
        }
    }
}
