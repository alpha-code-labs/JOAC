using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeMove : MonoBehaviour
{
    public GameObject ball;
    public Rigidbody _rb;

    // Start is called before the first frame update
    void Start()
    {
        _rb = ball.GetComponent<Rigidbody>();
        _rb.velocity = new Vector3(4f, 2f, 3f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
