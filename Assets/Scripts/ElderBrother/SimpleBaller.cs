using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleBaller : MonoBehaviour
{
    public GameObject BallPrefab;
    public Transform handTransform;
    public Rigidbody Ball_rb;
    public Transform BallSpawnPosition;
    public float BallSpeed=5f;
    private float ballerAnimationTime = 1.1f;
    public int totalBalls = 5;


    private Animator _anim;
    private float delayBetweenBalls = 5f;
    private float delayforFirstBall = 3f;
    private bool throwingFirstBall = true;
    private int ballsBalled  = 0;

    // Start is called before the first frame update
    void Start()
    {
        
        BallPrefab.transform.position = handTransform.position + new Vector3(0.0149999997f, 0.0829999968f, 0.0390000008f);
        Ball_rb = BallPrefab.gameObject.GetComponent<Rigidbody>();
        Ball_rb.isKinematic = true;
        _anim = GetComponent<Animator>();

        //strt throwing balls
        StartCoroutine(ThrowBallsSequentially());
    }

    void ResetBallPosition()
    {
        BallPrefab.transform.parent = handTransform;
        BallPrefab.transform.localPosition = new Vector3(0.0149999997f, 0.0829999968f, 0.0390000008f);
        Ball_rb.isKinematic = true;
    }

    void ThrowBall()
    {
        //GameObject.Find("Player").GetComponent<Collider>().enabled = true;
        Ball_rb.isKinematic = false;
        BallPrefab.transform.position = BallSpawnPosition.transform.position;
        Ball_rb.velocity = new Vector3(BallSpeed, 0f, 0f);
        GameManager.Instance.ballsBalled += 1;
        UIManagerGamePlay_1.Instance.UpdateScore(GameManager.Instance.ballsHit, GameManager.Instance.maxBallsToBall - GameManager.Instance.ballsBalled);
    }

    IEnumerator ThrowAfterAnimationDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ThrowBall();
    }

    IEnumerator ThrowBallAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        _anim.SetTrigger("roll");
        StartCoroutine(ThrowAfterAnimationDelay(ballerAnimationTime));
    }


    IEnumerator ThrowBallsSequentially()
    {
        while (GameManager.Instance.canStartBalling == false) yield return null;

        yield return new WaitForSeconds(delayforFirstBall); // optional initial delay
        
        while (ballsBalled < GameManager.Instance.maxBallsToBall)
        {
            _anim.SetTrigger("roll");
            yield return new WaitForSeconds(ballerAnimationTime); // wait for animation
            ThrowBall();
            ballsBalled++;

            yield return new WaitForSeconds(delayBetweenBalls); // wait before next
        }
    }

}
