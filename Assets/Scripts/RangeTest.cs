using UnityEngine;

public class CheckObjectInRange : MonoBehaviour
{
    public GameObject targetObject;
    public float PerfectRange = 1f;
    public float VeryGoodRange = 2f;
    public float GoodRange = 3f;
    public float EarlyRange = 5f;
    public float BadRange = 7f;
    public Vector3 rangeOffset = Vector3.zero;
    public float detectionHeight = 3f;
    public float detectionWidth = 4f;

    public Animator animator;

    [Header("TouchAndDrag")]
    private Vector2 startPos;
    private bool isDragging = false;

    [Header("Drag Distance Thresholds")]
    public float lowThreshold = 100f;
    public float mediumThreshold = 300f;

    [Header("GameObjects for Drag Categories")]
    public GameObject redObject;
    public GameObject greenObject;
    public GameObject blueObject;
    private string dragCategory = "None";

    void Update()
    {
        checkRange();

        if (Input.GetMouseButtonDown(0))
        {
            startPos = Input.mousePosition;
            isDragging = true;
            dragCategory = "None";
            Debug.Log("Screen Pressed at: " + startPos);
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            Vector2 currentPos = Input.mousePosition;
            float dragDistance = Vector2.Distance(startPos, currentPos);

            if (dragDistance < lowThreshold)
                dragCategory = "Low";
            else if (dragDistance < mediumThreshold)
                dragCategory = "Medium";
            else
                dragCategory = "High";

            SetActiveObject();
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            Vector2 endPos = Input.mousePosition;
            Vector2 dragDirection = endPos - startPos;
            string dragSide = dragDirection.x < 0 ? "LEFT" : "RIGHT";

            Debug.Log($"Screen Released. Final Drag Category: {dragCategory}, Drag Side: {dragSide}");

            if (IsTargetWithinGizmoRange())
            {
                PlayAnimation(dragSide);
            }
            else
            {
                Debug.Log("Target is out of range. Animation not played.");
            }

            ResetObjects();
        }
    }

    private bool IsTargetWithinGizmoRange()
    {
        if (targetObject == null)
        {
            Debug.LogWarning("No target object assigned!");
            return false;
        }

        Vector3 adjustedPosition = transform.position + rangeOffset;
        Vector3 targetPos = targetObject.transform.position;

        float distanceX = Mathf.Abs(targetPos.x - adjustedPosition.x); // Left/right distance
        float distanceZ = Mathf.Abs(targetPos.z - adjustedPosition.z); // Forward/backward distance

        // Check if the object is within the fixed detection length
        if (distanceZ > detectionWidth / 2)
        {
            Debug.Log("Target is OUT of detection length.");
            return false;
        }

        // Check which range the target is in
        if (distanceX <= PerfectRange)
        {
            Debug.Log($"<color=green>{targetObject.name} is in PERFECT RANGE.</color>");
            return true;
        }
        else if (distanceX <= VeryGoodRange)
        {
            Debug.Log($"<color=yellow>{targetObject.name} is in VERY GOOD RANGE.</color>");
            return true;
        }
        else if (distanceX <= GoodRange)
        {
            Debug.Log($"<color=blue>{targetObject.name} is in GOOD RANGE.</color>");
            return true;
        }
        else if (distanceX <= EarlyRange)
        {
            Debug.Log($"<color=orange>{targetObject.name} is in EARLY RANGE.</color>");
            return true;
        }
        else if (distanceX <= BadRange)
        {
            Debug.Log($"<color=red>{targetObject.name} is in BAD RANGE.</color>");
            return true;
        }

        Debug.Log($"{targetObject.name} is OUT of all detection ranges.");
        return false;
    }



    private void SetActiveObject()
    {
        redObject.SetActive(dragCategory == "High");
        blueObject.SetActive(dragCategory == "Medium");
        greenObject.SetActive(dragCategory == "Low");
    }

    private void ResetObjects()
    {
        redObject.SetActive(false);
        blueObject.SetActive(false);
        greenObject.SetActive(false);
    }

    void checkRange()
    {
        if (targetObject == null)
        {
            Debug.LogWarning("No target object assigned!");
            return;
        }

        //Vector3 adjustedPosition = transform.position + rangeOffset;
        //float distance = Vector3.Distance(new Vector3(adjustedPosition.x, 0, adjustedPosition.z),
        //                                new Vector3(targetObject.transform.position.x, 0, targetObject.transform.position.z));

        //string positionSide = GetSide(targetObject.transform.position);

        //float closeRange = PerfectRange + rangeOffset.magnitude;
        //float mediumRange = VeryGoodRange + rangeOffset.magnitude;
        //float farRange = GoodRange + rangeOffset.magnitude;
        //float YellowRange = EarlyRange + rangeOffset.magnitude;
        //float RedRange = BadRange + rangeOffset.magnitude;

        //if (distance <= closeRange)
        //{
        //   Debug.Log(FormatMessage(targetObject.name, "VERY CLOSE", positionSide));
        //}
        //else if (distance <= mediumRange)
        //{
        //  Debug.Log(FormatMessage(targetObject.name, "MEDIUM RANGE", positionSide));
        //}
        //else if (distance <= farRange)
        //{
        //  Debug.Log(FormatMessage(targetObject.name, "FAR RANGE", positionSide));
        //}
    }

    void PlayAnimation(string side)
    {
        if (animator == null)
        {
            Debug.LogWarning("No Animator assigned!");
            return;
        }

        animator.ResetTrigger("Left");
        animator.ResetTrigger("Right");

        string triggerName = (side == "LEFT") ? "Left" : "Right";
        animator.SetTrigger(triggerName);
    }

    string GetSide(Vector3 targetPosition)
    {
        Vector3 directionToTarget = (targetPosition - transform.position).normalized;
        float dotProduct = Vector3.Dot(transform.right, directionToTarget);

        if (dotProduct < 0) return "LEFT";
        if (dotProduct > 0) return "RIGHT";
        return "CENTER";
    }

    string FormatMessage(string objectName, string range, string side)
    {
        string color = range == "VERY CLOSE" ? "red" :
                       range == "MEDIUM RANGE" ? "yellow" : "green";

        return $"<color={color}>{objectName} is in {range} on the {side} side!</color>";
    }

    void OnDrawGizmosSelected()
    {
        Vector3 adjustedPosition = transform.position + rangeOffset;

        // Define colors for different range zones
        Color[] colors = { Color.green, Color.yellow, Color.blue, new Color(1f, 0.5f, 0f), Color.red };
        float[] ranges = { PerfectRange, VeryGoodRange, GoodRange, EarlyRange, BadRange };

        // Draw all range zones with the same length but varying width
        for (int i = 0; i < ranges.Length; i++)
        {
            DrawWireCube(adjustedPosition, ranges[i], colors[i]);
        }
    }

    void DrawWireCube(Vector3 center, float width, Color color)
    {
        Gizmos.color = color;

        // Keep a constant length while adjusting width based on range
        Vector3 cubeSize = new Vector3(width * 2, detectionHeight, detectionWidth); // Fixed depth

        Gizmos.DrawWireCube(center, cubeSize);
    }

}



