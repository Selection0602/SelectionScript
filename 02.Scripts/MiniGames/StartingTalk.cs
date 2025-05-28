using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class StartingTalk : MonoBehaviour
{
    public GameObject Panel;                //대사 나올 패널
    public TextMeshProUGUI Letters;         //대사가 쓰일 곳
    public TalkList talks;                  //대사
    private int _currentCount;              //현재 카운트
    private int _talkCount;                 //최대 카운트
    private Coroutine _coroutine;
    private PlayerInput _playerInput;
    
    public UnityEvent StartTalk;
    public UnityEvent EndTalk;
    
    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
    }

    private void Start()
    {
        Panel.SetActive(false);
        _currentCount = 0;
        _talkCount = talks.Talks.Count;

        StartTalk?.Invoke();
        
        var tutorialController = ServiceLocator.GetService<TutorialController>();
        if(tutorialController)
        {
            tutorialController.StartTutorial(false, () =>
            {
                _playerInput.enabled = false;
            }, () =>
            {
                _playerInput.enabled = true;
            });
        }
        
        _coroutine = StartCoroutine(Talking());
    }

    //대사가 나오는 과정
    public void OnDisplayTalk()
    {
        if (_currentCount == 0)
        {
            Panel.SetActive(true);
            StartTalk?.Invoke();
        }

        if (_currentCount == _talkCount)
        {
            Panel.SetActive(false);
            this.gameObject.GetComponent<PlayerInput>().enabled = false;
            EndTalk?.Invoke();
        }
        else
        {
            Letters.text = talks.Talks[_currentCount];
            _currentCount++;
        }
    }

    private IEnumerator Talking()
    {
        yield return new WaitForSeconds(1f);
        OnDisplayTalk();
    }

}
