using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.U2D;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class KnifeEscapeObject : BaseInteractObject
{
    public CheckObject CO;                 //검사해야할 CheckObject(스테이지 2의 Knife)

    [Header("Result TalkList Settings")]
    [SerializeField] private TalkList _tList_01;             //결과1 - 체크O 
    [SerializeField] private TalkList _tList_02;             //결과2 - 체크X

    [Header("Result Panel Settings")]
    [SerializeField] private NormalPanel _normalPanel;      //실패했을 때 뜰 패널
    [SerializeField] private CheckPanel _checkPanel;        //성공했을 때 뜰 패널 -> 선택지 제공
    private int _currentSituation = 0;                      //결과 상황

    [Header("Knife SpriteAtlas Settings")]
    [SerializeField] private string _atlasAddress;           //Addressables Atlas 주소
    [SerializeField] private string _originSpriteName;       //원래 이미지 이름
    [SerializeField] private string _changeSpriteName;       //바뀔 이미지 이름

    private Sprite _originImage;                            //원래 이미지
    private Sprite _changeImage;                            //바뀔 이미지
    private SpriteAtlas _loadedAtlas;                       //아틀라스

    [Header("Get Knife Event")]
    public UnityEvent GetKnife;                             //칼을 가지게 하는 이벤트

    [Header("Other Settings")]
    [SerializeField] private AvoidItemData _knifeItemData;
    [SerializeField] private Inventory _inventory;
    [SerializeField] private TalkBox _talkBox;

    private void Start()
    {
        StartCoroutine(WaitForAtlasLoadAndInit());
    }

    private IEnumerator WaitForAtlasLoadAndInit()
    {
        // Addressables로 SpriteAtlas 불러오기
        var handle = Addressables.LoadAssetAsync<SpriteAtlas>(_atlasAddress);
        yield return handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _loadedAtlas = handle.Result;

            _originImage = _loadedAtlas.GetSprite(_originSpriteName);
            _changeImage = _loadedAtlas.GetSprite(_changeSpriteName);

            if (_originImage == null || _changeImage == null)
            {
                Debug.LogWarning("아틀라스에서 스프라이트를 찾을 수 없습니다.");
                yield break;
            }

            //이미지 세팅 완료 후 CheckDict가 준비될 때까지 대기
            yield return new WaitUntil(() => CO.CheckDict.ContainsKey(0));

            Init();
        }
        else
        {
            Debug.LogError("SpriteAtlas 로드 실패!");
        }
    }

    public override void Init()
    {
        _currentCount = 0;
        //결과1 - 체크O 
        if (CO.CheckDict[_currentCount])
        {
            Panel = _normalPanel;
            TList = _tList_01;
            _currentSituation = 1;
            gameObject.GetComponent<SpriteRenderer>().sprite = _changeImage;
        }
        //결과2 - 체크X
        else
        {
            Panel = _checkPanel;
            TList = _tList_02;
            _currentSituation = 2;
            gameObject.GetComponent<SpriteRenderer>().sprite = _originImage;
        }

        //SO로 받은 질문 리스트를 Dictionary 대입
        for (int i = 0; i < TList.Talks.Count; i++)
        {
            TDict[i] = TList.Talks[i].Replace("\\n", "\n");
        }

        Talk = TDict[_currentCount];
        _tCount = TDict.Count;
        CanInteract = true;
    }

    public override void DisplayNextTalk()
    {
        if (_currentSituation == 1) return;         //체크O
        else if (_currentSituation == 2)            //체크X
        {
            GetKnife?.Invoke();
            gameObject.GetComponent<SpriteRenderer>().sprite = _changeImage;
            CanInteract = false;
            OnGetKnife();
        }
    }

    //칼을 가잘때 실행하는 함수
    private void OnGetKnife()
    {
        _inventory.AddItem(_knifeItemData);

        _talkBox.StartDialogue(_knifeItemData.DialogueTexts,
            null, _knifeItemData.ItemSprite);
    }

    #region OnTrigger
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<GBInteractDetector>() != null)
        {
            _checkPanel.YesButton.onClick.AddListener(DisplayNextTalk);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<GBInteractDetector>() != null)
        {
            _checkPanel.YesButton.onClick.RemoveListener(DisplayNextTalk);
        }
    }
    #endregion
}
