using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class Ball : MonoBehaviour
{
    private string _ballVariation;
    public float swingInfluence = 1f;
    public float swingForce = 2f;
    public float maxSwingDistance = 10f;
    public bool startApplyingSwing = false;
    private Vector3 pitchPointPosition;
    private Rigidbody _rb;
    private bool _ballPitched;
    private bool _GPressed = false;
    private bool _LPressed = false;
    public float sliderValue = 0;

    public BallType ballType;

    //animation related
    public Animator playerAnim;
    private float _animationTime = 1.46f;
    private float _actualAnimationTime = 1.46f;
    public float animationSpeed = 1;
    private string currentAnimation;


    //Animation time dictionary
    private Dictionary<string, float> animationTimeDict = new Dictionary<string, float>
    {
        { "PullShot", 1.46f},
        { "CoverDrive", 1.46f},
        { "SweepShot", 1.46f},
        { "StraightDrive", 1.46f}
    };

    void Start()
    {
        _ballPitched = false;
    }

    void Update()
    {
        //calculate distance of ball of the center of the ground

        float ballDistanceFromCenter = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(0f, 0f));

        GameManager.Instance.currentBallPosition = transform.position;


        GameObject player = GameManager.Instance.player;
        float side = Vector3.Dot(Vector3.right, (transform.position - player.transform.position));
        float dist = Vector3.Distance(transform.position, player.transform.position);
        GameManager.Instance.side = side;
        GameManager.Instance.dist = dist;

        if (side > 0 && !GameManager.Instance.isAnimationPlaying)
        {
            if (dist < 2.5f)
            {
                GameManager.Instance.rightHandPositionWeight = .1f / dist;
            }

            if (dist < 8f)
            {
                GameManager.Instance.lookAtWeight = Mathf.Clamp(0, 1, 1f / dist);
            }
        }
        else
        {
            GameManager.Instance.rightHandPositionWeight = 0f;
            GameManager.Instance.lookAtWeight = 0f;
        }

    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Pitch" && !_ballPitched)
        {
            _rb = transform.GetComponent<Rigidbody>();
            Debug.Log("ball pitched");
            _rb.AddForce(Vector3.up * GameManager.Instance.SpingStrength, ForceMode.Impulse);
            //Destroy(gameObject, 5.0f);
            pitchPointPosition = transform.position;
            //maxSwingDistance = Mathf.Abs(GameManager.Instance.stumps.transform.position.x - transform.position.x);
            startApplyingSwing = true;
            _ballPitched = true;
        }

    }
    void FixedUpdate()
    {
        //swing the ball after pitching as per the ball variation type
        if (startApplyingSwing)
        {
            ApplySwing();
        }
        swingInfluence = GameManager.Instance.swingInfluence;
    }

    void ApplySwing()
    {
        if (_rb != null)
        {
            float distanceTraveled = Vector3.Distance(pitchPointPosition, _rb.position);
            float distanceFactor = Mathf.Clamp01(1 - (distanceTraveled / maxSwingDistance));

            Vector3 velocity = _rb.velocity;
            Vector3 perpendicularForce = Vector3.Cross(velocity.normalized, Vector3.up) * GameManager.Instance.swingForce * GameManager.Instance.swingForceFactor * distanceFactor * swingInfluence;

            _rb.AddForce(perpendicularForce, ForceMode.Force);
        }
    }

}
