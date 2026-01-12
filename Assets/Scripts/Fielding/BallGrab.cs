using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallGrab : MonoBehaviour
{
    public Transform playerHand;
    public List<GameObject> collidedObjects = new List<GameObject>();
    private Rigidbody _rb;



    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision collision){

        GameObject collidedObject = collision.gameObject;

        if (!collidedObjects.Contains(collidedObject))
        {
            collidedObjects.Add(collidedObject);
            Debug.Log("Collided with: " + collidedObject.name);
        }


        Debug.Log("Collision occured..." + collision.gameObject.name);
        if (collision.gameObject.tag == "Hand")
        {

            transform.SetParent(playerHand);
            _rb.velocity = Vector3.zero;
            _rb.isKinematic = true;
            transform.localPosition = new Vector3(-0.00499999989f, 0.075000003f, 0.074000001f);
            Debug.Log("Grabbed ball");
        }
    }
}
