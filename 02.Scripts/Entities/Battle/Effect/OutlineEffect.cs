using DG.Tweening;
using UnityEngine;

public class OutlineEffect : MonoBehaviour
{
    [SerializeField] private UIOutline _uiOutline;

    private void OnEnable()
    {
        BlinkOutlineAlpha();
    }

    private void BlinkOutlineAlpha()
    {
        Color color = _uiOutline.color;
        color.a = 0.3f;
        _uiOutline.color = color;

        float duration = 1.0f;
        DOTween.To(
            () => _uiOutline.color.a,
            a => {
                Color c = _uiOutline.color;
                c.a = a;
                _uiOutline.color = c;
            },
            1f,
            duration
        ).From(color.a)
        .SetLoops(-1, LoopType.Yoyo)
        .SetEase(Ease.InOutSine);
    }
}
