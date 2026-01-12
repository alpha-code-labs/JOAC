using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIAgentController : MonoBehaviour
{
    public GameObject Ball;
    public NavMeshAgent _agent;
    private bool playSlowRunningAnim;
    private bool playFastRunningAnim;
    public Animator _anim;
    public float DistanceToBall;
    public float agentSpeed;
    public GameObject AgentRightHand;
    public bool ballPicked = false;
    public bool ballThrown = false;
    private float slowDownTriggerRange = 14f;
    private float ballPickupTriggerRange = 1.8f;
    public Transform ballerStumps;
    public float throwForce = 20f;
    private Rigidbody ballRb;
    private bool boolIsCatchingBall = false;
    private float minDistance = 20f;
   


    // Start is called before the first frame update
    void Start()
    {
        ballRb = Ball.transform.GetComponent<Rigidbody>();
        _anim = GetComponent<Animator>();
        StartCoroutine(DelayedTargetUpdate());
    }

    public void updateTarget(GameObject target)
    {

        _agent = GetComponent<NavMeshAgent>();
        _agent.speed = agentSpeed;
        _agent.updateRotation = true;
        _agent.SetDestination(Ball.transform.position);
        _anim = GetComponent<Animator>();
        ballRb = Ball.transform.GetComponent<Rigidbody>();
        minDistance = 100f;
    }


    // Update is called once per frame
    void Update()
    {
        if (Ball != null) {
            float dist = Vector3.Distance(AgentRightHand.transform.position, Ball.transform.position);

            if (dist < minDistance)
            {

                _agent.SetDestination(Ball.transform.position);
                DistanceToBall = dist;
                Debug.Log(_anim.GetBool("RunningFast") + "running slow");

                if (dist > slowDownTriggerRange && !ballPicked)
                {
                    _anim.SetFloat("Speed", .95f);
                }

                else if (dist < slowDownTriggerRange)
                {

                    float speedFactor = Mathf.Sin((Mathf.PI / 2) * (dist / slowDownTriggerRange));

                    _anim.SetFloat("Speed", speedFactor);
                    //_anim.SetBool("RunningFast", false);

                    if (dist < ballPickupTriggerRange)
                        if (!ballPicked)
                        {
                            _anim.SetTrigger("Grab");
                            ballPicked = true;
                            _agent.isStopped = true;
                        }
                    if (ballPicked && !ballThrown)
                    {
                        _anim.SetTrigger("Throw");
                        Vector3 throwDirection = (ballerStumps.position - Ball.transform.position).normalized;
                        Vector3 playerRotationDirection = (transform.position - ballerStumps.position).normalized;
                        //rotate player in right direction

                        Quaternion targetRotation = Quaternion.LookRotation(playerRotationDirection);
                        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f); // Smooth rotation
                        StartCoroutine(WaitAndThrow(2f, throwDirection));
                        ballThrown = true;
                    }
                }

            }
            else
            {
                _agent.isStopped = true;
            }
        }

        //else _agent.isStopped = true;
    }


    IEnumerator WaitAndThrow(float waitTime, Vector3 throwDirection) {
        yield return new WaitForSeconds(1f);

        //_agent.isStopped = false;
        //_agent.SetDestination(ballerStumps.position);
        
        yield return new WaitForSeconds(waitTime);

        ballRb.isKinematic = false;
        ballRb.useGravity = true;
        ballRb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
        ballThrown = true;
    }

    void OnDrawGizmos()
    {
        if(Ball != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, Ball.transform.position);  // Show AI�s path to the ball
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, slowDownTriggerRange);  // Show slow down range
            Gizmos.DrawWireSphere(transform.position, ballPickupTriggerRange);  // Show pickup range
        }
        
    }

    IEnumerator DelayedTargetUpdate()
    {
        yield return new WaitForSeconds(11f);
        updateTarget(Ball);
    }
}
