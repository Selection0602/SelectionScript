using System;
using System.Collections.Generic;
using TMPro;
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
    
    [SerializeField] private GameObject _resolutionPanel;
    [SerializeField] private TMP_Dropdown _resolutionDropdown;
    [SerializeField] private Toggle _fullScreenToggle;
    [SerializeField] private Toggle _borderlessToggle;

    public event Action OnClose;
    private SoundManager _soundManager;

    private string _tutorialCompleteKey;
    private SpriteAtlas _spriteAtlas;
    
    // 해상도 설정
    private readonly List<Resolution> _availableResolutions = new List<Resolution>
    {
        new Resolution { width = 1920, height = 1080 },
        new Resolution { width = 1600, height = 900 },
        new Resolution { width = 1366, height = 768 },
        new Resolution { width = 1280, height = 720 }
    };

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

        InitializeResolutionDropdown();
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

#if UNITY_WEBGL
    _resolutionPanel.SetActive(false);
#else
        _resolutionPanel.SetActive(true);
        _resolutionDropdown.value = GetCurrentResolutionIndex();
        InitializeScreenModeToggles();
#endif
        
        // 현재 씬의 튜토리얼이 존재하고 완료했다면 튜토리얼 다시보기 버튼 활성화
        bool tutorialCompleted =
            PlayerPrefs.HasKey(_tutorialCompleteKey) && PlayerPrefs.GetInt(_tutorialCompleteKey) == 1;
        _tutorialButton.gameObject.SetActive(tutorialCompleted);

        if (tutorialCompleted)
        {
            _tutorialButton.onClick.RemoveAllListeners();
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
        
        _resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        _fullScreenToggle.onValueChanged.AddListener(OnFullScreenToggleChanged);
        _borderlessToggle.onValueChanged.AddListener(OnBorderlessToggleChanged);
    }
    
    #region 해상도 설정
    private void InitializeResolutionDropdown()
    {
        _resolutionDropdown.ClearOptions();
        
        List<string> resolutionOptions = new List<string>();
        foreach (var resolution in _availableResolutions)
        {
            resolutionOptions.Add($"{resolution.width}X{resolution.height}");
        }
        
        _resolutionDropdown.AddOptions(resolutionOptions);
        
        int currentResolutionIndex = GetCurrentResolutionIndex();
        _resolutionDropdown.value = currentResolutionIndex;
    }

    private void InitializeScreenModeToggles()
    {
        var currentMode = Screen.fullScreenMode;
        
        switch (currentMode)
        {
            case FullScreenMode.FullScreenWindow:
                _fullScreenToggle.isOn = true;
                _borderlessToggle.isOn = false;
                break;
            case FullScreenMode.MaximizedWindow:
                _fullScreenToggle.isOn = false;
                _borderlessToggle.isOn = true;
                break;
            case FullScreenMode.Windowed:
            default:
                _fullScreenToggle.isOn = false;
                _borderlessToggle.isOn = false;
                break;
        }
    }
    
    private int GetCurrentResolutionIndex()
    {
        int currentWidth = Screen.width;
        int currentHeight = Screen.height;
        
        // 현재 해상도와 일치하는 인덱스 찾기
        for (int i = 0; i < _availableResolutions.Count; i++)
        {
            if (_availableResolutions[i].width == currentWidth && _availableResolutions[i].height == currentHeight)
            {
                return i;
            }
        }
        
        // 일치하는 해상도가 없으면 0 반환 (1920x1080)
        return 0;
    }

    // 해상도 드롭다운 변경
    private void OnResolutionChanged(int index)
    {
        if (index >= 0 && index < _availableResolutions.Count)
        {
            var selectedResolution = _availableResolutions[index];
            var currentFullScreenMode = Screen.fullScreenMode;
            Screen.SetResolution(selectedResolution.width, selectedResolution.height, currentFullScreenMode);
            _soundManager.PlaySFX(SFXType.SFX_Click);
        }
    }
    
    // 전체화면 토글 클릭
    private void OnFullScreenToggleChanged(bool isFullScreen)
    {
        if (isFullScreen)
        {
            // 전체화면 모드로 변경
            _borderlessToggle.isOn = false; // 경계선 없는 창모드 토글 해제
            Screen.SetResolution(Screen.width, Screen.height, FullScreenMode.FullScreenWindow);
        }
        else
        {
            // 창모드로 변경 (경계선 없는 창모드가 활성화되어 있지 않은 경우)
            if (!_borderlessToggle.isOn)
            {
                Screen.SetResolution(Screen.width, Screen.height, FullScreenMode.Windowed);
            }
        }
        
        _soundManager.PlaySFX(SFXType.SFX_Click);
    }
    
    // 경계선 없는 창모드 토글 클릭
    private void OnBorderlessToggleChanged(bool isBorderless)
    {
        if (isBorderless)
        {
            // 경계선 없는 창모드로 변경
            _fullScreenToggle.isOn = false; // 전체화면 토글 해제
            Screen.SetResolution(Screen.width, Screen.height, FullScreenMode.MaximizedWindow);
        }
        else
        {
            // 전체화면이 아닐 경우 창모드로 변경
            if (!_fullScreenToggle.isOn)
                Screen.SetResolution(Screen.width, Screen.height, FullScreenMode.Windowed);
        }
        
        _soundManager.PlaySFX(SFXType.SFX_Click);
    }
    #endregion
    
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
