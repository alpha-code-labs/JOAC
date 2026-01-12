using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadSceneOnButtonPress : MonoBehaviour
{
    public string SceneName;
    public bool SaveScene = false;
    public string SaveSceneName;
    private Button _button;

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
        SceneManager.LoadScene(SceneName);
    }

}
