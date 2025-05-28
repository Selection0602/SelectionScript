using System;
using TMPro;
using UnityEngine;

public class MenuUIController : BaseAvoidUIController
{
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private TextMeshProUGUI _stepCountText; // 스텝 카운트 텍스트
    
    [SerializeField] private GameObject[] _menuSelections; // 메뉴 선택 항목
    [SerializeField] private GameObject _selectCursor; // 선택 커서
    
    private int _currentIndex = 0; // 현재 선택된 메뉴 인덱스
    
    public event Action OnOpenInventory;
    public event Action OnCloseMenu;

    // 타이머 UI 업데이트
    public void UpdateTimer(float time)
    {
        int hours = Mathf.FloorToInt(time / 3600);
        int minutes = Mathf.FloorToInt((time % 3600) / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        _timerText.text = $"{hours:00}:{minutes:00}:{seconds:00}";
    }
    
    // 선택 커서 위치 업데이트
    private void UpdateCursorPosition()
    {
        if (_menuSelections.Length <= 0) return;
        
        _selectCursor.transform.position = new Vector3
        (
            _selectCursor.transform.position.x,
            _menuSelections[_currentIndex].transform.position.y,
            _selectCursor.transform.position.z
        );
    }
    
    public void SetStepCount(int stepCount)
    {
        _stepCountText.text = stepCount.ToString();
    }
    
    public override void OnMoveUp()
    {
        if (_menuSelections.Length <= 1) return;
        
        _currentIndex = (_currentIndex - 1 + _menuSelections.Length) % _menuSelections.Length;
        UpdateCursorPosition();
    }
    
    public override void OnMoveDown()
    {
        if (_menuSelections.Length <= 1) return;
        
        _currentIndex = (_currentIndex + 1) % _menuSelections.Length;
        UpdateCursorPosition();
    }
    
    public override void OnMoveLeft()
    {
    }
    
    public override void OnMoveRight()
    {
    }
    
    public override void OnSubmit()
    {
        switch (_currentIndex)
        {
            case 0: // 인벤토리 열기
                OnOpenInventory?.Invoke();
                break;
        }
    }
    
    public override void OnCancel()
    {
        OnCloseMenu?.Invoke();
    }
    
    public override void Show()
    {
        base.Show();
        _currentIndex = 0;
        UpdateCursorPosition();
    }
}
