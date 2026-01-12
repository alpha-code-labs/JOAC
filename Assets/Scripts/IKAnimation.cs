using System;
using UnityEngine;
using System.Collections;

public class IKAnimation : MonoBehaviour
{
    public Transform playerTransform;
    public Animator animator;
    public Transform ballTransform;
    public Transform playerHandTransform; // Add reference to the hand transform
    public float rightHandWeight;
    public float rightHandRotationWeight;

    public float sensitivity = 100f;

    private Vector2 mouseInputs;
    private Vector2 MouseInputs
    {
        get
        {
            return mouseInputs;
        }
    }

    void Start()
    {
    }

    void Update()
    {
        float dist = Vector3.Distance(ballTransform.position, playerHandTransform.position);

        if (dist < 4f)
        {
            StartCoroutine(AdjustHandWeight(.8f));
        }
        else
        {
            StartCoroutine(AdjustHandWeight(0f));
        }

        if (dist < 10f)
        {
            StartCoroutine(AdjustHandRotationWeight(.61f));
        }
        else
        {
            StartCoroutine(AdjustHandRotationWeight(0f));
        }
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (animator)
        {
            // Calculate rotation vector
            Vector3 difference = ballTransform.position - playerHandTransform.position;
            Vector3 perpendicular = Vector3.Cross(difference.normalized, Vector3.right);
            Quaternion rotation = Quaternion.LookRotation(perpendicular);

            // Set IK weights
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, rightHandWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, rightHandRotationWeight);

            // Set IK position and rotation
            animator.SetIKPosition(AvatarIKGoal.RightHand, ballTransform.position);
            animator.SetIKRotation(AvatarIKGoal.RightHand, rotation);
        }
    }

    IEnumerator AdjustHandWeight(float targetWeight)
    {
        while (Mathf.Abs(rightHandWeight - targetWeight) > 0.01f)
        {
            rightHandWeight = Mathf.Lerp(rightHandWeight, targetWeight, Time.deltaTime * .5f);
            yield return null;
        }
        rightHandWeight = targetWeight; // Snap to the final value
    }

    IEnumerator AdjustHandRotationWeight(float targetWeight)
    {
        while (Mathf.Abs(rightHandRotationWeight - targetWeight) > 0.01f)
        {
            rightHandRotationWeight = Mathf.Lerp(rightHandRotationWeight, targetWeight, Time.deltaTime * 1f);
            yield return null;
        }
        rightHandRotationWeight = targetWeight; // Snap to the final value
    }
}
