using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;


public class GameLoop_GamePlay_1 : MonoBehaviour
{

    [Header("Coin Animation")]
    public CoinCollectionAnimator coinAnimator; // Drag your coin animator here

    public GameObject GameOverPanel;
    public GameObject WinMenu;
    public GameObject LostMenu;


    public Button ContinueButton;
    public Button MainMenuButton_Win;
    public Button MainMenuButton_Lost;
    public Button RetryButton;

    public int maxBalls = 10;
    private int hitsToWin = 8;

    private bool MenuOpen = false;

    public int rewardCoins = 30;

    public LeanAnimator LeanAnimator;


    // Start is called before the first frame update
    void Start()
    {
        GameOverPanel.SetActive(false);
        WinMenu.SetActive(false);
        LostMenu.SetActive(false);
        //ContinueButton.AddListener()
        MainMenuButton_Lost.onClick.AddListener(LoadMainMenu);
        MainMenuButton_Win.onClick.AddListener(LoadMainMenu);
        RetryButton.onClick.AddListener(ResetGame);
        SaveManager.SaveGameCenterIntroduced(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.ballsBalled >= GameManager.Instance.maxBallsToBall && !MenuOpen)
        {
            //show end menu
            StartCoroutine(showMenuWithDelay(4.5f));
            MenuOpen = true;

        }
    }


    IEnumerator showMenuWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowMenu();
    }

    void ShowMenu()
    {
        if (GameManager.Instance.ballsHit > hitsToWin)
        {
            GameOverPanel.SetActive(true);
            LeanAnimator.ShowPanel(WinMenu);

            // Start the coin collection animation
            if (coinAnimator != null)
            {
                coinAnimator.PlayCoinCollectionAnimation(rewardCoins);
            }

            SaveManager.SaveSceneName("Transition_Event_2");
            int coins = int.Parse(SaveManager.LoadCoins());
            coins += rewardCoins;
            SaveManager.SaveCoins(coins.ToString());
        }
        else
        {
            GameOverPanel.SetActive(true);
            LeanAnimator.ShowPanel(LostMenu);
        }

        // Pause the game after animations complete
        StartCoroutine(PauseAfterAnimation());
    }
    IEnumerator PauseAfterAnimation()
    {
        //Wait for the animation duration (0.5s) plus a small buffer
        yield return new WaitForSeconds(0.6f);
        Time.timeScale = 0f;
    }

    void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu");
    }

    void ResetGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}
