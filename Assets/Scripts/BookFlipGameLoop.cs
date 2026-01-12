using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class BookFlipGameLoop : MonoBehaviour
{
    public NumberAnimation numberAnimation;
    public TextMeshProUGUI numberText; // Assign in Inspector
    public Transform targetPosition;  // Assign the target UI position
    public float duration = 1.5f;

    public Image OutImage;
    public Image DotBallImage;

    public TextMeshProUGUI remainingRunsText;
    public TextMeshProUGUI remainingBallsText;

    private int score = 0;
    public int maxBallsToBall;
    public int ballsBalled;
    public int targetScore;
    public int currentScore;
    public Vector3 defaultPosition;
    public int reawardCoins;

    public GameObject GameOverPanel;
    public GameObject WinMenu;
    public GameObject LostMenu;

    public Button ContinueButton;
    public Button RetryButton;
    public Button MainMenuButton;

    void Start()
    {
        GameOverPanel.SetActive(false);
        WinMenu.SetActive(false);
        LostMenu.SetActive(false);

        //addlisteners for butttons
        ContinueButton.onClick.AddListener(LoadMainMenu);
        MainMenuButton.onClick.AddListener(LoadMainMenu);
        RetryButton.onClick.AddListener(ReloadScene);
        
        //Subscribe to the animation completion event
        numberAnimation.OnAnimationComplete += AssignScore;
        defaultPosition = numberText.transform.position;

        remainingRunsText.text = "" + targetScore;
        remainingBallsText.text = "" + maxBallsToBall;
    }

    void AssignScore(int finalNumber)
    {
        finalNumber = finalNumber % 10;

        switch (finalNumber)
        {
            case 0: {
                    PlayerOut();
                    break;
                }
            case 1:
                {
                    currentScore += 1;
                    if (currentScore >= targetScore)
                    {
                        PlayerWon();
                    }
                    break;
                }
            case 2: {
                    currentScore += 2;
                    if(currentScore >= targetScore)
                    {
                        PlayerWon();
                    }
                    break;
                }
            case 3:
                {
                    currentScore += 3;
                    if (currentScore >= targetScore)
                    {
                        PlayerWon();
                    }
                    break;
                }
            case 4:
                 {
                    currentScore += 4;
                    if (currentScore >= targetScore)
                    {
                        PlayerWon();
                    }
                    break;
                }
            case 6:
                 {
                    currentScore += 6;
                    if (currentScore >= targetScore)
                    {
                        PlayerWon();
                    }
                    break;
                }

            default: break;
        }

        ballsBalled++;

       
        //show lost menu if player can not score runs
        if(ballsBalled == maxBallsToBall && currentScore < targetScore)
        {
            StartCoroutine(PlayerLost());
        }
        
        
        if(finalNumber == 1 || finalNumber == 2 || finalNumber == 3 || finalNumber == 4 || finalNumber == 6)
        {
            StartCoroutine(AnimateNumber(finalNumber));
        }
        else if(finalNumber != 0)
        {
            StartCoroutine(FlashDotBallImage());
        }

        int remainingBalls = maxBallsToBall - ballsBalled;
        remainingBallsText.text = "" + remainingBalls;

    }

    void PlayerOut()
    {
        //show lost menu
        StartCoroutine(FlashOutImage());
        StartCoroutine(PlayerLost());
    }

    void PlayerWon()
    {
        PlayerPrefs.SetString("sceneName", "Transition_Event_2");
        GameOverPanel.SetActive(true);
        WinMenu.SetActive(true);
    }

    IEnumerator PlayerLost()
    {
        yield return new WaitForSeconds(2.15f);
        GameOverPanel.SetActive(true);
        LostMenu.SetActive(true);
    }

    void LoadMainMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }

    void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    IEnumerator AnimateNumber(int number)
    {
        numberText.gameObject.SetActive(true);
        numberText.text = "+" + number;
        numberText.transform.position = defaultPosition;
        Vector3 startPos = numberText.transform.position;
        Vector3 endPos = targetPosition.position;
        float startSize = 100f;  // Starting font size
        float endSize = 30f;  // Smaller size at target
        float startOpacity = 1f;
        float endOpacity = 0.3f;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Smoothly interpolate position, size, and opacity
            numberText.transform.position = Vector3.Lerp(startPos, endPos, t);
            numberText.fontSize = Mathf.Lerp(startSize, endSize, t);
            numberText.color = new Color(1f, 1f, 1f, Mathf.Lerp(startOpacity, endOpacity, t));

            yield return null;
        }

        // Ensure final values are set
        numberText.transform.position = endPos;
        numberText.fontSize = endSize;
        numberText.color = new Color(1f, 1f, 1f, endOpacity);

        // Wait briefly and then hide the text
        yield return new WaitForSeconds(0.5f);
        numberText.gameObject.SetActive(false);

        int neededScore = (targetScore - currentScore) > 0 ? (targetScore - currentScore) : 0;
        remainingRunsText.text = "" + neededScore;

        //scoreText.text = "Need: " + neededScore + " in " + remainingBalls + " balls";
        Debug.Log("Score Assigned: " + score);
       
    }

    IEnumerator FlashOutImage()
    {
        OutImage.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        OutImage.gameObject.SetActive(false);
    }

    IEnumerator FlashDotBallImage()
    {
        DotBallImage.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        DotBallImage.gameObject.SetActive(false);
    }
}
