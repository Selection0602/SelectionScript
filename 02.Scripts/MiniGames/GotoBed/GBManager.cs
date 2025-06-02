using System.Collections;
using TMPro;
using UnityEngine;

public class GBManager : MonoBehaviour
{
    [Header("Basic Settings")]
    [SerializeField] private GBPlayer _player;                     //플레이어
    [SerializeField] private int _stage = 1;                       //스테이지

    [Header("Display Simulation Settings")]
    [SerializeField] private string _simulation;                    //"상황" string
    [SerializeField] private TextMeshProUGUI _stageText;            //스테이지 보여줄 text
    [SerializeField] private GameObject _darkPanel;                 //스테이지 보여줄 text
    private Coroutine coroutine;                                    //코루틴

    [Header("Teleport Position Settings")]
    [SerializeField] private Transform _stage02Pos;                //스테이지2 위치
    [SerializeField] private Transform _stage03Pos;                //스테이지3 위치
    [SerializeField] private Transform _stage03SecondPos;          //스테이지3 두번째 위치

    private void Start()
    {
        Manager.Instance.CreditManager.UnlockMiniGameAndSave(3);

        _darkPanel.SetActive(false);
        Manager.Instance.SoundManager.PlayBGM(BGMType.BGM_MiniGame);
        coroutine = StartCoroutine(ShowSimulText());
    }

    //다음 스테이지로 보내기
    public void GoNextSatge()
    {
        _stage++;
        if (_stage == 2) 
        {
            _player.Teleport(_stage02Pos.position);
        }
        else if(_stage == 3) 
        {
            _player.Teleport(_stage03Pos.position);
        }
        coroutine = StartCoroutine(ShowSimulText());
    }

    //플레이어 없애기
    public void RemovePlayer()
    {
        _player.gameObject.SetActive(false);
    }

    //3스테이지 지상층으로 가기
    public void GoGroundFloor()
    {
        _player.Teleport(_stage03SecondPos.position);
    }

    //다음 상황의 텍스트 띄우는 효과 보여주기
    private IEnumerator ShowSimulText()
    {
        _player.ChangeInputToEmpty();
        
        _stageText.text = _simulation +" "+_stage.ToString();
        _stageText.gameObject.SetActive(true);
        _darkPanel.SetActive(true); 
        yield return new WaitForSeconds(1.5f);
        _stageText.gameObject.SetActive(false);
        _darkPanel.SetActive(false);
        
        _player.ChangeInputToPlayer();

        //1스테이지일 경우, 튜토리얼을 보여주기
        if(_stage == 1)
        {
            var tutorialController = ServiceLocator.GetService<TutorialController>();
            if (tutorialController)
            {
                tutorialController.StartTutorial(false, () =>
                {
                    _player.ChangeInputToEmpty();
                }, () =>
                {
                    _player.ChangeInputToPlayer();
                });
            }
        }
    }
}
