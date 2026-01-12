using UnityEngine;
using UnityEngine.EventSystems;

public class Pointer : MonoBehaviour, IPointerUpHandler
{
    public void OnPointerUp(PointerEventData eventData)
    {
        // Logic when mouse is released
        Debug.Log("Slider value: " + GetComponent<UnityEngine.UI.Slider>().value);
        GameManager.Instance.pointerUp = true;
        GetComponent<UnityEngine.UI.Slider>().value = 0;
    }
}


