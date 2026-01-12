using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadNextScene : MonoBehaviour
{
    public string SceneName;
    public bool saveScene = false;
    public string saveSceneName;
    public bool resetData = true;



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
        SceneManager.LoadScene(SceneName);

    }


}
