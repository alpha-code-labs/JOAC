using UnityEngine;

public class PlayerHorizontalMovement : MonoBehaviour
{
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        // Always set player position based on slider value
        float sliderValue = UIManager.Instance.playerPositionSliderValue;
        transform.position = new Vector3(transform.position.x, transform.position.y, startPosition.z - sliderValue);

        GameManager.Instance.playerTransformBeforeAnimation = transform;
    }
}
