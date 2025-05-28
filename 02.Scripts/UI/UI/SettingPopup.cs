using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;
using UnityEngine.UI;

public class SettingPopup : MonoBehaviour, IPopUp
{
    [SerializeField] private Slider _masterVolumeSlider;
    [SerializeField] private Slider _bgmVolumeSlider;
    [SerializeField] private Slider _sfxVolumeSlider;

    [SerializeField] private Button _closeButton;
    [SerializeField] private Button _exitButton;
    [SerializeField] private Button _tutorialButton;

    public event Action OnClose;
    private SoundManager _soundManager;

    private string _tutorialCompleteKey;
    private SpriteAtlas _spriteAtlas;

    #region 어드레서블, 아틀라스 라벨
    private const string ATLAS_LABEL = "PopupUI";
    private const string X_SPRITE_NAME = "X";
    private const string CONFIRM_BUTTON_SPRITE_NAME = "ConfirmButton";
    private const string POPUP_SPRITE_NAME = "Popup";
    #endregion

    #region 튜토리얼 키 초기화 및 아틀라스 설정
    private async void Awake()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        _tutorialCompleteKey = $"Tutorial_{TutorialType.Main}_{sceneName}";

        _spriteAtlas = await Manager.Instance.AddressableManager.Load<SpriteAtlas>(ATLAS_LABEL);
        SetSpriteAtlas();
    }

    private void SetSpriteAtlas()
    {
        transform.GetComponent<Image>().sprite = _spriteAtlas.GetSprite(POPUP_SPRITE_NAME);
        _closeButton.GetComponent<Image>().sprite = _spriteAtlas.GetSprite(X_SPRITE_NAME);
        _exitButton.GetComponent<Image>().sprite = _spriteAtlas.GetSprite(CONFIRM_BUTTON_SPRITE_NAME);
        _tutorialButton.GetComponent<Image>().sprite = _spriteAtlas.GetSprite(CONFIRM_BUTTON_SPRITE_NAME);
    }
    #endregion

    private void OnEnable()
    {
        if (!_soundManager)
            _soundManager = Manager.Instance.SoundManager;

        // 볼륨 슬라이더 값 초기화
        _masterVolumeSlider.value = _soundManager.MasterVolume;
        _bgmVolumeSlider.value = _soundManager.BGMVolume;
        _sfxVolumeSlider.value = _soundManager.SFXVolume;

        // 현재 씬의 튜토리얼이 존재하고 완료했다면 튜토리얼 다시보기 버튼 활성화
        bool tutorialCompleted = PlayerPrefs.HasKey(_tutorialCompleteKey) && PlayerPrefs.GetInt(_tutorialCompleteKey) == 1;
        _tutorialButton.gameObject.SetActive(tutorialCompleted);

        if (tutorialCompleted)
        {
            // 튜토리얼 다시보기 버튼에 클릭 이벤트 추가
            var tutorialController = ServiceLocator.GetService<TutorialController>();
            if (tutorialController)
                _tutorialButton.onClick.AddListener(() =>
                {
                    _soundManager.PlaySFX(SFXType.SFX_Click);
                    OnClose?.Invoke();
                    tutorialController.StartTutorialWithDelay(0.5f, true);
                });
        }
    }

    private void Start()
    {
        _masterVolumeSlider.onValueChanged.AddListener((value) => _soundManager.SetMasterVolume(value));
        _bgmVolumeSlider.onValueChanged.AddListener((value) => _soundManager.SetBGMVolume(value));
        _sfxVolumeSlider.onValueChanged.AddListener((value) => _soundManager.SetSFXVolume(value));

        _exitButton.onClick.AddListener(OnExitButton);
        _closeButton.onClick.AddListener(() =>
        {
            _soundManager.PlaySFX(SFXType.SFX_Click);
            OnClose?.Invoke();
        });
    }

    private void OnExitButton()
    {
        _soundManager.PlaySFX(SFXType.SFX_Click);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
