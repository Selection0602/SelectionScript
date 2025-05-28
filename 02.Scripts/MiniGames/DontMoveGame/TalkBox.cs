using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TalkBox : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _talk; // 대화 텍스트 UI
    [SerializeField] private float _typingSpeed = 0.05f; // 타이핑 속도
    
    [SerializeField] private GameObject _ImageBG;
    [SerializeField] private Image _itemImage;        
    
    private string[] _dialogueTexts; // 대화 텍스트 배열
    private int _currentDialogueIndex; // 현재 대화 인덱스
    private Coroutine _typingCoroutine; // 현재 실행 중인 타이핑 코루틴
    private bool _isTyping; // 타이핑 중인지 여부
    private Action _onDialogueComplete; // 대화 완료 시 콜백

    private bool _isChoiceDialogue; // 선택지 대화 여부
    
    // 대화 시작
    public void StartDialogue(string[] texts, Action onComplete = null, Sprite itemImage = null, bool isChoice = false)
    {
        _dialogueTexts = texts;
        _currentDialogueIndex = 0;
        _onDialogueComplete = onComplete;

        _isChoiceDialogue = isChoice;
        
        if(itemImage != null)
        {
            _itemImage.sprite = itemImage;
            _ImageBG.SetActive(true);
            _itemImage.gameObject.SetActive(true);
        }
        else
        {
            _ImageBG.SetActive(false);
            _itemImage.gameObject.SetActive(false);
        }
        
        gameObject.SetActive(true);
        _talk.gameObject.SetActive(true);
        DisplayCurrentDialogue();
    }
    
    // 현재 대화 표시
    private void DisplayCurrentDialogue()
    {
        if (_currentDialogueIndex < _dialogueTexts.Length)
        {
            if (_typingCoroutine != null)
                StopCoroutine(_typingCoroutine);
            
            if (_typingSpeed <= 0)
            {
                _talk.text = _dialogueTexts[_currentDialogueIndex];
                _isTyping = false;
            }
            else
            {
                _typingCoroutine = StartCoroutine(TypingEffect(_dialogueTexts[_currentDialogueIndex]));
            }

        }
        else
            // 모든 대화 완료
            CompleteDialogue();
    }
    
    // 다음 대화로 진행
    public void ProceedToNextDialogue()
    {
        if (_isTyping)
        {
            // 아직 텍스트 출력 중이라면 바로 완료 처리
            CompleteTyping();
        }
        else
        {
            // 텍스트 출력이 완성 된 후 호출됐다면 다음 대화로 이동
            _currentDialogueIndex++;
            DisplayCurrentDialogue();
        }
    }
    
    // 타이핑 즉시 완료
    private void CompleteTyping()
    {
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
            _typingCoroutine = null;
        }
        
        _talk.text = _dialogueTexts[_currentDialogueIndex];
        _isTyping = false;
    }
    
    // 대화 완료
    private void CompleteDialogue()
    {
        if(!_isChoiceDialogue)
            gameObject.SetActive(false);
        
        _onDialogueComplete?.Invoke();
    }
    
    // 타이핑 효과
    private IEnumerator TypingEffect(string fullText)
    {
        _isTyping = true;
        _talk.text = "";
        
        foreach (char c in fullText)
        {
            _talk.text += c;
            yield return new WaitForSecondsRealtime(_typingSpeed);
        }
        
        _isTyping = false;
        _typingCoroutine = null;
    }
    
    private void OnDisable()
    {
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
            _typingCoroutine = null;
        }
        
        _itemImage.sprite = null;
        
        _ImageBG.SetActive(false);
        _itemImage.gameObject.SetActive(false);
    }
}