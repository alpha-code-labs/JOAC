using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class BatPullIK : MonoBehaviour
{
    public Animator animator; // Reference to the Animator component
    public Transform batTransform; // Reference to the bat's transform
    public Transform rightHandIKTarget; // Target for the right hand IK

    public Transform targetObject;

    private Vector3 initialHandPosition; // Initial hand position
    private Quaternion initialHandRotation; // Initial hand rotation
    public Vector3 targetRotation;
    public Vector3 rh_eulerAngles;

    void Start()
    {
        if (rightHandIKTarget != null)
        {
            // Store the initial position and rotation of the hand
            initialHandPosition = rightHandIKTarget.position;
            initialHandRotation = rightHandIKTarget.rotation;
        }
        rh_eulerAngles = rightHandIKTarget.transform.eulerAngles;
        // Debug.Log(rightHandIKTarget.transform.eulerAngles + " right hand euler angles...");
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (animator == null || rightHandIKTarget == null || batTransform == null)
            return;

        // Enable IK for the right hand
        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, GameManager.Instance.rightHandPositionWeight);
        animator.SetIKRotationWeight(AvatarIKGoal.RightHand, GameManager.Instance.sliderScore);

        // Calculate the IK target position and rotation based on the bat's position
        //Vector3 pullDirection = targetObject.position; //- initialHandPosition; // Direction of the pull
        ///pullDirection = new Vector3(pullDirection.z, 0f, 0f);

        // Get the current rotation of the right hand IK target
        rh_eulerAngles = new Vector3(200.634918f, 90.891304f, 100.962204f);
        // Convert the Euler angles back to a Quaternion
        Quaternion targetRotation = Quaternion.Euler(rh_eulerAngles);




        // Set the IK target position and rotation
        GameObject targetBall = GameManager.Instance.rightHandTargetBall;

        //isPositive = targetBall.transform.position.z > 


        animator.SetIKPosition(AvatarIKGoal.RightHand, targetBall.transform.position);
        animator.SetIKRotation(AvatarIKGoal.RightHand, targetRotation);


        //animator.SetLookAtWeight(GameManager.Instance.lookAtWeight);
        //animator.SetLookAtPosition(targetBall.transform.position);
    }
}
