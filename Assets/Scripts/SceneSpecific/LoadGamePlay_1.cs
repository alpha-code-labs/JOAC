using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadGamePlay_1 : MonoBehaviour
{
    public Button ContinueButton;
    private string sceneName = "GamePlay_1";
    //Start is called before the first frame update
    void Start()
    {
        sceneName = SaveManager.LoadSceneName();
        if (sceneName == null)
        {
            sceneName = "GamePlay_1";
        }
        Debug.Log(sceneName + " Scene Name");
        ContinueButton.onClick.AddListener(LoadNextScene);
    }
    void LoadNextScene()
    {
        SceneManager.LoadScene(sceneName);
        PlayerPrefs.SetInt("controlCenterIntroduced", 1);
    }
}
