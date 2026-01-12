using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class HitBall : MonoBehaviour
{
    private Animator _anim;
    private Transform BallPositionAtHitTime;
    public GameObject Ball;
    public float diff;
    public AudioSource audioSource;
    private bool audiPlayed = false;

    // Start is called before the first frame update
    void Start()
    {
        _anim = GetComponent<Animator>();
        if (_anim == null)
        {
            Debug.Log("Small boy's Animator is null");
        }
        disableCollider();
        UIManagerGamePlay_1.Instance.UpdateScore(GameManager.Instance.ballsHit, GameManager.Instance.maxBallsToBall - GameManager.Instance.ballsBalled);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.pointerUp)
        {
            GameManager.Instance.pointerUp = false;
            _anim.SetTrigger("sweep");
            BallPositionAtHitTime = Ball.transform;

            if (GetTimingOfShot() < 4f)
            {
                if (GetTimingOfShot() > 1f)
                {
                    if (GetTimingOfShot() > 2f)
                    {
                        UIManagerGamePlay_1.Instance.ShowFlashMessage("Perfect");
                        StartCoroutine(DelayedMoveBall(.6f));
                        enableCollider();
                    }
                    else
                    {
                        UIManagerGamePlay_1.Instance.ShowFlashMessage("Good");
                        StartCoroutine(DelayedMoveBall(.8f));
                        enableCollider();
                    }

                }
                else
                {
                    disableCollider();
                    UIManagerGamePlay_1.Instance.ShowFlashMessage("Late");
                }
            }
            else
            {
                //show early shot message on ui
                UIManagerGamePlay_1.Instance.ShowFlashMessage("Early");
                //disable the collider
                disableCollider();
            }
        }
    }

    void enableCollider()
    {
        GetComponent<Collider>().enabled = false;
    }

    void disableCollider()
    {
        GetComponent<Collider>().enabled = false;
    }


    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Ball")
        {
            MoveBall();
            GameManager.Instance.ballsHit += 1;
            UIManagerGamePlay_1.Instance.UpdateScore(GameManager.Instance.ballsHit, GameManager.Instance.maxBallsToBall - GameManager.Instance.ballsBalled);
            Debug.Log("collided witht the ball");
        }
    }

    float GetTimingOfShot()
    {
        float diffX = transform.position.x - Ball.transform.position.x;
        diff = diffX;
        return diffX;
    }

    void MoveBall()
    {
        Rigidbody _rb = Ball.transform.GetComponent<Rigidbody>();
        _rb.velocity = Vector3.zero;

        _rb.velocity = new Vector3(-12f, 1f, 1f);
        GameManager.Instance.ballsHit += 1;
        UIManagerGamePlay_1.Instance.UpdateScore(GameManager.Instance.ballsHit, GameManager.Instance.maxBallsToBall - GameManager.Instance.ballsBalled);
    }

    IEnumerator DelayAudio()
    {
        yield return new WaitForSeconds(5f);
        audiPlayed = false;
    }

    IEnumerator DelayedMoveBall(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (audiPlayed == false)
        {
            audioSource.Play();
            audiPlayed = true;
            StartCoroutine(DelayAudio());
        }
        MoveBall();
    }
}
