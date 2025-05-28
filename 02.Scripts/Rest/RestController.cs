using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class RestController : MonoBehaviour
{
    [SerializeField] private SerializableDic<Button, RestOptionBase> _restButtons;
    [SerializeField] private Button _exitButton;
    [SerializeField] private Image _panelImage;
    
    [Header("Title Text Option")]
    [SerializeField] private TextMeshProUGUI _titleText;
    [Range(30, 100)][SerializeField] private int _fontSize = 50;

    private SpriteAtlas _spriteAtlas;
    
    private async void Awake()
    {
        FadeIn();
        _spriteAtlas = await Manager.Instance.AddressableManager.Load<SpriteAtlas>("PopupUI");
        SetSpriteAtlas();
    }

    private void SetSpriteAtlas()
    {
        foreach (var data in _restButtons.dataList)
        {
            Button button = data.Key;
            button.GetComponent<Image>().sprite = _spriteAtlas.GetSprite("ConfirmButton");
        }
    }

    private void Start()
    {
        _exitButton.onClick.AddListener(FadeOutAndMoveToMapScene);
        _exitButton.gameObject.SetActive(false);
        
        foreach (var data in _restButtons.dataList)
        {
            Button button = data.Key;
            RestOptionBase option = data.Value;

            if (button == null)
            {
                Debug.LogError("버튼이 등록되지 않았습니다.");
                continue;
            }

            if (option == null)
            {
                option = button.GetComponent<RestOptionBase>();
                if(option == null)
                {
                    Debug.LogError("옵션이 등록되지 않았습니다.");
                    continue;
                }
            }
            
            // 버튼 클릭 이벤트 연결
            button.onClick.AddListener(() => 
            {
                // 모든 버튼 비활성화
                DisableAllButtons();
                
                // 옵션 적용, 적용 후 액션 등록
                option.ApplyOption(() => 
                {
                    _exitButton.gameObject.SetActive(true);
                    ChangeTitleText(option.TitleText);
                });
            });
        }
    }
    
    private void DisableAllButtons()
    {
        foreach (var data in _restButtons.dataList)
        {
            Button button = data.Key;
            if (button != null)
            {
                button.gameObject.SetActive(false);
            }
        }
    }

    private void ChangeTitleText(string text)
    {
        _titleText.text = text;
        _titleText.fontSize = _fontSize;

        _titleText.color = new Color(1, 1, 1, 0);
        _titleText.gameObject.SetActive(true);

        _titleText.DOFade(1, 1f).OnComplete(() =>
        {
            _titleText.DOFade(0, 1f).SetDelay(3f).OnComplete(() =>
            {
                _titleText.gameObject.SetActive(false);
            });
        });
    }

    private void FadeIn()
    {
        _panelImage.gameObject.SetActive(true);
        _panelImage.color = Color.black;

        _panelImage.DOFade(0f, 1f).OnComplete(() =>
        {
            _panelImage.gameObject.SetActive(false);
        });
    }

    private void FadeOutAndMoveToMapScene()
    {
        _panelImage.gameObject.SetActive(true);
        _panelImage.color = Color.clear;
        _panelImage.DOFade(1, 1f).OnComplete(() =>
        {
            SceneBase.GetCurrent<RestScene>().MoveToMapScene();
        });
    }
}
