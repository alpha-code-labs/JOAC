using DG.Tweening;
using UnityEngine;

public class MainMenuAnim : MonoBehaviour
{
    public RectTransform ContinueButton;
    // Start is called before the first frame update
    void Start()
    {
        ContinueButton.DOScale(Vector3.one * .75f, 0.5f).SetEase(Ease.OutBack);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
