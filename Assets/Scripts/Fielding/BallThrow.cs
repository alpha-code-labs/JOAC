using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallThrow : MonoBehaviour
{
    private Rigidbody _rb;

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();

        _rb.velocity = new Vector3(-7f, 20f, 12f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
