using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FielderMovement : MonoBehaviour
{
    
    public Animator _anim;
    public float speed=1;
    public bool shouldStand = false;
    public float delay;
    public Transform Target;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(MoveWithDelay());
        if (shouldStand)
            StartCoroutine(Stand());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator MoveWithDelay()
    {
        yield return new WaitForSeconds(delay);
        _anim.SetFloat("Speed", speed);
        _anim.SetTrigger("Gaze");

    }


    IEnumerator Stand()
    {
        yield return new WaitForSeconds(2f);
        _anim.SetTrigger("Stand");
    }

}
