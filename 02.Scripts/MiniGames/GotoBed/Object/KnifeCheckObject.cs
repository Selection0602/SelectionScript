using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class KnifeCheckObject : CheckObject
{
    [Header("SpriteAtlas Settings")]
    [SerializeField] private string _atlasAddress;          //아틀라스 어드레서블
    [SerializeField] private string _originSpriteName;      //스프라이트 이름
    [SerializeField] private string _replaceSpriteName;     //바뀔 스프라이트 이름

    private Sprite _originImage;                        //원래 이미지
    private Sprite _replaceImage;                       //바뀔 이미지
    private SpriteAtlas _loadedAtlas;                   //아틀라스

    private void Awake()
    {
        Init();
    }

    private void OnAtlasLoaded(AsyncOperationHandle<SpriteAtlas> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _loadedAtlas = handle.Result;

            _originImage = _loadedAtlas.GetSprite(_originSpriteName);
            _replaceImage = _loadedAtlas.GetSprite(_replaceSpriteName);

            if (_originImage == null || _replaceImage == null)
            {
                Debug.LogWarning("Atlas에서 스프라이트를 찾을 수 없습니다.");
                return;
            }

            ChangeToOriginImage();      //이미지 먼저 세팅
            base.Init();                
        }
    }


    public override void Init()
    {
        Addressables.LoadAssetAsync<SpriteAtlas>(_atlasAddress).Completed += OnAtlasLoaded;
    }

    //원래 이미지로 바꾸기
    public void ChangeToOriginImage()
    {
        GetComponent<SpriteRenderer>().sprite = _originImage;
    }
    //바뀔 이미지로 바꾸기
    public void ChangeToReplaceImage()
    {
        GetComponent<SpriteRenderer>().sprite = _replaceImage;
    }

    #region OnTrigger
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<GBInteractDetector>() != null)
        {
            _checkPanel.YesButton.onClick.AddListener(DisplayNextTalk);
            _checkPanel.YesButton.onClick.AddListener(ChangeToReplaceImage);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<GBInteractDetector>() != null)
        {
            _checkPanel.YesButton.onClick.RemoveListener(DisplayNextTalk);
            _checkPanel.YesButton.onClick.RemoveListener(ChangeToReplaceImage);
        }
    }
    #endregion
}
