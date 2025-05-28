using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class RandomEventUI : MonoBehaviour
{
    [SerializeField] private Image _bgImage;
    [SerializeField] private TextMeshProUGUI _eventScriptText;
    [SerializeField] private TextMeshProUGUI _eventDescriptionText;
    
    [SerializeField] private Button _yesButton;
    [SerializeField] private TextMeshProUGUI _yesButtonText;
    [SerializeField] private Button _noButton;
    [SerializeField] private TextMeshProUGUI _noButtonText;

    private const string RETURN_TEXT = "돌아가기";
    
    [SerializeField] private Image _fadeImage;
    [SerializeField] private Image _bgFilterImage;

    private RandomEventBase _eventData;
    
    #region 아틀라스 세팅
    
    private SpriteAtlas _spriteAtlas;
    private const string ATLAS_LABEL = "PopupUI";
    private const string CONFIRM_BUTTON_SPRITE_NAME = "ConfirmButton";
    
    private async void Start()
    {
        _spriteAtlas = await Manager.Instance.AddressableManager.Load<SpriteAtlas>(ATLAS_LABEL);
        
        Initialize();
    }

    private void Initialize()
    {
        _fadeImage.color = Color.black;

        _eventScriptText.text = string.Empty;
        _eventScriptText.gameObject.SetActive(false);
        _yesButton.gameObject.SetActive(false);
        _noButton.gameObject.SetActive(false);

        _eventDescriptionText.text = string.Empty;
        _eventDescriptionText.gameObject.SetActive(false);

        _yesButton.GetComponent<Image>().sprite = _spriteAtlas.GetSprite(CONFIRM_BUTTON_SPRITE_NAME);
        _noButton.GetComponent<Image>().sprite = _spriteAtlas.GetSprite(CONFIRM_BUTTON_SPRITE_NAME);
    }
    #endregion

    #region UI 초기화 및 이벤트 설정
    /// <summary>
    /// 이벤트 UI 설정
    /// </summary>
    /// <param name="randomEvent">이벤트 데이터</param>
    public void SetEvent(RandomEventBase randomEvent)
    {
        if (!randomEvent) return;

        _eventData = randomEvent;
        SetUI();
    }

    private void SetUI()
    {
        _bgImage.sprite = _eventData.EventSprite;
        _eventScriptText.text = _eventData.EventText;
        _yesButtonText.text = _eventData.YesButtonText;
        _noButtonText.text = _eventData.NoButtonText;
    }

    private void SetButton()
    {
        _yesButton.gameObject.SetActive(true);
        _noButton.gameObject.SetActive(true);

        _yesButton.onClick.RemoveAllListeners();
        _noButton.onClick.RemoveAllListeners();

        // 이벤트의 필요 아이템을 가지고 있다면 Yes 버튼 활성화
        if (_eventData.HasRequiredBooty())
        {
            _yesButton.onClick.AddListener(() =>
            {
                _eventData.ExecuteEvent(); // 이벤트 실행
                // Yes 이벤트 텍스트 애니메이션 시작 및 완료 후 버튼 클릭 처리
                StartCoroutine(StartTextAnimation(_eventData.YesEventText, () => HandleButtonClick(_eventData.YesEventText, true)));

                _yesButton.gameObject.SetActive(false);
                _noButton.gameObject.SetActive(false);
            });
        }
        else
        {
            _yesButton.gameObject.SetActive(false);
        }
        
        _noButton.onClick.AddListener(() =>
        {
            StartCoroutine(StartTextAnimation(_eventData.NoEventText, () => HandleButtonClick(_eventData.NoEventText, false)));
            
            _yesButton.gameObject.SetActive(false);
            _noButton.gameObject.SetActive(false);
        });
    }
    #endregion

    #region 이벤트 완료 후 버튼 클릭 처리
    // 버튼 클릭 처리
    private void HandleButtonClick(string resultText, bool isYes)
    {
        _eventScriptText.text = resultText;
        _yesButton.gameObject.SetActive(false);
        
        // 랜덤 보상 이벤트라면 보상 선택 후 바로 맵 씬으로 이동
        if (_eventData is RandomBootyEvent or RandomMemoryEvent && isYes)
        {
            _noButton.gameObject.SetActive(false);
        }
        // No 버튼을 맵 씬으로 돌아가기 위한 버튼으로 변경
        else
        {
            _noButton.onClick.RemoveAllListeners();
            
            _noButton.gameObject.SetActive(true);
            _noButtonText.text = RETURN_TEXT;
            _noButton.onClick.AddListener(() => 
            {
                StartFade(false, 1f, () =>
                {
                    SceneBase.GetCurrent<RandomEventScene>()?.MoveMapScene();
                });
            });
        }
        
        // 전리품 교환 이벤트 혹은 전리품 손실 이벤트라면 설명 텍스트 활성화(얻은 효과 및 잃은 전리품 설명)
        if (_eventData is IBootyExchangeEvent exchangeEvent)
            ChangeDescription(exchangeEvent.Description);
    }
    #endregion

    #region 연출

    /// <summary>
    /// 페이드 인/아웃 시작
    /// </summary>
    /// <param name="isFadeIn">true = 페이드 인</param>
    /// <param name="duration">시간</param>
    /// <param name="onComplete"></param>
    public void StartFade(bool isFadeIn = true, float duration = 1f, Action onComplete = null)
    {
        _fadeImage.gameObject.SetActive(true);
        _fadeImage.color = isFadeIn ? Color.black : Color.clear;

        if(!isFadeIn)
            _fadeImage.transform.SetAsLastSibling();
        
        _fadeImage.DOFade(isFadeIn ? 0 : 1, duration).
            OnComplete(() =>
            {
                if(isFadeIn)
                {
                    // 페이드 인이라면 배경 필터 이미지 활성화 후 필터 이미지 페이드 아웃
                    _bgFilterImage.gameObject.SetActive(true);
                    _bgFilterImage.DOFade(0.5f, 0.5f).OnComplete(() =>
                    {
                        StartCoroutine(StartTextAnimation(_eventData.EventText, SetButton));
                    });
                    _fadeImage.gameObject.SetActive(false);
                }
                else
                {
                    onComplete?.Invoke();
                }
            });
    }

    // 한 글자 씩 텍스트 출력 코루틴
    private IEnumerator StartTextAnimation(string text, Action onComplete = null)
    {
        _eventScriptText.gameObject.SetActive(true);
        _eventScriptText.text = text;

        for(int i = 0; i < text.Length; i++)
        {
            _eventScriptText.text = text.Substring(0, i);
            yield return new WaitForSeconds(0.05f);
        }

        _eventScriptText.text = text;
        onComplete?.Invoke();
    }
    #endregion

    #region 설명 텍스트 설정
    private void ChangeDescription(string description)
    {
        _eventDescriptionText.color = new Color(1, 1, 1, 0);
        _eventDescriptionText.gameObject.SetActive(true);
        _eventDescriptionText.text = description;
        
        // 설명 텍스트 페이드 인 아웃
        _eventDescriptionText.DOFade(1, 1f).OnComplete(() =>
        {
            _eventDescriptionText.DOFade(0, 1f).SetDelay(3f).OnComplete(() =>
            {
                _eventDescriptionText.gameObject.SetActive(false);
            });
        });
    }
    #endregion
}