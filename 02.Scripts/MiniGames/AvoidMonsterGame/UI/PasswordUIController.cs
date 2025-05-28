using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class PasswordUIController : BaseAvoidUIController
{
    [SerializeField] private TextMeshProUGUI[] _passwordTexts;
    [SerializeField] private GameObject _cursorObject;
    [SerializeField] private SerializableDic<string, Sprite> _passwordBGSerializableDic;
    private Dictionary<string, Sprite> _passwordBGDict = new Dictionary<string, Sprite>();
    
    private const float CURSOR_OFFSET = 75.5f;
    private float _initStartPosX = 0f;
    
    private int _currentDigits = 0; // 현재 자리수
    private int[] _password = new int[4] { 0, 0, 0, 0 };
    
    private string _correctPassword;

    private Action _onOpenSafeBox;
    public event Action OnClosePasswordUI;
    
    [SerializeField] private SpriteEventChannel _spriteEventChannel;

    private void Awake()
    {
        _initStartPosX = _cursorObject.transform.localPosition.x;
        
        foreach (var item in _passwordBGSerializableDic.dataList)
            _passwordBGDict.Add(item.Key, item.Value);
    }

    public void Show(string password, Action onOpenSafeBox = null)
    {
        base.Show();
        StartPasswordInput(password);
        _onOpenSafeBox += onOpenSafeBox;
    }

    public override void Hide()
    {
        base.Hide();
        _onOpenSafeBox = null;
    }

    private void StartPasswordInput(string correctPassword)
    {
        _correctPassword = correctPassword;
        Debug.Log(correctPassword);
        _spriteEventChannel?.Invoke(_passwordBGDict[correctPassword]);
        _currentDigits = 0;

        for (int i = 0; i < _password.Length; i++)
        {
            _password[i] = 0;
            _passwordTexts[i].text = _password[i].ToString();
        }
        
        UpdateCursorPosition();
        UpdatePasswordDisplay();
    }
    
    private void UpdateCursorPosition()
    {
        float xPos = _initStartPosX + (CURSOR_OFFSET * _currentDigits);
        
        _cursorObject.transform.localPosition = 
            new Vector3(xPos, _cursorObject.transform.localPosition.y, _cursorObject.transform.localPosition.z);
    }
    
    private void UpdatePasswordDisplay()
    {
        for (int i = 0; i < 4; i++)
            _passwordTexts[i].text = _password[i].ToString();
    }
    
    public override void OnMoveUp()
    {
        _password[_currentDigits] = (_password[_currentDigits] + 1) % 10;
        UpdatePasswordDisplay();
    }

    public override void OnMoveDown()
    {
        _password[_currentDigits] = (_password[_currentDigits] + 9) % 10;
        UpdatePasswordDisplay();
    }

    public override void OnMoveLeft()
    {
        _currentDigits = (_currentDigits + 3) % 4;
        UpdateCursorPosition();
    }

    public override void OnMoveRight()
    {
        _currentDigits = (_currentDigits + 1) % 4;
        UpdateCursorPosition();
    }

    public override void OnSubmit()
    {
        if (_correctPassword == string.Join("", _password))
        {
            _onOpenSafeBox?.Invoke();
        }
        
        OnClosePasswordUI?.Invoke();
        _spriteEventChannel?.Invoke(null);
    }

    public override void OnCancel()
    {
        OnClosePasswordUI?.Invoke();
        _spriteEventChannel?.Invoke(null);
    }
}
