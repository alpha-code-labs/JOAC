using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadMainGame : MonoBehaviour
{
    public Button SkipButton;


    // Start is called before the first frame update
    void Start()
    {
        SkipButton.onClick.AddListener(LoadNextScene);
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene("GamePlayaNormal");
    }
}
