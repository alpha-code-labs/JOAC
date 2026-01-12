using UnityEngine;
using static System.Net.Mime.MediaTypeNames;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Configuration;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.Log("game manager is null");
            }
            return _instance;
        }
    }

    [Header("Ball Related")]
    public bool SpinDirection = false;
    public float SpingStrength = 10.0f;
    public float lateralSwingDistance { get; set; }
    public bool enableTarget = true;
    public Vector3 targetPosition { get; set; }
    public string ballVariation = "inswing";
    public GameObject stumps;
    public float swingInfluence = 1f;
    public float swingForce = 2f;
    public float swingForceFactor = 1;
    public BallType currentBallType;
    public float SliderPosition;
    public bool startSlider;
    public bool resetSlider;
    public int ballsHit;
    public int missedBallCount = 0;
    public int ballsBalled = 0;
    public int maxBallsToBall = 36;
    public int currentScore = 0;
    public int targetScore = 90;
    public float sliderScore = 0;
    public float lockedSliderScoreOnRelease = 0f;
    public bool pauseSlider = false;
    public float animationSpeed = 1.0f;
    public string range;
    public Vector3 currentBallPosition { get; set; }
    public GameObject currentBall;
    public bool pointerUp;
    public bool startVideoSequence;
    public float rightHandPositionWeight, lookAtWeight;
    public GameObject rightHandTargetBall;
    public GameObject player;
    public float side;
    public float dist;
    public bool activateFielding = true;
    public bool canThrowNextBall = true;
    public Transform playerTransformBeforeAnimation;
    public bool isAnimationPlaying;
    public float dragCoefficient;
    public Vector3 ShotDirection;
    public string ShotDirectionName;
    public bool canStartBalling = false;
    public bool canHit = false;
    public bool triedHitting = false;
    public void restartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public bool gameOver = false;
    public bool saveSceneNameOnWin = false;
    public string saveSceneName;
    public int awarededCoins = 30;
    public void Awake()
    {
        _instance = this;
    }

    void Update()
    {
        if (PauseManager.Instance != null && PauseManager.Instance.IsPaused)
            return;
        if (pointerUp)
        {
            StartCoroutine(PointerUpCoolDown(.8f));
        }
        if (gameOver && currentScore >= targetScore && saveSceneNameOnWin)
        {
            Debug.Log("Saving scene name: " + saveSceneName);
            SaveManager.SaveSceneName(saveSceneName);
            SaveManager.SaveCoins(awarededCoins.ToString());
            string sceneName = SaveManager.LoadSceneName();
            string coins = SaveManager.LoadCoins();
            if (!string.IsNullOrEmpty(sceneName))
            {
                Debug.Log(sceneName + " saved...");
            }
            if (!string.IsNullOrEmpty(coins))
            {
                Debug.Log(coins + " saved...");
            }
            saveSceneNameOnWin = false;
        }
    }

    IEnumerator PointerUpCoolDown(float delay)
    {
        yield return new WaitForSeconds(delay);
        pointerUp = false;
    }
}
