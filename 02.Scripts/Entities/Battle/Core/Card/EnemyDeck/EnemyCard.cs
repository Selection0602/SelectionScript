using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class EnemyCard : Card
{
    [SerializeField] private Image _backImage;

    private RectTransform _rectTransform;

    protected override void Awake()
    {
        base.Awake();
        _rectTransform = GetComponent<RectTransform>();
    }

    protected override void Set()
    {
        _rectTransform.localScale = Vector3.one;
        _rectTransform.localRotation = Quaternion.identity;
        _frontImage.sprite = _battleDataManager.EnemyImageData.CardFrontImage;
        _backImage.sprite = _battleDataManager.EnemyImageData.CardBackImage;
        base.Set();
    }

    public override async Task Use()
    {
        await PlayUseEffect();
    }

    private async Task PlayUseEffect()
    {
        Vector3 worldCenter = PositionUtility.ScreenCenterToUI(_rectTransform,Camera.main);
        _rectTransform.DOScale(5.0f, 0.5f);
        _rectTransform.DOAnchorPos(worldCenter,0.5f).SetEase(Ease.OutCubic);
        await UniTask.Delay(200);
        await _rectTransform.DORotate(new Vector3(0, 90, 0), 0.25f, RotateMode.LocalAxisAdd).SetEase(Ease.InOutQuad).AsyncWaitForCompletion();
        // 앞/뒷면 전환 (중간 지점에서)
        _front.SetActive(true);
        _back.SetActive(false);
        await _rectTransform.DORotate(new Vector3(0, -90, 0), 0.25f, RotateMode.LocalAxisAdd).SetEase(Ease.InOutQuad).AsyncWaitForCompletion();
        // 카드 이동 완료 후 대기
        await UniTask.Delay(200);
        await Dissolve(0.0f, 1.0f, 0.5f);
    }
}
