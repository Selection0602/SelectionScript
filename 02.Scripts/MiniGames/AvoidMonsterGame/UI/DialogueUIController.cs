using System;
using UnityEngine;

public class DialogueUIController : BaseAvoidUIController
{
    [SerializeField] private TalkBox _talkBox;
    
    [Header("선택지 UI")]
    [SerializeField] private GameObject _choicePanel;
    [SerializeField] private GameObject _yesButton;
    [SerializeField] private GameObject _noButton;
    [SerializeField] private GameObject _selectCursor;
    
    private bool _currentButton = true;
    
    private Action _onYes;
    private Action _onNo;
    
    public event Action OnOpenDialogue;
    public event Action OnCloseDialogue;

    public void StartDialogue(string[] texts, Action onComplete = null, Sprite itemImage = null)
    {
        OnOpenDialogue?.Invoke();
        _talkBox.StartDialogue(texts, onComplete, itemImage);
    }
    
    public void StartDialogueWithChoices(string[] text, Action onYes, Action onNo)
    {
        _onYes += onYes;
        _onNo += onNo;
        
        OnOpenDialogue?.Invoke();
        _talkBox.StartDialogue(text, ShowChoices, null, true);
    }
    
    private void ShowChoices()
    {
        _choicePanel.SetActive(true);
        _selectCursor.SetActive(true);

        UpdateCursorPosition();
    }
    
    private void UpdateCursorPosition()
    {
        _selectCursor.transform.position = _currentButton ? 
            new Vector3(_selectCursor.transform.position.x, _yesButton.transform.position.y, _selectCursor.transform.position.z) : 
            new Vector3(_selectCursor.transform.position.x, _noButton.transform.position.y, _selectCursor.transform.position.z);
    }
    
    public override void OnMoveUp()
    {
        _currentButton = !_currentButton;

        UpdateCursorPosition();
    }

    public override void OnMoveDown()
    {
        _currentButton = !_currentButton;
        
        UpdateCursorPosition();
    }

    public override void OnMoveLeft() {}

    public override void OnMoveRight() {}

    public override void OnSubmit()
    {
        if (_choicePanel.activeSelf)
        {
            Action selectedAction = _currentButton ? _onYes : _onNo;
        
            selectedAction?.Invoke();
            _choicePanel.SetActive(false);

        }
        else
        {
            _talkBox.ProceedToNextDialogue();
        }
    }

    public override void OnCancel()
    {
    }

    private void OnDisable()
    {
        OnCloseDialogue?.Invoke();
        
        _currentButton = true;
        
        _onYes = null;
        _onNo = null;
    }
}
