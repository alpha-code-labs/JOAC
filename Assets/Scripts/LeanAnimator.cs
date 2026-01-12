using UnityEngine;
using DG.Tweening;

public class LeanAnimator : MonoBehaviour
{
    public void ShowPanel(GameObject panel)
    {
        RectTransform panelTransform = panel.GetComponent<RectTransform>();
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();

        if (panelTransform == null || canvasGroup == null)
        {
            Debug.LogWarning("Panel is missing RectTransform or CanvasGroup!");
            return;
        }

        panelTransform.localScale = Vector3.zero;
        canvasGroup.alpha = 0f;
        panel.SetActive(true);

        Sequence seq = DOTween.Sequence();
        seq.Append(panelTransform.DOScale(1f, 0.5f).SetEase(Ease.OutBack).SetUpdate(true));
        seq.Join(canvasGroup.DOFade(1f, 0.5f).SetUpdate(true));
    }

    public void HidePanel(GameObject panel)
    {
        RectTransform panelTransform = panel.GetComponent<RectTransform>();
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();

        if (panelTransform == null || canvasGroup == null)
        {
            Debug.LogWarning("Panel is missing RectTransform or CanvasGroup!");
            return;
        }

        Sequence seq = DOTween.Sequence();
        seq.Append(panelTransform.DOScale(0f, 0.4f).SetEase(Ease.InBack).SetUpdate(true));
        seq.Join(canvasGroup.DOFade(0f, 0.4f).SetUpdate(true));
        seq.OnComplete(() => panel.SetActive(false));
    }
}