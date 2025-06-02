using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class TutorialUI : MonoBehaviour
{
    [SerializeField] private RectTransform _tutorialUI;
    [SerializeField] private Button _nextButton;
    [SerializeField] private GameObject _objectPanel;
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _descText;
    
    private List<TutorialData> _tutorialSequence;
    private Func<TutorialData, List<GameObject>> _getTargetsFunc;
    private TutorialType _currentTutorialType;

    
    private int _currentTutorialIndex;
    private Dictionary<GameObject, Transform> _originalParent = new Dictionary<GameObject, Transform>();
    private Action _onComplete;
    private SpriteAtlas _spriteAtlas;

    #region 어드레서블, 아틀라스 라벨
    private const string ATLAS_LABEL = "PopupUI";
    private const string POPUP_SPRITE_NAME = "Popup";
    private const string NEXT_BUTTON_SPRITE_NAME = "Start";
    #endregion

    private async void Awake()
    {
        _nextButton.onClick.AddListener(OnNextButtonClicked);

        _spriteAtlas = await Manager.Instance.AddressableManager.Load<SpriteAtlas>(ATLAS_LABEL);
        SetSpriteAtlas();
    }

    private void SetSpriteAtlas()
    {
        _nextButton.GetComponent<Image>().sprite = _spriteAtlas.GetSprite(NEXT_BUTTON_SPRITE_NAME);
        _tutorialUI.GetComponent<Image>().sprite = _spriteAtlas.GetSprite(POPUP_SPRITE_NAME);
    }

    
    /// <summary>
    /// 튜토리얼 UI 초기화 (튜토리얼 목록 및 타겟을 가져오기 위한 Func)
    /// </summary>
    /// <param name="sequence">튜토리얼 목록</param>
    /// <param name="getTargetsFunc">데이터의 타겟을 가져오기 위한 Func</param>
    /// <param name="type">튜토리얼 타입</param>>
    /// <param name="onStart">튜토리얼 시작 시 호출될 Action</param>
    /// <param name="onComplete">튜토리얼 완료 시 호출될 Action</param>
    public void Initialize(List<TutorialData> sequence, Func<TutorialData, List<GameObject>> getTargetsFunc, TutorialType type, Action onComplete)
    {
        _tutorialSequence = sequence;
        _getTargetsFunc = getTargetsFunc;
        _currentTutorialType = type;
        _onComplete = onComplete;
    }
    
    /// <summary>
    /// 튜토리얼 시퀀스 인덱스에 맞게 띄우기
    /// </summary>
    /// <param name="index"></param>
    public void ShowTutorial(int index)
    {
        if (_tutorialSequence == null || index < 0 || index >= _tutorialSequence.Count)
        {
            CloseTutorial();
            return;
        }
     
        Time.timeScale = 0f;
        
        // 부모가 변경된 오브젝트들 다시 원래 위치로 변경
        RestoreParentAllTargets();
        transform.SetAsLastSibling();
        
        _currentTutorialIndex = index;
        // 현재 인덱스에 맞는 데이터 가져오기
        TutorialData data = _tutorialSequence[index];
        
        _titleText.text = data.TitleText;
        _descText.text = data.DescText;
        
        _objectPanel.SetActive(true);
        gameObject.SetActive(true);
        
        if (_getTargetsFunc != null)
        {
            // 현재 인덱스의 데이터의 타겟리스트 가져오기
            List<GameObject> targets = _getTargetsFunc(data);
            // 타겟 부모를 튜토리얼 UI 쪽으로 변경
            if(_currentTutorialType != TutorialType.Memory)
                SetParentTargets(targets);
        }
    }
    
    private void SetParentTargets(List<GameObject> targets)
    {
        if (targets == null || targets.Count == 0)
            return;
            
        foreach (var target in targets)
        {
            if (!target)
                continue;
                
            if (!_originalParent.ContainsKey(target))
            {
                // 튜토리얼 종료 후 원래 위치로 돌려놓기 위해 원래의 부모 저장
                _originalParent[target] = target.transform.parent;
                
                target.transform.SetParent(_objectPanel.transform, true);
                
                RectTransform targetRectTransform = target.GetComponent<RectTransform>();
                if (targetRectTransform)
                {
                    // 튜토리얼UI의 앵커 및 피벗 타겟과 같도록 변경
                    _tutorialUI.anchorMin = targetRectTransform.anchorMin;
                    _tutorialUI.anchorMax = targetRectTransform.anchorMax;
                
                    _tutorialUI.pivot = targetRectTransform.pivot;
                
                    // SO에 설정된 위치 오프셋 가져오기
                    Vector2 offset = GetOffsetForTarget(target);
                    
                    if(offset != Vector2.zero)
                        _tutorialUI.anchoredPosition = targetRectTransform.anchoredPosition + offset;
                }
            }
        }
    }
    
    // 타겟 데이터에 들어있는 오프셋 정보 가져오기
    private Vector2 GetOffsetForTarget(GameObject target)
    {
        TutorialTarget tutorialTarget = target.GetComponent<TutorialTarget>();

        if (tutorialTarget == null) return Vector2.zero;
        
        var data = _tutorialSequence[_currentTutorialIndex];
        
        if (data.TargetInfos != null)
        {
            foreach (var targetInfo in data.TargetInfos)
            {
                if (targetInfo.TargetId == tutorialTarget.TargetId)
                {
                    return targetInfo.UIOffset;
                }
            }
        }

        return Vector2.zero;
    }
    
    // 저장된 타겟들 원래 부모로 되돌리기
    private void RestoreParentAllTargets()
    {
        foreach (var pair in _originalParent)
        {
            if (pair.Key)
                pair.Key.transform.SetParent(pair.Value, true);
        }
        
        _originalParent.Clear();
    }
    
    private void OnNextButtonClicked()
    {
        // 다음 타겟이 없다면 튜토리얼 종료
        if (_currentTutorialIndex >= _tutorialSequence.Count - 1)
        {
            CloseTutorial();
        }
        // 다음 타겟이 있다면 다음 인덱스 튜토리얼 시작
        else
        {
            _currentTutorialIndex++;
            ShowTutorial(_currentTutorialIndex);
        }
    }
    
    private void CloseTutorial()
    {
        Time.timeScale = 1f;
        
        // 타겟 부모 초기화
        RestoreParentAllTargets();
        _objectPanel.SetActive(false);
        gameObject.SetActive(false);
        
        // 컨트롤러에서 튜토리얼 완료 처리(PlayerPrefs)
        TutorialController controller = FindObjectOfType<TutorialController>();
        if (controller)
            controller.CompleteTutorial(_currentTutorialType);

        _onComplete?.Invoke();
        Destroy(gameObject);
    }
    
    private void OnDestroy()
    {
        if (_nextButton)
            _nextButton.onClick.RemoveListener(OnNextButtonClicked);
    }
}