using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.Playables;

public class BallTester : MonoBehaviour
{
    public GameObject BallPrefab;
    public GameObject StartPoint;
    public GameObject PitchPoint;
    public GameObject BallerStumps;
    public GameObject BatsmanStumps;
    public GameObject FakeBall;
    public Animator _anim;
    public GameObject Baller;
    private Vector3 ballerInitialPosition = new Vector3(11.67f, 0f, -0.12f);

    public float SpawnDelay = 10.0f;
    private float _ballSpeed;
    public float BallTravelTime = 1.0f;
    public float DistanceBetweenWickets = 15.0f;
    public float maxSwingDistance = 10f;
    public Vector3 swingDirection = Vector3.right;
    public float swingInfluence = 1f;
    public float swingForce = 5f;
    public float BallDelay = 14.0f; // Delay between balls
    public float OverDelay = 10.0f; // Delay between overs
    public Animator _batAnimator;

    private GameObject _ball;
    private Rigidbody _rb;
    public float _horizontalDistBtwnPitchPointAndBallerStumps;
    public float verticalVelocity;
    public float _timeToPitch;
    public Vector3 targetPosition = new Vector3(0, 0, 0);
    private int currentOver = 0;
    public GameObject player;
    public GameObject MainBall;

    public PlayableDirector director;
    private Coroutine checkGameLostCoroutine;


    private Dictionary<string, float> targetPositionDictionary = new Dictionary<string, float>
        {
            { "short", 0f },
            { "goodLength", -4.5f},
            { "fullLength", -5.8f },
            { "yorker", -6.5f },
            { "fullToss", -10.0f }
        };

    private Dictionary<string, float> ballLineDictionary = new Dictionary<string, float>
        {
            { "wayOutsideOff", -0.29f },
            { "outsideOff", -0.20f },
            { "atTheStumps", 0f},
            { "downTheLeg", 0.16f },
            { "outsideLg", 0.20f },
            { "wayOutsideLeg", 0.33f }
        };

    private Dictionary<string, float> pitchTimeDictionary = new Dictionary<string, float>
        {
            { "fast", .95f  },
            { "slow", 1.3f},
            { "mediumPace", 1.05f },
        };
    public float lateralSwingDistance;

    BallType[] ballTypes_1 = new BallType[]
    {
        new BallType("slow", "wayOutsideOff", "short", "offSpin"),
        new BallType("slow", "wayOutsideOff", "short", "legSpin"),
        new BallType("mediumPace", "wayOutsideOff", "short", "offSpin"),
        new BallType("mediumPace", "wayOutsideOff", "short", "legSpin"),
        new BallType("fast", "wayOutsideOff", "short", "offSpin"),
        new BallType("fast", "wayOutsideOff", "short", "legSpin"),
        new BallType("slow", "wayOutsideOff", "fullLength", "offSpin"),
        new BallType("slow", "wayOutsideOff", "fullLength", "legSpin"),
        new BallType("mediumPace", "wayOutsideOff", "fullLength", "offSpin"),
        new BallType("mediumPace", "wayOutsideOff", "fullLength", "legSpin"),
        new BallType("fast", "wayOutsideOff", "fullLength", "offSpin"),
        new BallType("fast", "wayOutsideOff", "fullLength", "legSpin"),
        new BallType("slow", "wayOutsideOff", "goodLength", "offSpin"),
        new BallType("slow", "wayOutsideOff", "goodLength", "legSpin"),
        new BallType("mediumPace", "wayOutsideOff", "goodLength", "offSpin"),
        new BallType("mediumPace", "wayOutsideOff", "goodLength", "legSpin"),
        new BallType("fast", "wayOutsideOff", "goodLength", "offSpin"),
        new BallType("fast", "wayOutsideOff", "goodLength", "legSpin"),
        new BallType("slow", "wayOutsideOff", "yorker", "offSpin"),
        new BallType("slow", "wayOutsideOff", "yorker", "legSpin"),
        new BallType("mediumPace", "wayOutsideOff", "yorker", "offSpin"),
        new BallType("mediumPace", "wayOutsideOff", "yorker", "legSpin"),
        new BallType("fast", "wayOutsideOff", "yorker", "offSpin"),
        new BallType("fast", "wayOutsideOff", "yorker", "legSpin"),
        new BallType("slow", "wayOutsideOff", "fullToss", "offSpin"),
        new BallType("slow", "wayOutsideOff", "fullToss", "legSpin"),
        new BallType("mediumPace", "wayOutsideOff", "fullToss", "offSpin"),
        new BallType("mediumPace", "wayOutsideOff", "fullToss", "legSpin"),
        new BallType("fast", "wayOutsideOff", "fullToss", "offSpin"),
        new BallType("fast", "wayOutsideOff", "fullToss", "legSpin"),
        new BallType("slow", "outsideOff", "short", "offSpin"),
        new BallType("slow", "outsideOff", "short", "legSpin"),
        new BallType("mediumPace", "outsideOff", "short", "offSpin"),
        new BallType("mediumPace", "outsideOff", "short", "legSpin"),
        new BallType("fast", "outsideOff", "short", "offSpin"),
        new BallType("fast", "outsideOff", "short", "legSpin"),
        new BallType("slow", "outsideOff", "fullLength", "offSpin"),
        new BallType("slow", "outsideOff", "fullLength", "legSpin"),
        new BallType("mediumPace", "outsideOff", "fullLength", "offSpin"),
        new BallType("mediumPace", "outsideOff", "fullLength", "legSpin"),
        new BallType("fast", "outsideOff", "fullLength", "offSpin"),
        new BallType("fast", "outsideOff", "fullLength", "legSpin"),
        new BallType("slow", "outsideOff", "goodLength", "offSpin"),
        new BallType("slow", "outsideOff", "goodLength", "legSpin"),
        new BallType("mediumPace", "outsideOff", "goodLength", "offSpin"),
        new BallType("mediumPace", "outsideOff", "goodLength", "legSpin"),
        new BallType("fast", "outsideOff", "goodLength", "offSpin"),
        new BallType("fast", "outsideOff", "goodLength", "legSpin"),
        new BallType("slow", "outsideOff", "yorker", "offSpin"),
        new BallType("slow", "outsideOff", "yorker", "legSpin"),
        new BallType("mediumPace", "outsideOff", "yorker", "offSpin"),
        new BallType("mediumPace", "outsideOff", "yorker", "legSpin"),
        new BallType("fast", "outsideOff", "yorker", "offSpin"),
        new BallType("fast", "outsideOff", "yorker", "legSpin"),
        new BallType("slow", "outsideOff", "fullToss", "offSpin"),
        new BallType("slow", "outsideOff", "fullToss", "legSpin"),
        new BallType("mediumPace", "outsideOff", "fullToss", "offSpin"),
        new BallType("mediumPace", "outsideOff", "fullToss", "legSpin"),
        new BallType("fast", "outsideOff", "fullToss", "offSpin"),
        new BallType("fast", "outsideOff", "fullToss", "legSpin"),
        new BallType("slow", "atTheStumps", "short", "offSpin"),
        new BallType("slow", "atTheStumps", "short", "legSpin"),
        new BallType("mediumPace", "atTheStumps", "short", "offSpin"),
        new BallType("mediumPace", "atTheStumps", "short", "legSpin"),
        new BallType("fast", "atTheStumps", "short", "offSpin"),
        new BallType("fast", "atTheStumps", "short", "legSpin"),
        new BallType("slow", "atTheStumps", "fullLength", "offSpin"),
        new BallType("slow", "atTheStumps", "fullLength", "legSpin"),
        new BallType("mediumPace", "atTheStumps", "fullLength", "offSpin"),
        new BallType("mediumPace", "atTheStumps", "fullLength", "legSpin"),
        new BallType("fast", "atTheStumps", "fullLength", "offSpin"),
        new BallType("fast", "atTheStumps", "fullLength", "legSpin"),
        new BallType("slow", "atTheStumps", "goodLength", "offSpin"),
        new BallType("slow", "atTheStumps", "goodLength", "legSpin"),
        new BallType("mediumPace", "atTheStumps", "goodLength", "offSpin"),
        new BallType("mediumPace", "atTheStumps", "goodLength", "legSpin"),
        new BallType("fast", "atTheStumps", "goodLength", "offSpin"),
        new BallType("fast", "atTheStumps", "goodLength", "legSpin"),
        new BallType("slow", "atTheStumps", "yorker", "offSpin"),
        new BallType("slow", "atTheStumps", "yorker", "legSpin"),
        new BallType("mediumPace", "atTheStumps", "yorker", "offSpin"),
        new BallType("mediumPace", "atTheStumps", "yorker", "legSpin"),
        new BallType("fast", "atTheStumps", "yorker", "offSpin"),
        new BallType("fast", "atTheStumps", "yorker", "legSpin"),
        new BallType("slow", "atTheStumps", "fullToss", "offSpin"),
        new BallType("slow", "atTheStumps", "fullToss", "legSpin"),
        new BallType("mediumPace", "atTheStumps", "fullToss", "offSpin"),
        new BallType("mediumPace", "atTheStumps", "fullToss", "legSpin"),
        new BallType("fast", "atTheStumps", "fullToss", "offSpin"),
        new BallType("fast", "atTheStumps", "fullToss", "legSpin"),
    };

    BallType[] ballTypes_2 = new BallType[]
    {
        new BallType("slow", "outsideOff", "short", "fast"),
        new BallType("mediumPace", "outsideOff", "short", "fast"),
        new BallType("fast", "outsideOff", "short", "fast"),
        new BallType("slow", "outsideOff", "short", "inswing"),
        new BallType("slow", "outsideOff", "short", "outswing"),
        new BallType("mediumPace", "outsideOff", "short", "inswing"),
        new BallType("mediumPace", "outsideOff", "short", "outswing"),
        new BallType("fast", "outsideOff", "short", "inswing"),
        new BallType("fast", "outsideOff", "short", "outswing"),
        new BallType("slow", "outsideOff", "fullLength", "fast"),
        new BallType("mediumPace", "outsideOff", "fullLength", "fast"),
        new BallType("fast", "outsideOff", "fullLength", "fast"),
        new BallType("slow", "outsideOff", "fullLength", "inswing"),
        new BallType("slow", "outsideOff", "fullLength", "outswing"),
        new BallType("mediumPace", "outsideOff", "fullLength", "inswing"),
        new BallType("mediumPace", "outsideOff", "fullLength", "outswing"),
        new BallType("fast", "outsideOff", "fullLength", "inswing"),
        new BallType("fast", "outsideOff", "fullLength", "outswing"),
        new BallType("slow", "outsideOff", "goodLength", "fast"),
        new BallType("mediumPace", "outsideOff", "goodLength", "fast"),
        new BallType("fast", "outsideOff", "goodLength", "fast"),
        new BallType("slow", "outsideOff", "goodLength", "inswing"),
        new BallType("slow", "outsideOff", "goodLength", "outswing"),
        new BallType("mediumPace", "outsideOff", "goodLength", "inswing"),
        new BallType("mediumPace", "outsideOff", "goodLength", "outswing"),
        new BallType("fast", "outsideOff", "goodLength", "inswing"),
        new BallType("fast", "outsideOff", "goodLength", "outswing"),
        new BallType("slow", "outsideOff", "yorker", "fast"),
        new BallType("mediumPace", "outsideOff", "yorker", "fast"),
        new BallType("fast", "outsideOff", "yorker", "fast"),
        new BallType("slow", "outsideOff", "yorker", "inswing"),
        new BallType("slow", "outsideOff", "yorker", "outswing"),
        new BallType("mediumPace", "outsideOff", "yorker", "inswing"),
        new BallType("mediumPace", "outsideOff", "yorker", "outswing"),
        new BallType("fast", "outsideOff", "yorker", "inswing"),
        new BallType("fast", "outsideOff", "yorker", "outswing"),
        new BallType("slow", "outsideOff", "fullToss", "fast"),
        new BallType("mediumPace", "outsideOff", "fullToss", "fast"),
        new BallType("fast", "outsideOff", "fullToss", "fast"),
        new BallType("slow", "outsideOff", "fullToss", "inswing"),
        new BallType("slow", "outsideOff", "fullToss", "outswing"),
        new BallType("mediumPace", "outsideOff", "fullToss", "inswing"),
        new BallType("mediumPace", "outsideOff", "fullToss", "outswing"),
        new BallType("fast", "outsideOff", "fullToss", "inswing"),
        new BallType("fast", "outsideOff", "fullToss", "outswing"),
        new BallType("slow", "atTheStumps", "short", "fast"),
        new BallType("mediumPace", "atTheStumps", "short", "fast"),
        new BallType("fast", "atTheStumps", "short", "fast"),
        new BallType("slow", "atTheStumps", "short", "inswing"),
        new BallType("slow", "atTheStumps", "short", "outswing"),
        new BallType("mediumPace", "atTheStumps", "short", "inswing"),
        new BallType("mediumPace", "atTheStumps", "short", "outswing"),
        new BallType("fast", "atTheStumps", "short", "inswing"),
        new BallType("fast", "atTheStumps", "short", "outswing"),
        new BallType("slow", "atTheStumps", "fullLength", "fast"),
        new BallType("mediumPace", "atTheStumps", "fullLength", "fast"),
        new BallType("fast", "atTheStumps", "fullLength", "fast"),
        new BallType("slow", "atTheStumps", "fullLength", "inswing"),
        new BallType("slow", "atTheStumps", "fullLength", "outswing"),
        new BallType("mediumPace", "atTheStumps", "fullLength", "inswing"),
        new BallType("mediumPace", "atTheStumps", "fullLength", "outswing"),
        new BallType("fast", "atTheStumps", "fullLength", "inswing"),
        new BallType("fast", "atTheStumps", "fullLength", "outswing"),
        new BallType("slow", "atTheStumps", "goodLength", "fast"),
        new BallType("mediumPace", "atTheStumps", "goodLength", "fast"),
        new BallType("fast", "atTheStumps", "goodLength", "fast"),
        new BallType("slow", "atTheStumps", "goodLength", "inswing"),
        new BallType("slow", "atTheStumps", "goodLength", "outswing"),
        new BallType("mediumPace", "atTheStumps", "goodLength", "inswing"),
        new BallType("mediumPace", "atTheStumps", "goodLength", "outswing"),
        new BallType("fast", "atTheStumps", "goodLength", "inswing"),
        new BallType("fast", "atTheStumps", "goodLength", "outswing"),
        new BallType("slow", "atTheStumps", "yorker", "fast"),
        new BallType("mediumPace", "atTheStumps", "yorker", "fast"),
        new BallType("fast", "atTheStumps", "yorker", "fast"),
        new BallType("slow", "atTheStumps", "yorker", "inswing"),
        new BallType("slow", "atTheStumps", "yorker", "outswing"),
        new BallType("mediumPace", "atTheStumps", "yorker", "inswing"),
        new BallType("mediumPace", "atTheStumps", "yorker", "outswing"),
        new BallType("fast", "atTheStumps", "yorker", "inswing"),
        new BallType("fast", "atTheStumps", "yorker", "outswing"),
        new BallType("slow", "atTheStumps", "fullToss", "fast"),
        new BallType("mediumPace", "atTheStumps", "fullToss", "fast"),
        new BallType("fast", "atTheStumps", "fullToss", "fast"),
        new BallType("slow", "atTheStumps", "fullToss", "inswing"),
        new BallType("slow", "atTheStumps", "fullToss", "outswing"),
        new BallType("mediumPace", "atTheStumps", "fullToss", "inswing"),
        new BallType("mediumPace", "atTheStumps", "fullToss", "outswing"),
        new BallType("fast", "atTheStumps", "fullToss", "inswing"),
        new BallType("fast", "atTheStumps", "fullToss", "outswing"),
        new BallType("slow", "wayOutsideLeg", "short", "fast"),
        new BallType("mediumPace", "wayOutsideLeg", "short", "fast"),
        new BallType("fast", "wayOutsideLeg", "short", "fast"),
        new BallType("slow", "wayOutsideLeg", "short", "inswing"),
        new BallType("slow", "wayOutsideLeg", "short", "outswing"),
        new BallType("mediumPace", "wayOutsideLeg", "short", "inswing"),
        new BallType("mediumPace", "wayOutsideLeg", "short", "outswing"),
        new BallType("fast", "wayOutsideLeg", "short", "inswing"),
        new BallType("fast", "wayOutsideLeg", "short", "outswing"),
        new BallType("slow", "wayOutsideLeg", "fullLength", "fast"),
        new BallType("mediumPace", "wayOutsideLeg", "fullLength", "fast"),
        new BallType("fast", "wayOutsideLeg", "fullLength", "fast"),
        new BallType("slow", "wayOutsideLeg", "fullLength", "inswing"),
        new BallType("slow", "wayOutsideLeg", "fullLength", "outswing"),
        new BallType("mediumPace", "wayOutsideLeg", "fullLength", "inswing"),
        new BallType("mediumPace", "wayOutsideLeg", "fullLength", "outswing"),
        new BallType("fast", "wayOutsideLeg", "fullLength", "inswing"),
        new BallType("fast", "wayOutsideLeg", "fullLength", "outswing"),
        new BallType("slow", "wayOutsideLeg", "goodLength", "fast"),
        new BallType("mediumPace", "wayOutsideLeg", "goodLength", "fast"),
        new BallType("fast", "wayOutsideLeg", "goodLength", "fast"),
        new BallType("slow", "wayOutsideLeg", "goodLength", "inswing"),
        new BallType("slow", "wayOutsideLeg", "goodLength", "outswing"),
        new BallType("mediumPace", "wayOutsideLeg", "goodLength", "inswing"),
        new BallType("mediumPace", "wayOutsideLeg", "goodLength", "outswing"),
        new BallType("fast", "wayOutsideLeg", "goodLength", "inswing"),
        new BallType("fast", "wayOutsideLeg", "goodLength", "outswing"),
        new BallType("slow", "wayOutsideLeg", "yorker", "fast"),
        new BallType("mediumPace", "wayOutsideLeg", "yorker", "fast"),
        new BallType("fast", "wayOutsideLeg", "yorker", "fast"),
        new BallType("slow", "wayOutsideLeg", "yorker", "inswing"),
        new BallType("slow", "wayOutsideLeg", "yorker", "outswing"),
        new BallType("mediumPace", "wayOutsideLeg", "yorker", "inswing"),
        new BallType("mediumPace", "wayOutsideLeg", "yorker", "outswing"),
        new BallType("fast", "wayOutsideLeg", "yorker", "inswing"),
        new BallType("fast", "wayOutsideLeg", "yorker", "outswing"),
        new BallType("slow", "wayOutsideLeg", "fullToss", "fast"),
        new BallType("mediumPace", "wayOutsideLeg", "fullToss", "fast"),
        new BallType("fast", "wayOutsideLeg", "fullToss", "fast"),
        new BallType("slow", "wayOutsideLeg", "fullToss", "inswing"),
        new BallType("slow", "wayOutsideLeg", "fullToss", "outswing"),
        new BallType("mediumPace", "wayOutsideLeg", "fullToss", "inswing"),
        new BallType("mediumPace", "wayOutsideLeg", "fullToss", "outswing"),
        new BallType("fast", "wayOutsideLeg", "fullToss", "inswing"),
        new BallType("fast", "wayOutsideLeg", "fullToss", "outswing"),
                new BallType("slow", "downTheLeg", "short", "fast"),
        new BallType("mediumPace", "downTheLeg", "short", "fast"),
        new BallType("fast", "downTheLeg", "short", "fast"),
        new BallType("slow", "downTheLeg", "short", "inswing"),
        new BallType("slow", "downTheLeg", "short", "outswing"),
        new BallType("mediumPace", "downTheLeg", "short", "inswing"),
        new BallType("mediumPace", "downTheLeg", "short", "outswing"),
        new BallType("fast", "downTheLeg", "short", "inswing"),
        new BallType("fast", "downTheLeg", "short", "outswing"),
        new BallType("slow", "downTheLeg", "fullLength", "fast"),
        new BallType("mediumPace", "downTheLeg", "fullLength", "fast"),
        new BallType("fast", "downTheLeg", "fullLength", "fast"),
        new BallType("slow", "downTheLeg", "fullLength", "inswing"),
        new BallType("slow", "downTheLeg", "fullLength", "outswing"),
        new BallType("mediumPace", "downTheLeg", "fullLength", "inswing"),
        new BallType("mediumPace", "downTheLeg", "fullLength", "outswing"),
        new BallType("fast", "downTheLeg", "fullLength", "inswing"),
        new BallType("fast", "downTheLeg", "fullLength", "outswing"),
        new BallType("slow", "downTheLeg", "goodLength", "fast"),
        new BallType("mediumPace", "downTheLeg", "goodLength", "fast"),
        new BallType("fast", "downTheLeg", "goodLength", "fast"),
        new BallType("slow", "downTheLeg", "goodLength", "inswing"),
        new BallType("slow", "downTheLeg", "goodLength", "outswing"),
        new BallType("mediumPace", "downTheLeg", "goodLength", "inswing"),
        new BallType("mediumPace", "downTheLeg", "goodLength", "outswing"),
        new BallType("fast", "downTheLeg", "goodLength", "inswing"),
        new BallType("fast", "downTheLeg", "goodLength", "outswing"),
        new BallType("slow", "downTheLeg", "yorker", "fast"),
        new BallType("mediumPace", "downTheLeg", "yorker", "fast"),
        new BallType("fast", "downTheLeg", "yorker", "fast"),
        new BallType("slow", "downTheLeg", "yorker", "inswing"),
        new BallType("slow", "downTheLeg", "yorker", "outswing"),
        new BallType("mediumPace", "downTheLeg", "yorker", "inswing"),
        new BallType("mediumPace", "downTheLeg", "yorker", "outswing"),
        new BallType("fast", "downTheLeg", "yorker", "inswing"),
        new BallType("fast", "downTheLeg", "yorker", "outswing"),
        new BallType("slow", "downTheLeg", "fullToss", "fast"),
        new BallType("mediumPace", "downTheLeg", "fullToss", "fast"),
        new BallType("fast", "downTheLeg", "fullToss", "fast"),
        new BallType("slow", "downTheLeg", "fullToss", "inswing"),
        new BallType("slow", "downTheLeg", "fullToss", "outswing"),
        new BallType("mediumPace", "downTheLeg", "fullToss", "inswing"),
        new BallType("mediumPace", "downTheLeg", "fullToss", "outswing"),
        new BallType("fast", "downTheLeg", "fullToss", "inswing"),
        new BallType("fast", "downTheLeg", "fullToss", "outswing"),

    };


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DistanceBetweenWickets = Mathf.Abs(BatsmanStumps.transform.position.x - StartPoint.transform.position.x);
        _ballSpeed = DistanceBetweenWickets / BallTravelTime;
        _horizontalDistBtwnPitchPointAndBallerStumps = Mathf.Abs(targetPosition.x - BallerStumps.transform.position.x);
        _timeToPitch = (_horizontalDistBtwnPitchPointAndBallerStumps / DistanceBetweenWickets) * BallTravelTime;
        // StartCoroutine(SpawnBallWithDelay(SpawnDelay));

        lateralSwingDistance = CalculateInstantSwingDistance(_ballSpeed, _timeToPitch, maxSwingDistance);
        GameManager.Instance.lateralSwingDistance = lateralSwingDistance;

        UIManager.Instance.UpdateScore(GameManager.Instance.targetScore, GameManager.Instance.maxBallsToBall);

        StartCoroutine(HandleOvers());

        MainBall = FakeBall;
        //FakeBall.isKinematic = false;
        //Instantiate(BallPrefab, StartPoint.transform.position, Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        _ballSpeed = DistanceBetweenWickets / BallTravelTime;
        _horizontalDistBtwnPitchPointAndBallerStumps = Mathf.Abs(targetPosition.x - BallerStumps.transform.position.x);
        _timeToPitch = (_horizontalDistBtwnPitchPointAndBallerStumps / DistanceBetweenWickets) * BallTravelTime;

        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    StartCoroutine(SpawnBallWithDelay(SpawnDelay));
        //}
    }

    void FixedUpdate()
    {
        lateralSwingDistance = CalculateInstantSwingDistance(_ballSpeed, _timeToPitch, maxSwingDistance);
        GameManager.Instance.lateralSwingDistance = lateralSwingDistance;
    }

    public void InstantiateNewBall()
    {
        if (BallPrefab != null)
        {
            MainBall.transform.position = StartPoint.transform.position;
            _ball = MainBall; //Instantiate(BallPrefab, StartPoint.transform.position, Quaternion.identity);
            // _ball.tag = "currentBall";
            // GameManager.Instance.currentBall = _ball;
            GameManager.Instance.currentBall.SetActive(true);
            GameManager.Instance.rightHandTargetBall = _ball;

            //destroy ball after 12seconds
            //Destroy(_ball, 12f);
            _rb = _ball.GetComponent<Rigidbody>();
            _rb.isKinematic = false;
            verticalVelocity = (9.8f * _timeToPitch / 2.0f) - (StartPoint.transform.position.y / _timeToPitch);
            _rb.velocity = new Vector3(-(_ballSpeed), (9.8f * _timeToPitch / 2.0f) - (StartPoint.transform.position.y / _timeToPitch), (targetPosition.z - StartPoint.transform.position.z) / _timeToPitch);
            //_rb.AddTorque(Vector3.up * 10f);
        }
    }

    IEnumerator SpawnBallWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        InstantiateNewBall();
        //StartCoroutine(SpawnBallWithDelay(delay));
    }

    float CalculateInstantSwingDistance(float initialSpeed, float totalTime, float maxSwingDistance)
    {
        // Constants
        float mass = 0.16f; // Assuming mass = 1 kg for simplicity
        float maxLateralAcceleration = (swingForce * swingInfluence) / mass;

        // Calculate maximum distanceFactor (starts at 1 and decreases linearly)
        float averageDistanceFactor = 0.5f; // Linear decay assumption

        // Effective acceleration considering distance factor
        float effectiveAcceleration = maxLateralAcceleration * averageDistanceFactor;

        // Total lateral displacement (s = 0.5 * a * t^2)
        float swingDistance = 0.5f * effectiveAcceleration * Mathf.Pow(totalTime, 2);

        return swingDistance;
    }

    public void ThrowBall(BallType ball) // variation can be fast, inswing, leg spin, off spin 
    {
        Debug.Log($"Throwing ball: Speed={ball.BallSpeed}, Line={ball.BallLine}, Length={ball.BallLength}, Variation={ball.BallVariation}");

        //initialize variables
        BallTravelTime = pitchTimeDictionary[ball.BallSpeed];
        StartCoroutine(PlayBallerAnimation(1.8f));
        Debug.Log("Trying to reset baller position");
        StartCoroutine(UpdateScoreGUIWithDelay(3.0f, 0));
        StartCoroutine(ResetBallerPosition(10f));
        GameManager.Instance.triedHitting = false;
    }

    IEnumerator UpdateScoreGUIWithDelay(float delay, int runs)
    {
        yield return new WaitForSeconds(delay);
        if (!GameManager.Instance.triedHitting)
        {
            int maxBalls = GameManager.Instance.maxBallsToBall;
            int ballsBalled = GameManager.Instance.ballsBalled;
            int targetScore = GameManager.Instance.targetScore;
            GameManager.Instance.currentScore += runs;
            int currentScore = GameManager.Instance.currentScore;

            int required = targetScore - currentScore;
            int remainingBalls = maxBalls - ballsBalled;

            if (required < 0)
            {
                required = 0;
            }
            if (remainingBalls < 0)
            {
                remainingBalls = 0;
            }

            if (remainingBalls > 0)
            {
                GameManager.Instance.canThrowNextBall = true;
            }

            UIManager.Instance.UpdateScore(required, remainingBalls);
            if (checkGameLostCoroutine != null)
            {
                StopCoroutine(checkGameLostCoroutine);
            }
            checkGameLostCoroutine = StartCoroutine(CheckForGameLost(3f));
        }
    }

    IEnumerator CheckForGameLost(float delay)
    {
        yield return new WaitForSeconds(delay);
        Debug.Log("Checking for game lost");
        int currentScore = GameManager.Instance.currentScore;
        int maxBalls = GameManager.Instance.maxBallsToBall;
        int ballsBalled = GameManager.Instance.ballsBalled;
        int targetScore = GameManager.Instance.targetScore;
        if (currentScore < targetScore && ballsBalled == maxBalls)
        {
            GameManager.Instance.canThrowNextBall = false;
            GameManager.Instance.gameOver = true;
            UIManager.Instance.showLostScreen();
        }
    }

    IEnumerator HandleOvers()
    {
        while (GameManager.Instance.ballsBalled < GameManager.Instance.maxBallsToBall && !GameManager.Instance.gameOver)
        {
            yield return new WaitForSeconds(3);

            while (GameManager.Instance.canStartBalling == false) yield return null;
            while (GameManager.Instance.canThrowNextBall == false) yield return null;
            //wait till the baller is ready to throw
            Debug.Log("Trying to throw ball........");
            // while (!GameManager.Instance.canThrowNextBall) yield return null;
            Debug.Log("Throwing ball........");

            BallType[] currentBallTypes = currentOver % 2 == 0 ? ballTypes_2 : ballTypes_2;
            // new BallType[] { new BallType("fast", "atTheStumps", "yorker", "fast") };
            //
            // BallType[] balls = new BallType[] {
            //     new BallType("slow", "atTheStumps", "short", "fast"),
            //     //new BallType("fast", "atTheStumps", "goodLength", "fast"),
            //     //new BallType("slow", "atTheStumps", "short", "fast"),
            //     //new BallType("slow", "atTheStumps", "short", "inswing"),
            //     //new BallType("slow", "atTheStumps", "goodLength", "outswing")
            // };

            for (int i = 0; i < 6 && !GameManager.Instance.gameOver; i++)
            {
                while (GameManager.Instance.canThrowNextBall == false) yield return null;
                while (GameManager.Instance.canStartBalling == false) yield return null;
                yield return new WaitForSeconds(3f);
                Debug.Log("Throwing ball: " + i);
                int randomIndex = Random.Range(0, currentBallTypes.Length);

                BallType selectedBall = currentBallTypes[randomIndex];

                //calculate speed of the ball
                float DistanceBetweenWickets_ = Mathf.Abs(BatsmanStumps.transform.position.x - StartPoint.transform.position.x);
                float BallTravelTime_ = pitchTimeDictionary[selectedBall.BallSpeed];
                int _ballSpeed_ = (int)((22.12 / BallTravelTime_) * 3.6f);

                UIManager.Instance.updateUpcomingBallSpeed(_ballSpeed_);


                //BallType selectedBall = new BallType("fast", "outsideOff", "fullLength", "outSwing", new Dictionary<string, Vector3> { { "out", new Vector3(0, 0, 0) }, { "one", new Vector3(0, 0, 0) }, { "two", new Vector3(0, 0, 0) } }, new Vector3(-15f, 0f, 0.168f), "CoverDrive", 1f);


                //BallType selectedBall = new BallType("mediumPace", "outsideOff", "goodLength", "OutSwing", new Dictionary<string, Vector3> { { "out", new Vector3(0, 0, 0) }, { "one", new Vector3(0, 0, 0) }, { "two", new Vector3(0, 0, 0) } }, new Vector3(-15f, 0f, 0.79f));
                //player.transform.position = selectedBall.PlayerPosition;
                //new BallType("slow", "outsideOff", "short", "offSpin",
                //new Dictionary<string, Vector3> {
                //        { "out", new Vector3(0, 0, 0) }, { "one", new Vector3(0, 0, 0) }, { "two", new Vector3(0, 0, 0) }
                //});

                GameManager.Instance.currentBallType = selectedBall;
                //update target point
                targetPosition = new Vector3(targetPositionDictionary[selectedBall.BallLength], 0f, ballLineDictionary[selectedBall.BallLine]);

                //set swing force factor
                switch (selectedBall.BallLength)
                {
                    case "yorker": GameManager.Instance.swingForceFactor = 1f; break;
                    case "goodLength": GameManager.Instance.swingForceFactor = .7f; break;
                    case "fullLength": GameManager.Instance.swingForceFactor = .9f; break;
                    case "short": GameManager.Instance.swingForceFactor = .6f; break;
                    case "fullToss": GameManager.Instance.swingForceFactor = 0f; break;
                }

                switch (selectedBall.BallSpeed)
                {
                    case "fast": GameManager.Instance.swingForceFactor *= 1f; break;
                    case "slow": GameManager.Instance.swingForceFactor *= .7f; break;
                    case "mediumPace": GameManager.Instance.swingForceFactor *= .9f; break;
                }


                switch (selectedBall.BallVariation)
                {
                    case "inswing":
                        {
                            switch (selectedBall.BallLine)
                            {
                                case "atTheStumps": GameManager.Instance.swingInfluence = 0.2f; break;
                                case "outsideOff": GameManager.Instance.swingInfluence = -0.4f; break;
                                case "downTheLeg": GameManager.Instance.swingInfluence = 0.4f; break;
                            }
                            break;
                        }
                    case "outswing":
                        {
                            switch (selectedBall.BallLine)
                            {
                                case "atTheStumps": GameManager.Instance.swingInfluence = -0.2f; break;
                                case "outsideOff": GameManager.Instance.swingInfluence = 0.2f; break;
                                case "downTheLeg": GameManager.Instance.swingInfluence = 0f; break;
                            }
                            break;
                        }
                    case "legSpin":
                        {
                            switch (selectedBall.BallLine)
                            {
                                case "atTheStumps": GameManager.Instance.swingInfluence = -0.2f; break;
                                case "outsideOff": GameManager.Instance.swingInfluence = 0.2f; break;
                                case "downTheLeg": GameManager.Instance.swingInfluence = 0f; break;
                            }
                            break;
                        }
                    case "offSpin":
                        {
                            switch (selectedBall.BallLine)
                            {
                                case "atTheStumps": GameManager.Instance.swingInfluence = -0.2f; break;
                                case "outsideOff": GameManager.Instance.swingInfluence = 0.2f; break;
                                case "downTheLeg": GameManager.Instance.swingInfluence = 0f; break;
                            }
                            break;
                        }
                    case "fast": GameManager.Instance.swingInfluence = 0; break;


                }

                switch (GameManager.Instance.ballVariation)
                {
                    case "inswing": swingInfluence = -0.4f; break;
                    case "outswing": swingInfluence = 0.4f; break;
                    case "fast": swingInfluence = 0f; break;
                    case "legSpin": swingInfluence = -0.5f; break;
                    case "offSpin": swingInfluence = 0.5f; break;
                }


                //FakeBall.SetActive(true);

                yield return StartCoroutine(LerpTargetPoint());
                GameManager.Instance.startSlider = true;
                //wait for 2 seconds before baller starts proceeding with the action
                yield return new WaitForSeconds(2.0f);
                //FakeBall.SetActive(false);
                // ThrowBall(selectedBall);
                try
                {
                    Debug.Log("[HandleOvers] Calling ThrowBall now");
                    ThrowBall(selectedBall);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("[HandleOvers] Error in ThrowBall: " + ex.Message);
                }
                GameManager.Instance.canThrowNextBall = false;
                GameManager.Instance.ballsBalled++;
                yield return new WaitForSeconds(BallDelay);
            }
            currentOver++;
            Debug.Log("Current Over: " + currentOver);
            yield return new WaitForSeconds(OverDelay);

        }
    }
    public float LerpDuration = 0.4f; // Time to complete the lerp
    private float _elapsedTime = 0f;
    private bool _isLerping = false;

    //moves the pitch point indicator
    IEnumerator LerpTargetPoint()
    {
        _isLerping = true;
        _elapsedTime = 0f;

        Vector3 initialPosition = PitchPoint.transform.position;
        Vector3 finalPosition = new Vector3(targetPosition.x, 0.05f, targetPosition.z);

        while (_elapsedTime < LerpDuration)
        {
            // Interpolate the position
            float t = _elapsedTime / LerpDuration;
            PitchPoint.transform.position = Vector3.Lerp(initialPosition, finalPosition, t);

            _elapsedTime += 3 * Time.deltaTime; // Increment elapsed time
            yield return null; // Wait for the next frame
        }

        // Ensure the object ends exactly at the target position
        PitchPoint.transform.position = finalPosition;
        _isLerping = false;
    }
    public float _elapsedTimeToPlayBatsmanAnimation = 0;
    IEnumerator PlayBallerAnimation(float delay)
    {
        _anim.SetTrigger("Bowl");

        yield return new WaitForSeconds(delay);
        Debug.Log("Ball Instantiated");
        InstantiateNewBall();
        GameManager.Instance.canHit = true;
        yield return new WaitForSeconds(1.1f);
        GameManager.Instance.canHit = false;
    }
    IEnumerator ResetBallerPosition(float delay)
    {
        Debug.Log("Resetting baller position");
        yield return new WaitForSeconds(delay);
        Baller.transform.position = ballerInitialPosition;
    }
    public float MapInverse(float inputValue, float minValue, float maxValue)
    {
        // Ensure the input is within a reasonable range and prevent division by zero
        inputValue = Mathf.Clamp(inputValue, minValue, maxValue);

        // Apply the inverse function and map it to a value between 0 and 1
        float normalizedValue = 1f / (inputValue); // Inverse function
        float mappedValue = Mathf.InverseLerp(0, 1, normalizedValue); // Normalize to [0, 1]

        return mappedValue;
    }
}

public class BallType
{
    public string BallSpeed { get; set; }
    public string BallLine { get; set; }
    public string BallLength { get; set; }
    public string BallVariation { get; set; }

    // Constructor
    public BallType(string speed, string line, string length, string variation)
    {
        BallSpeed = speed;
        BallLine = line;
        BallLength = length;
        BallVariation = variation;
    }
}
