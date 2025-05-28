using System;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

// 임시 확인 팝업
public class ConfirmPopUp : MonoBehaviour, IPopUp
{
    [SerializeField] private Image _background;
    [SerializeField] private Button _leftButton;
    [SerializeField] private Button _rightButton;
    [SerializeField] private Button _closeButton;

    private SpriteAtlas _spriteAtlas;

    public event Action OnClose;

    #region 어드레서블, 아틀라스 라벨
    private const string ATLAS_LABEL = "PopupUI";
    private const string X_SPRITE_NAME = "X";
    private const string CONFIRM_BUTTON_SPRITE_NAME = "ConfirmButton";
    private const string CONFIRM_POPUP_SPRITE_NAME = "ConfirmPopup";
    #endregion

    #region 버튼 이벤트 등록 및 아틀라스 설정
    private async void Awake()
    {
        _leftButton.onClick.AddListener(OnLeftButtonClick);
        _rightButton.onClick.AddListener(OnRightButtonClick);
        _closeButton.onClick.AddListener(() =>
        {
            Manager.Instance.SoundManager.PlaySFX(SFXType.SFX_Click);
            OnClose?.Invoke();
        });

        _spriteAtlas = await Manager.Instance.AddressableManager.Load<SpriteAtlas>(ATLAS_LABEL);

        if (_spriteAtlas)
            SetSpriteAtlas();
    }

    private void SetSpriteAtlas()
    {
        Image closeButtonImage = _closeButton.GetComponent<Image>();
        Image leftButtonImage = _leftButton.GetComponent<Image>();
        Image rightButtonImage = _rightButton.GetComponent<Image>();

        _background.sprite = _spriteAtlas.GetSprite(CONFIRM_POPUP_SPRITE_NAME);

        closeButtonImage.sprite = _spriteAtlas.GetSprite(X_SPRITE_NAME);

        Sprite buttonSprite = _spriteAtlas.GetSprite(CONFIRM_BUTTON_SPRITE_NAME);
        leftButtonImage.sprite = buttonSprite;
        rightButtonImage.sprite = buttonSprite;
    }
    #endregion

    #region 버튼 클릭 이벤트
    private void OnLeftButtonClick()
    {
        Manager.Instance.SoundManager.PlaySFX(SFXType.SFX_Click);

        MapScene mapScene = SceneBase.Current as MapScene;

        if(mapScene)
            mapScene.FadeOut(() => { mapScene.ReturnStartScene(); });
    }

    private void OnRightButtonClick()
    {
        Manager.Instance.SoundManager.PlaySFX(SFXType.SFX_Click);
        Manager.Instance.MapManager.ClearData();
        Manager.Instance.SaveManager.ClearAllData();

        MapScene mapScene = SceneBase.Current as MapScene;

        if(mapScene)
            mapScene.FadeOut(() => { mapScene.ReturnStartScene(); });
    }
    #endregion
}
