using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayShot : MonoBehaviour
{
    public Animator _anim;
    public Ball _currentBall;
    private bool isDefaultPosition = true;
    public Vector3 startPosition = new Vector3(-8.05000019f, 0f, 0.0299999993f);
    public Vector3 startRotation = new Vector3(355.586761f, 138.014694f, 355.192352f);
    public Collider batCollider;
    public float DistanceToBall;
    public string LockedShot;
    public float lateralDistance;
    public float lateralDistanceWithDirection;
    public string ShotDirection;
    public GameObject cutShot_cutScene;



    //for Gizmos
    public float PerfectRange = 4f;
    public float VeryGoodRange = 6f;
    public float GoodRange = 10f;
    public float EarlyRange = 11f;
    public float BadRange = 12f;
    public Vector3 rangeOffset = Vector3.zero;
    public float detectionHeight = 2f;
    public float detectionWidth = 2f;


    public GameObject Bat;

    public Transform batTransform_square;
    public Transform batTransform_cover_left;
    public Transform batTransform_cover_right;
    public Transform batTransform_straight;
    public Transform batTransform_left;

    ////stumps position
    //public Transform middleStumpPosition;
    //public Transform legStumpPosition;
    //public Transform offStumpPosition;

    private ShotPlaybackManager shotPlaybackManager;


    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.pointerUp = false;
        shotPlaybackManager = GameObject.Find("ShotPlayBackManager").GetComponent<ShotPlaybackManager>();

        //testing runtime timeline creation
        //shotPlaybackManager.PlaySequence("BACKFOOT_DEFENCE_DEFLECT", "BACKFOOT_DEFENCE_DEFLECT");
    }

    // Update is called once per frame
    void Update()
    {

        if (GameManager.Instance.pointerUp && GameManager.Instance.canHit)
        {
            Debug.Log("Pointer up and can hit");
            isDefaultPosition = false;
            GameManager.Instance.pointerUp = false;
            GameManager.Instance.startVideoSequence = true;
            GameManager.Instance.triedHitting = true;
            StartCoroutine(ResetTriedToHit());
            //GameObject.Find("Fielder").GetComponent<AIAgentController>().updateTarget(GameManager.Instance.currentBall);
            UpdateRange();
        }


        if (IsAnimationPlaying("Idle") && !isDefaultPosition)
        {
            //set default position of the player
            transform.position = GameManager.Instance.playerTransformBeforeAnimation.position;
            transform.rotation = Quaternion.Euler(startRotation);
            isDefaultPosition = true;
            batCollider.enabled = true;
        }

        GameManager.Instance.isAnimationPlaying = !IsAnimationPlaying("Idle");
    }

    bool IsAnimationPlaying(string animName)
    {
        // Get the Animator's current state info from layer 0 (default layer)
        AnimatorStateInfo stateInfo = _anim.GetCurrentAnimatorStateInfo(0);
        // Compare the name of the current state with the desired animation name
        return stateInfo.IsName(animName);
    }



    void UpdateRange()
    {
        float distanceToBall = Vector3.Distance(new Vector3(transform.position.x, 0f, 0f), new Vector3(GameManager.Instance.currentBall.transform.position.x, 0f, 0f));
        float horizontalDistanceToBall = Vector3.Distance(new Vector3(0f, 0f, transform.position.z), new Vector3(0f, 0f, GameManager.Instance.currentBall.transform.position.z));
        lateralDistance = horizontalDistanceToBall;
        Debug.Log(GameManager.Instance.currentBall.transform.position.x + " ball position from game manager... " + transform.position.x + " player position .." + distanceToBall + "... calculated distance");
        DistanceToBall = distanceToBall;


        bool enableCollider = true;

        //check wether the ball is on the left, right or coming to the batsman
        bool ballOnLeft = false;
        GameObject _currentBall = GameManager.Instance.currentBall;

        lateralDistanceWithDirection = transform.position.z - _currentBall.transform.position.z;
        if ((transform.position.z - _currentBall.transform.position.z) > 0)
        {
            ballOnLeft = true;
        }
        else ballOnLeft = false;

        string shotDirection = GameManager.Instance.ShotDirectionName;
        ShotDirection = shotDirection;

        if (shotDirection == "Square" || shotDirection == "Cover Right")
        {
            if ((transform.position.z - _currentBall.transform.position.z) > 0) enableCollider = true;
            else enableCollider = false;
        }
        else if (shotDirection == "Cover Left" || shotDirection == "Left")
        {
            if ((transform.position.z - _currentBall.transform.position.z) > -0.135f) enableCollider = true;
            else enableCollider = false;
        }
        else //shotDirection is Straight
        {
            if ((transform.position.z - _currentBall.transform.position.z) < 0.6f || (transform.position.z - _currentBall.transform.position.z) > -0.1f) // player is quite close to the center so enable the collider
            {
                enableCollider = true;
            }
            else enableCollider = false;
        }

        if (!enableCollider)
        {
            GameManager.Instance.range = "Out Of Range";
            UIManager.Instance.ShowFlashMessage("Missed");
            LockedShot = "Very Early";
            //disable collision
            GetComponent<Collider>().enabled = false;
        }
        else
        {
            GetComponent<Collider>().enabled = true;
        }

        if (horizontalDistanceToBall > 1f)
        {
            GameManager.Instance.range = "Out Of Range";
            GetComponent<Collider>().enabled = false;
            batCollider.enabled = false;
            LockedShot = "Very Early";

            Debug.Log("Hit Out of Range");
            UIManager.Instance.ShowFlashMessage("Out of Range");
        }
        else if (distanceToBall > BadRange)
        {
            GameManager.Instance.range = "Very Early";
            batCollider.enabled = false;
            LockedShot = "Very Early";

            Debug.Log("Hit Very Early");
            UIManager.Instance.ShowFlashMessage("Very Early");
        }
        else if (distanceToBall > EarlyRange)
        {
            GameManager.Instance.range = "Early";
            LockedShot = "Early";
            Debug.Log("Hit Early");
            UIManager.Instance.ShowFlashMessage("Early");
        }
        else if (distanceToBall > GoodRange)
        {
            GameManager.Instance.range = "Good";
            LockedShot = "Good";
            Debug.Log("Hit Good");
            if (GameManager.Instance.currentBallType.BallLength == "yorker")
            {
                UIManager.Instance.ShowFlashMessage("Good Defence");
            }
            else
            {
                UIManager.Instance.ShowFlashMessage("Good");
            }
        }
        else if (distanceToBall > VeryGoodRange)
        {
            GameManager.Instance.range = "Very Good";
            LockedShot = "Very Good";
            Debug.Log("Hit Very Good");
            if (GameManager.Instance.currentBallType.BallLength == "yorker")
            {
                UIManager.Instance.ShowFlashMessage("Very Good Defence");
            }
            else
            {
                UIManager.Instance.ShowFlashMessage("Very Good");
            }
        }
        else if (distanceToBall > (PerfectRange - 0.4f))
        {
            GameManager.Instance.range = "Perfect";
            LockedShot = "Perfect";
            Debug.Log("Hit Perfect");
            if (GameManager.Instance.currentBallType.BallLength == "yorker")
            {
                UIManager.Instance.ShowFlashMessage("Perfect Defence");
            }
            else
            {
                UIManager.Instance.ShowFlashMessage("Perfect");
            }

        }
        else if (distanceToBall > (PerfectRange - 0.9f))
        {
            GameManager.Instance.range = "Very Good";
            if (GameManager.Instance.currentBallType.BallLength == "yorker")
            {
                UIManager.Instance.ShowFlashMessage("Very Good Defence");
            }
            else
            {
                UIManager.Instance.ShowFlashMessage("Very Good");
            }
            LockedShot = "Very Good";
            Debug.Log("Hit Very Good");
        }
        else
        {
            GameManager.Instance.range = "Late";
            UIManager.Instance.ShowFlashMessage("Late");
            LockedShot = "Late";
            batCollider.enabled = false;
            Debug.Log("Hit Late");
        }

        string typeOfBall_, lineOfBall_, lengthOfBall_, inOrOutOfRange_, batArrowPllForce_, batArrowDirection_, positionofPointC_, timing_;

        //type of ball
        switch (GameManager.Instance.currentBallType.BallVariation)
        {
            case "inswing": typeOfBall_ = "In Swing"; break;
            case "outswing": typeOfBall_ = "Out Swing"; break;
            case "fast": typeOfBall_ = "Fast"; break;
            case "legSpin": typeOfBall_ = "Leg Spin"; break;
            case "offSpin": typeOfBall_ = "Off Spin"; break;
            default: typeOfBall_ = ""; break;
        }

        //line of ball
        switch (GameManager.Instance.currentBallType.BallLine)
        {
            case "wayOutsideOff": lineOfBall_ = "Way Outside Off"; break;
            case "outsideOff": lineOfBall_ = "Outside Off"; break;
            case "atTheStumps": lineOfBall_ = "At the Stumps"; break;
            case "downTheLeg": lineOfBall_ = "Outside Leg"; break;
            case "outsideLg": lineOfBall_ = "Outside Leg"; break;
            case "wayOutsideLeg": lineOfBall_ = "Way Outside Leg"; break;
            default: lineOfBall_ = ""; break;
        }

        //Length of ball
        switch (GameManager.Instance.currentBallType.BallLength)
        {
            case "short": lengthOfBall_ = "Short"; break;
            case "goodLength": lengthOfBall_ = "Good Length"; break;
            case "fullLength": lengthOfBall_ = "Full Length"; break;
            case "yorker": lengthOfBall_ = "Yorker"; break;
            case "fullToss": lengthOfBall_ = "Full Toss"; break;
            default: lengthOfBall_ = ""; break;
        }

        //bat arrow pull force
        if (GameManager.Instance.lockedSliderScoreOnRelease < 0.4f) batArrowPllForce_ = "Mild Pull";
        else if (GameManager.Instance.lockedSliderScoreOnRelease < 0.8f) batArrowPllForce_ = "Moderate Pull";
        else batArrowPllForce_ = "Full Pull";

        //bat arrow direction
        switch (GameManager.Instance.ShotDirectionName)
        {
            case "Square": batArrowDirection_ = "Square of the Wicket Off Side"; break;
            case "Straight": batArrowDirection_ = "Straight Down the Ground"; break;
            case "Cover Left": batArrowDirection_ = "Through the Covers Leg Side"; break;
            case "Cover Right": batArrowDirection_ = "Through the Covers Off Side"; break;
            case "Left": batArrowDirection_ = "Square of the Wicket Leg Side"; break;
            default: batArrowDirection_ = ""; break;
        }

        //position of point c
        positionofPointC_ = horizontalDistanceToBall > 0 ? "Right of the Batsman" : "Left of the Batsman";
        // in or out of range
        inOrOutOfRange_ = horizontalDistanceToBall > 1f ? "Out of Range" : "In Range";

        if (positionofPointC_ == "Left of the Batsman" && horizontalDistanceToBall < 1f)
        {
            UIManager.Instance.ShowFlashMessage("Wide Ball");
            StartCoroutine(UpdateScoreGUIWithDelay(.3f, 1));
            return;
        }

        //timing
        timing_ = LockedShot;

        Debug.Log("Type of Ball: " + typeOfBall_ + "\nLine of Ball: " + lineOfBall_ + "\nLength of Ball: " + inOrOutOfRange_ + "\nIn or Out of Range" + lengthOfBall_ + "\nBat Arrow Pull Force: " + batArrowPllForce_ + "\nBat Arrow Direction: " + batArrowDirection_ + "\nPosition of Point C: " + positionofPointC_ + "\nTiming: " + timing_);

        //typeOfBall_, lineOfBall_, lengthOfBall_, inOrOutOfRange_, batArrowPllForce_, batArrowDirection_, positionofPointC_, timing_;

        var input = new List<string> { typeOfBall_, lineOfBall_, lengthOfBall_, inOrOutOfRange_, positionofPointC_, batArrowPllForce_, batArrowDirection_, timing_ };
        ExcelDataLoader _excelDataLoader = GetComponent<ExcelDataLoader>();

        var outputs = _excelDataLoader.GetOutput(input);
        int outcomeScore = 0;
        if (outputs != null)
        {
            foreach (var o in outputs)
                Debug.Log("Output: " + o);
            for (int i = 0; i < outputs.Count; i++)
            {
                Debug.Log("Output: " + i);
                Debug.Log(outputs[i]);
            }

            switch (outputs[4])
            {
                case "Out":
                    outcomeScore = -1;
                    break;
                case "Dot Ball":
                    outcomeScore = 0;
                    break;
                case "1 run":
                    outcomeScore = 1;
                    break;
                case "2 runs":
                    outcomeScore = 2;
                    break;
                case "3 runs":
                    outcomeScore = 3;
                    break;
                case "4 runs":
                    outcomeScore = 4;
                    break;
                case "6 runs":
                    outcomeScore = 6;
                    break;
                default:
                    outcomeScore = 0;
                    break;
            }

            var (batsmanFbxName, fielderFbxName) = _excelDataLoader.GetOutcomeFbxName(batArrowDirection_, outputs[0], outputs[1], outputs[2], outputs[3]);
            Debug.Log(batsmanFbxName + "   " + fielderFbxName + " " + outcomeScore);

            //call the PlaySequence method of ShotPlayBackManager
            //deactivate ball object
            // GameManager.Instance.currentBall.SetActive(false);
            shotPlaybackManager.PlaySequence(batsmanFbxName, fielderFbxName, outcomeScore);
            //shotPlaybackManager.PlaySequence("BACKFOOT_DEFENSE_DEFLECT", "WK_CATCHES", 0);
        }
        else
        {
            Debug.Log("No valid output found");
            // Select fallback animation based on game manager's shot direction
            string batsmanFbxPrefix = "FORWARD_DEFENSE"; // Default fallback

            switch (GameManager.Instance.ShotDirectionName)
            {
                case "Square":
                    batsmanFbxPrefix = "CUT_SHOT";
                    break;
                case "Cover Right":
                    batsmanFbxPrefix = "COVER_DRIVE";
                    break;
                case "Cover Left":
                    batsmanFbxPrefix = "SQUARE_DRIVE";
                    break;
                case "Left":
                    batsmanFbxPrefix = "LEG_DRIVE";
                    break;
                case "Straight":
                    batsmanFbxPrefix = "STRAIGHT_DRIVE";
                    break;
            }

            string batsmanFbxName = batsmanFbxPrefix + "_MISS";
            string fielderFbxName = "WK_CATCHES";
            shotPlaybackManager.PlaySequence(batsmanFbxName, fielderFbxName, 0);

        }
        Debug.Log("Resetting GameManager");
        //reset GameManager
        GameManager.Instance.canHit = false;
        GameManager.Instance.range = null;
    }

    IEnumerator ResetTriedToHit()
    {
        yield return new WaitForSeconds(2f);
        GameManager.Instance.triedHitting = false;
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
            PlayerPrefs.SetString("sceneName", GameManager.Instance.saveSceneName);
        }
        Debug.Log("calling win screen from uiManager");
        yield return new WaitForSeconds(delay);
        UIManager.Instance.showWinScreen();
    }

    ///Draw Gizmos
    void OnDrawGizmosSelected()
    {
        Vector3 adjustedPosition = transform.position + rangeOffset;

        // Define colors for different range zones
        Color[] colors = { Color.green, Color.yellow, Color.blue, new Color(1f, 0.5f, 0f), Color.red };
        float[] ranges = { PerfectRange, VeryGoodRange, GoodRange, EarlyRange, BadRange };

        // Draw all range zones with the same length but varying width
        for (int i = 0; i < ranges.Length; i++)
        {
            DrawWireCube(adjustedPosition, ranges[i], colors[i]);
        }
    }

    void DrawWireCube(Vector3 center, float width, Color color)
    {
        Gizmos.color = color;

        // Keep a constant length while adjusting width based on range
        Vector3 cubeSize = new Vector3(width * 2, detectionHeight, detectionWidth); // Fixed depth

        Gizmos.DrawWireCube(center, cubeSize);
    }
}
