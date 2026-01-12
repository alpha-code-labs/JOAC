using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class ShotPlaybackManager : MonoBehaviour
{
    [Header("Timeline")]
    public PlayableDirector director;
    public float cleanupDelay = 0.01f;
    public float startTimeInSeconds = 0.0f;
    public float reduceDurationBy = 0.5f;

    [Header("Game Objects")]
    public GameObject player;
    public GameObject stumps;
    public GameObject bowler;
    public GameObject wicketKeeper;
    public GameObject ball;

    [Header("Sequence Objects")]
    public Transform batsmanParent;  // Parent object containing all batsman FBX files
    public Transform fielderParent;  // Parent object containing all fielder FBX files

    // Cache for quick lookups
    private Dictionary<string, GameObject> batsmanObjects = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> fielderObjects = new Dictionary<string, GameObject>();

    private void Start()
    {
        CacheFBXObjects();
    }

    private void CacheFBXObjects()
    {
        // Cache all batsman objects
        if (batsmanParent != null)
        {
            foreach (Transform child in batsmanParent)
            {
                batsmanObjects[child.name] = child.gameObject;
                child.gameObject.SetActive(false); // Ensure all start disabled
            }
        }

        // Cache all fielder objects  
        if (fielderParent != null)
        {
            foreach (Transform child in fielderParent)
            {
                fielderObjects[child.name] = child.gameObject;
                child.gameObject.SetActive(false); // Ensure all start disabled
            }
        }

        Debug.Log($"Cached {batsmanObjects.Count} batsman objects and {fielderObjects.Count} fielder objects");
    }

    public void PlaySequence(string batsmanOutcome, string fielderBehavior, int runs)
    {
        Debug.Log(batsmanOutcome + " " + fielderBehavior + " -------sequence names");

        // Disable Player and Stumps
        if (player != null) player.SetActive(false);
        if (bowler != null) bowler.SetActive(false);

        if (batsmanOutcome != null && batsmanOutcome.Contains("STUMPS"))
        {
            if (stumps != null) stumps.SetActive(false);
        }

        if (fielderBehavior.Contains("WK_CATCHES"))
        {
            if (wicketKeeper != null) wicketKeeper.SetActive(false);
        }

        // Disable ball visually
        if (ball != null)
        {
            Renderer renderer = ball.GetComponent<Renderer>();
            renderer.enabled = false;
        }

        // Find and enable the correct FBX objects
        GameObject batsmanObj = null;
        GameObject fielderObj = null;

        // Get batsman object
        if (batsmanOutcome != null && batsmanOutcome != "Not Applicable")
        {
            if (batsmanObjects.TryGetValue(batsmanOutcome, out batsmanObj))
            {
                batsmanObj.SetActive(false); // Start disabled, activation track will handle timing
            }
            else
            {
                Debug.LogWarning($"Batsman object '{batsmanOutcome}' not found in scene!");
            }
        }

        // Get fielder object (with variant support)
        if (fielderBehavior != null && fielderBehavior != "Not Applicable")
        {
            // Check if the _1 variant exists
            string fielderVariant1 = fielderBehavior + "_1";

            if (fielderObjects.ContainsKey(fielderVariant1))
            {
                // Both variants exist, choose randomly
                string selectedFielder = Random.Range(0, 2) == 0 ? fielderVariant1 : fielderBehavior;
                fielderObjects.TryGetValue(selectedFielder, out fielderObj);
                Debug.Log($"Randomly selected fielder variant: {selectedFielder}");
            }
            else if (fielderObjects.TryGetValue(fielderBehavior, out fielderObj))
            {
                // Only the base variant exists
                Debug.Log($"Using base fielder: {fielderBehavior}");
            }
            else
            {
                Debug.LogWarning($"Fielder object '{fielderBehavior}' not found in scene!");
            }

            if (fielderObj != null)
            {
                fielderObj.SetActive(false); // Start disabled, activation track will handle timing
            }
        }

        if (batsmanObj == null || fielderObj == null)
        {
            Debug.Log("Batsman Object: " + (batsmanObj?.name ?? "null"));
            Debug.Log("Fielder Object: " + (fielderObj?.name ?? "null"));
        }

        // Get animators and clips
        Animator batsmanAnim = batsmanObj?.GetComponent<Animator>();
        Animator fielderAnim = fielderObj?.GetComponent<Animator>();

        AnimationClip batsmanClip = batsmanAnim?.runtimeAnimatorController?.animationClips?[0];
        AnimationClip fielderClip = fielderAnim?.runtimeAnimatorController?.animationClips?[0];

        // Create timeline
        TimelineAsset timeline = ScriptableObject.CreateInstance<TimelineAsset>();

        // Create tracks for batsman
        if (batsmanClip != null && batsmanObj != null)
        {
            // Animation Track for Batsman
            var batsmanAnimTrack = timeline.CreateTrack<AnimationTrack>(null, "BatsmanAnimTrack");
            var batsmanAnimClip = batsmanAnimTrack.CreateDefaultClip();
            (batsmanAnimClip.asset as AnimationPlayableAsset).clip = batsmanClip;
            batsmanAnimClip.duration = batsmanClip.length;
            // director.SetGenericBinding(batsmanAnimTrack, batsmanAnim);

            // Activation Track for Batsman
            var batsmanActivationTrack = timeline.CreateTrack<ActivationTrack>(null, "BatsmanActivationTrack");
            var batsmanActivationClip = batsmanActivationTrack.CreateDefaultClip();
            batsmanActivationClip.start = 0;
            batsmanActivationClip.duration = batsmanClip.length;
            director.SetGenericBinding(batsmanActivationTrack, batsmanObj);
        }

        // Create tracks for fielder
        if (fielderClip != null && fielderObj != null)
        {
            float fielderStartTime = batsmanClip != null ? batsmanClip.length : 0;

            // Animation Track for Fielder
            var fielderAnimTrack = timeline.CreateTrack<AnimationTrack>(null, "FielderAnimTrack");
            var fielderAnimClip = fielderAnimTrack.CreateDefaultClip();
            (fielderAnimClip.asset as AnimationPlayableAsset).clip = fielderClip;
            fielderAnimClip.start = fielderStartTime;
            fielderAnimClip.duration = fielderClip.length - reduceDurationBy;
            // director.SetGenericBinding(fielderAnimTrack, fielderAnim);

            // Activation Track for Fielder
            var fielderActivationTrack = timeline.CreateTrack<ActivationTrack>(null, "FielderActivationTrack");
            var fielderActivationClip = fielderActivationTrack.CreateDefaultClip();
            fielderActivationClip.start = fielderStartTime;
            fielderActivationClip.duration = fielderClip.length;
            director.SetGenericBinding(fielderActivationTrack, fielderObj);
        }

        director.playableAsset = timeline;
        director.time = startTimeInSeconds;
        director.Play();

        float maxDuration = batsmanClip != null ? batsmanClip.length : 0;
        if (fielderClip != null)
        {
            maxDuration += fielderClip.length;
        }

        StartCoroutine(CleanupAfter(maxDuration + cleanupDelay, batsmanObj, fielderObj));
        StartCoroutine(ActivateWicketKeeperAfterDelay(maxDuration + .5f));
        StartCoroutine(ReenableObjectsAfter(maxDuration + cleanupDelay));
        StartCoroutine(UpdateScoreGUIWithDelay(maxDuration, runs));
    }

    private IEnumerator ReenableObjectsAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (player != null) player.SetActive(true);
        if (stumps != null) stumps.SetActive(true);
        if (bowler != null) bowler.SetActive(true);
        if (ball != null)
        {
            Renderer renderer = ball.GetComponent<Renderer>();
            renderer.enabled = true;
        }
        // Clean up Timeline
        if (director != null)
        {
            director.Stop();
            director.playableAsset = null;
        }
    }

    private IEnumerator ActivateWicketKeeperAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (wicketKeeper != null) wicketKeeper.SetActive(true);
    }

    IEnumerator UpdateScoreGUIWithDelay(float delay, int runs)
    {
        yield return new WaitForSeconds(delay);

        if (runs == -1)
        {
            GameManager.Instance.gameOver = true;
            StartCoroutine(ShowGameOverScreen(.3f));
            runs = 0;
        }

        int maxBalls = GameManager.Instance.maxBallsToBall;
        int ballsBalled = GameManager.Instance.ballsBalled;
        int targetScore = GameManager.Instance.targetScore;
        GameManager.Instance.currentScore += runs;
        int currentScore = GameManager.Instance.currentScore;

        //UIManager.Instance.UpdateScore(targetScore - currentScore < 0 ? 0 : targetScore - currentScore, maxBalls - ballsBalled);
        int targetRemaining = targetScore - currentScore < 0 ? 0 : targetScore - currentScore;
        int ballsRemaining = maxBalls - ballsBalled;

        // Show run animation first
        if (runs > 0)
            UIManager.Instance.ShowRunsScored(runs);

        // Then update the score UI
        UIManager.Instance.UpdateScore(targetRemaining, ballsRemaining);

        if (targetScore - currentScore < 1)
        {
            Debug.Log("gameOver you won");
            GameManager.Instance.gameOver = true;
            StartCoroutine(ShowWinScreen(1.3f));
        }
        else if (targetRemaining > 0 && ballsRemaining < 1)
        {
            Debug.Log("gameOver you lost");
            GameManager.Instance.gameOver = true;
            StartCoroutine(ShowGameOverScreen(.3f));
        }
        if (!GameManager.Instance.gameOver)
            GameManager.Instance.canThrowNextBall = true;
    }

    IEnumerator ShowGameOverScreen(float delay)
    {
        yield return new WaitForSeconds(delay);
        UIManager.Instance.showLostScreen();
    }

    IEnumerator ShowWinScreen(float delay)
    {
        if (GameManager.Instance.saveSceneNameOnWin)
        {
            Debug.Log("Saving scene name: " + GameManager.Instance.saveSceneName);
            SaveManager.SaveSceneName(GameManager.Instance.saveSceneName);
            SaveManager.SaveCoins(GameManager.Instance.awarededCoins.ToString());

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
            GameManager.Instance.saveSceneNameOnWin = false;
        }
        Debug.Log("calling win screen from uiManager");
        yield return new WaitForSeconds(delay);
        UIManager.Instance.showWinScreen();
    }

    private IEnumerator CleanupAfter(float delay, params GameObject[] sequenceObjects)
    {
        yield return new WaitForSeconds(delay);

        // Instead of destroying, just disable the sequence objects
        foreach (var go in sequenceObjects)
        {
            if (go != null)
                go.SetActive(false);
        }
    }

    // Optional: Method to manually refresh the cache if objects are added/removed at runtime
    public void RefreshObjectCache()
    {
        batsmanObjects.Clear();
        fielderObjects.Clear();
        CacheFBXObjects();
    }
}