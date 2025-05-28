using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightFlicker : MonoBehaviour
{
    [Header("조명 밝기")]
    [SerializeField] private float _maxIntensity; //최대 밝기
    [SerializeField] private float _minIntensity; //최소 밝기

    [Header("깜빡거리는 주기")]
    [SerializeField] private float _duration; //깜빡거리는 주기

    [SerializeField]  private Light2D _light2D;

    public void OnEnable()
    {
        StartFlickerEffect();
    }

    public void StartFlickerEffect()
    {
        _light2D.intensity = _minIntensity;
        DOTween.To(() => _light2D.intensity,
                   x => _light2D.intensity = x,
                   _maxIntensity,
                   _duration)
               .From(_minIntensity)
               .SetLoops(-1, LoopType.Yoyo)
               .SetEase(Ease.InOutSine);
    }
}
