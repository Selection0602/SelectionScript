using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreditPanel : MonoBehaviour
{
    [SerializeField]private TextMeshProUGUI desc;
    
    // isLocked를 프로퍼티로 변경
    private bool _isLocked;
    
    // NonSerialized 속성 제거 (프로퍼티에는 적용 불가)
    public bool isLocked
    {
        get => _isLocked;
        set
        {
            _isLocked = value;
            UpdateButtonState();
        }
    }
    
    private Button button;
    private Action onClickAction;

    private void Awake()
    {
        button = GetComponent<Button>();
    }
    
    private void OnEnable()
    {        
        // 부모 캔버스가 활성화되어 있는지 확인
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        
        // 버튼 상태 초기화
        UpdateButtonState();
    }
    
    public void SetText(string text)
    {
        desc.text = text;
    }

    public string GetCurrentText()
    {
        return desc.text;
    }

    public void SetButtonAction(Action onClick)
    {
        if (button == null) button = GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick?.Invoke());
        
        // 액션 저장
        onClickAction = onClick;
        
        // 버튼 상태 업데이트
        UpdateButtonState();
    }
    
    private void UpdateButtonState()
    {
        if (button == null) button = GetComponent<Button>();
        
        // 버튼은 항상 활성화 (잠금 상태는 내부 로직에서 처리)
        button.interactable = true;
        
        // 잠금 상태에 따라 시각적 피드백 변경 (선택 사항)
        // 예: 잠긴 상태일 때 버튼 투명도 조절
        var colors = button.colors;
        colors.normalColor = _isLocked ? new Color(0.7f, 0.7f, 0.7f, 1f) : Color.white;
        button.colors = colors;
        
    }
}