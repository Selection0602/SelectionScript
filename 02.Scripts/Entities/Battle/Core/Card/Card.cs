using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class Card : MonoBehaviour
{
    [Header("뒷면")]
    [SerializeField] protected GameObject _back;

    [Header("앞면")]
    [SerializeField] private Sprite[] _frontImages;
    [SerializeField] protected Image _frontImage;
    [SerializeField] protected GameObject _front;
    [SerializeField] protected TextMeshProUGUI _cost;
    [SerializeField] protected TextMeshProUGUI _cardName;
    [SerializeField] protected Image _cardIcon;
    [SerializeField] protected TextMeshProUGUI _cardDesc;

    public CardData Data
    {
        get => _data;
        set
        {
            _data = value;
            Set();
        }
    }
    private CardData _data;

    public CardAnimation CardAnimation;
    protected BattleDataManager _battleDataManager;

    protected int _dissolveAmount = Shader.PropertyToID("_DissolveAmount");
    protected int _mainText = Shader.PropertyToID("_MainTex");

    protected virtual void Awake()
    {
        CardAnimation = GetComponent<CardAnimation>();
        _battleDataManager = (SceneBase.Current as BattleSceneController).BattleDataManager;
    }

    protected virtual void Set()
    {
        _cost.alpha = 1;
        _cardName.alpha = 1;
        _cardDesc.alpha = 1;
        _cost.gameObject.SetActive(true);
        _cardName.gameObject.SetActive(true);
        _cardDesc.gameObject.SetActive(true);

        _cost.text = $"{Data.Cost}";
        _cardName.text = $"{Data.CardName}";
        _cardIcon.sprite = Data.Image;
        _cardDesc.text = TextUtility.CleanLineBreaks(Data.Desc);

        _frontImage.material.SetTexture(_mainText, _frontImage.mainTexture);
        _cardIcon.material.SetTexture(_mainText, _cardIcon.mainTexture);

        _frontImage.material.SetFloat(_dissolveAmount, 0);
        _cardIcon.material.SetFloat(_dissolveAmount, 0);
    }

    public void UpdateCardDesc()
    {
        _cardDesc.text = TextUtility.CleanLineBreaks(Data.Desc);
    }

    public void SetMaterial(Material mat)
    {
        _frontImage.material = new Material(mat);
        _cardIcon.material = new Material(mat);
    }

    public void SetState(bool isOpen)
    {
        _back.gameObject.SetActive(!isOpen);
        _front.gameObject.SetActive(isOpen);
    }

    //카드 사용
    public abstract Task Use();

    //카드 사라지는 효과
    protected async Task Dissolve(float startValue, float endValue, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += 0.02f;
            float t = Mathf.Clamp01(elapsed / duration); // 진행 비율 계산
            float currentValue = Mathf.Lerp(startValue, endValue, t);

            // 머티리얼의 DissolveAmount 값 갱신
            _frontImage.material.SetFloat(_dissolveAmount, currentValue);
            _cardIcon.material.SetFloat(_dissolveAmount, currentValue);

            _cost.alpha = 1 - currentValue;
            _cardName.alpha = 1 - currentValue;
            _cardDesc.alpha = 1 - currentValue;

            // 다음 프레임까지 대기
            await UniTask.Delay((int)(0.02f * 500));
        }

        // 마지막 값 확정 (오차 방지)
        _frontImage.material.SetFloat(_dissolveAmount, endValue);
        _cardIcon.material.SetFloat(_dissolveAmount, endValue);
    }
}
