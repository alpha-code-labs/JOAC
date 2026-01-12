using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIPathDragger2 : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public RectTransform dragObject;  // UI element (circle)
    public RectTransform lineContainer; // Parent container for lines
    public GameObject linePrefab;  // Prefab for line segments

    private Vector2 startPos, defaultPos, currentPos;
    public float dragCoefficient = 0f;
    private List<GameObject> pathLines = new List<GameObject>();

    private const float LINE_SPACING = 25f; // Minimum distance between lines
    public Color startColor = Color.green;  // Start color of the line
    public Color endColor = Color.red;
    public string DirectionName;
    public float directionAngle;
    public float magnitude;
    public Vector2 DirectionVector;
    public Transform gizmosStartPoint;
    public Vector3 finalShotDirection;

    private float liveMagnitude;
    private Vector2 liveDirectionVector;

    public bool isPointerDownOrDragging = false;
    public Vector2 draggedPos;

    private bool isAtMaxDrag = false; // Tracks if max drag is reached
    private float jitterIntensity = 2f; // Adjust for stronger/softer shake
    private float vibrationCooldown = 0.2f; // Prevent constant vibration
    private float vibrationTimer = 0f;
    private float jitterOffset = 1.8f;

    private float maxDragDistance = 270f;

    public float speed = 2.0f;
    private float minX, maxX; // Movement boundaries for UI

    public bool limitToVertical = false;
    private List<Color> colorGradient = new List<Color>
    {
        new Color(0f, 1f, 0f),    // Green
        new Color(0.5f, 1f, 0f),  // Yellow-Green
        new Color(0.8f, 1f, 0f),
        new Color(1f, 1f, 0f),    // Yellow
        new Color(1f, .9f, 0f),
        new Color(1f, .6f, 0f),
        new Color(1f, 0.5f, 0f),  // Orange
        new Color(1f, 0.3f, 0f),
        new Color(1f, 0.1f, 0f),
        new Color(1f, 0f, 0f)     // Red
    };

    void Start()
    {
        startPos = dragObject.anchoredPosition;
        defaultPos = startPos;

        // Set min and max movement range (UI Y-Axis equivalent to Player Z-Axis)
        minX = startPos.x - 100f; // Adjust range based on UI scale
        maxX = startPos.x + 100f;
    }

    void Update()
    {
        float move = Input.GetAxis("Horizontal");
        float newX = (dragObject.anchoredPosition.x + move * speed * Time.deltaTime * 100f); // Scale movement for UI

        float move_X = UIManager.Instance.playerPositionSliderValue;
        newX = dragObject.anchoredPosition.x + move_X * speed * Time.deltaTime * 1000f;

        // Clamp movement within bounds
        newX = Mathf.Clamp(newX, minX, maxX);

        // Apply movement along UI's x-axis (equivalent to Player's z-axis movement)
        //stop updating when pointer is down

        if (!isPointerDownOrDragging)
        {
            dragObject.anchoredPosition = new Vector2(newX, dragObject.anchoredPosition.y);
            startPos = dragObject.anchoredPosition;
            defaultPos = startPos;
        }

        if (isPointerDownOrDragging && isAtMaxDrag)
        {
            // Compute drag vector
            Vector2 dragDirection = draggedPos - defaultPos;

            // Clamp drag direction to max allowed distance
            float maxDistance = maxDragDistance;
            if (dragDirection.magnitude > maxDistance)
            {
                dragDirection = dragDirection.normalized * maxDistance;
            }

            // Get normal (perpendicular) to clamped drag direction
            Vector2 normal = new Vector2(-dragDirection.y, dragDirection.x).normalized;

            // Apply jitter along normal direction
            Vector2 jitter = normal * Mathf.Sin(Time.time * 80f) * jitterOffset;

            // Final position = clamped drag + jitter
            dragObject.anchoredPosition = defaultPos + dragDirection + jitter;

            //// Handle vibration
            //if (vibrationTimer <= 0f)
            //{
            //    #if UNITY_ANDROID || UNITY_IOS
            //            Handheld.Vibrate();
            //    #endif
            //    vibrationTimer = vibrationCooldown;
            //}

            //vibrationTimer -= Time.deltaTime;
        }



    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("On Pointer down is called" + dragObject.anchoredPosition.x);
        ClearPath();
        dragCoefficient = 0f;
        startPos = dragObject.anchoredPosition;
        defaultPos = startPos;
        isPointerDownOrDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("onDrag is called");
        Vector2 newPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)dragObject.parent, eventData.position, eventData.pressEventCamera, out newPos);

        draggedPos = newPos;

        float maxDistance = maxDragDistance;
        Vector2 direction = newPos - defaultPos;
        if (direction.magnitude > maxDistance)
        {
            direction = direction.normalized * maxDistance;
            newPos = defaultPos + direction;
        }

        if (limitToVertical)
        {
            newPos.x = defaultPos.x;
        }

        Vector2 delta = newPos - dragObject.anchoredPosition;
        dragObject.anchoredPosition += delta;


        //dragObject.anchoredPosition = newPos;

        dragCoefficient = (newPos - defaultPos).magnitude;
        GameManager.Instance.dragCoefficient = dragCoefficient;
        isAtMaxDrag = (dragCoefficient >= maxDragDistance);

        UpdatePath(startPos, newPos);

        float score = dragCoefficient / maxDragDistance;
        GameManager.Instance.sliderScore = score;
        // Debug.Log("slider score..." + score);
        isPointerDownOrDragging = true;

    }

    public void OnPointerUp(PointerEventData eventData)
    {
        GetDragDirection(dragObject.anchoredPosition, startPos);
        ClearPath();
        GameManager.Instance.sliderScore = 0f;
        GameManager.Instance.pointerUp = true;

        // Debug.Log("slider score...0");
        isPointerDownOrDragging = false;
        isAtMaxDrag = false;
        vibrationTimer = 0f;
    }

    void UpdatePath(Vector2 from, Vector2 to)
    {
        float distance = Vector2.Distance(from, to);
        int requiredLines = Mathf.FloorToInt(distance / LINE_SPACING); // Compute required line segments

        if (dragCoefficient < maxDragDistance)
        {
            AdjustLineCount(requiredLines);
        }

        for (int i = 0; i < pathLines.Count; i++)
        {
            float t = (i + 1f) / (requiredLines + 1);
            Vector2 point = Vector2.Lerp(from, to, t);

            RectTransform lineRect = pathLines[i].GetComponent<RectTransform>();
            lineRect.anchoredPosition = point;

            if (i != 0)
                lineRect.sizeDelta = new Vector2(96f + i * 2f, 96 + i * i * .1f);
            else
                lineRect.sizeDelta = new Vector2(96f, 96f);

            float angle = Mathf.Atan2(to.y - from.y, to.x - from.x) * Mathf.Rad2Deg - 90;
            lineRect.rotation = Quaternion.Euler(0, 0, angle);

            Image lineImage = pathLines[i].GetComponent<Image>();
            lineImage.color = colorGradient[i];
        }
    }

    void AdjustLineCount(int requiredLines)
    {
        while (pathLines.Count > requiredLines)
        {
            Destroy(pathLines[pathLines.Count - 1]);
            pathLines.RemoveAt(pathLines.Count - 1);
        }

        while (pathLines.Count < requiredLines)
        {
            GameObject newLine = Instantiate(linePrefab, lineContainer);
            pathLines.Add(newLine);
        }
    }

    void ClearPath()
    {
        foreach (GameObject line in pathLines)
        {
            Destroy(line);
        }
        pathLines.Clear();
        dragObject.anchoredPosition = startPos;
    }

    void GetDragDirection(Vector2 startPoint, Vector2 endPoint)
    {
        // Calculate direction vector
        Vector2 direction = endPoint - startPoint;
        magnitude = GameManager.Instance.sliderScore;
        DirectionVector = direction.normalized;

        //proejceted vector on the x,z plane with magnitude acting as the lofting angle
        GameManager.Instance.ShotDirection = finalShotDirection = new Vector3(direction.y, GameManager.Instance.sliderScore * 3000, -direction.x).normalized;
        GameManager.Instance.lockedSliderScoreOnRelease = GameManager.Instance.sliderScore;

        // Get angle in degrees
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        directionAngle = angle;

        Debug.Log("Drag direction angle: " + angle + "�");
        // Determine direction based on angle
        if (angle > -15 && angle < 15)
        {
            Debug.Log("Square");
            DirectionName = "Square";
        }
        else if (angle > 15 && angle < 76)
        {
            Debug.Log("Cover Right");
            DirectionName = "Cover Right";
        }
        else if (angle > 75 && angle < 95)
        {
            Debug.Log("straight");
            DirectionName = "Straight";
        }
        else if (angle > 94 && angle < 150)
        {
            Debug.Log("Cover Left");
            DirectionName = "Cover Left";
        }
        else
        {
            Debug.Log("Left");
            DirectionName = "Left";
        }

        GameManager.Instance.ShotDirectionName = DirectionName;
    }


    // private void OnDrawGizmos()
    // {
    //     // Set the Gizmos color (you can change this to whatever color you want)
    //     Gizmos.color = Color.red;

    //     // Draw the vector starting from the position of the GameObject (this.transform.position)
    //     // The vector is drawn as a line from the position to the position + vector
    //     Gizmos.DrawLine(gizmosStartPoint.position, gizmosStartPoint.position + new Vector3(DirectionVector.y, dragCoefficient / 150f, -DirectionVector.x));

    //     // Optional: Draw a small sphere at the tip of the vector to make it more visible
    //     Gizmos.DrawSphere(gizmosStartPoint.position + new Vector3(DirectionVector.y, dragCoefficient / 150f, -DirectionVector.x), 0.1f);
    // }
}
