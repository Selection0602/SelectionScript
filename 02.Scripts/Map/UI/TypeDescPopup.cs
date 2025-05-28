using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;
using UnityEngine.UI;

public class TypeDescPopup : MonoBehaviour, IPopUp
{
    [SerializeField] private TypeDescBox _descPrefab;
    [SerializeField] private Transform _descParent;

    [SerializeField] private Button _closeButton;

    public event Action OnClose;

    private SpriteAtlas _spriteAtlas;

    #region 어드레서블, 아틀라스 라벨
    private const string TYPE_DESC_LABEL = "NodeType";
    private const string ATLAS_LABEL = "PopupUI";
    private const string X_SPRITE_NAME = "X";
    private const string POPUP_SPRITE_NAME = "Popup";
    #endregion

    #region 초기화 및 아틀라스 세팅
    private async void Awake()
    {
        _closeButton.onClick.AddListener(() =>
        {
            Manager.Instance.SoundManager.PlaySFX(SFXType.SFX_Click);
            OnClose?.Invoke();
        });

        _spriteAtlas = await Manager.Instance.AddressableManager.Load<SpriteAtlas>(ATLAS_LABEL);
        SetSpriteAtlas();
    }

    private void SetSpriteAtlas()
    {
        _closeButton.GetComponent<Image>().sprite = _spriteAtlas.GetSprite(X_SPRITE_NAME);
        GetComponent<Image>().sprite = _spriteAtlas.GetSprite(POPUP_SPRITE_NAME);
    }
    #endregion

    private async void Start()
    {
        try
        {        
            // 어드레서블 매니저에 저장된 데이터가 있으면 불러오기 / 없다면 직접 로드
            if (Manager.Instance.AddressableManager.HasHandle(TYPE_DESC_LABEL))
                await LoadNodeTypeData();
            else
                LoadManualNodeTypeData(TYPE_DESC_LABEL);

        }
        catch (Exception e)
        {
            Debug.LogError($"노드 타입 데이터를 불러오는 중 오류가 발생하였습니다. {e.Message}");
        }
    }

    #region 어드레서블 매니저를 통한 데이터 로드
    // 어드레서블 매니저를 통해 데이터 가져오기
    private async Task LoadNodeTypeData()
    {
        try
        {
            List<NodeTypeData> nodeTypeDataList = await Manager.Instance.AddressableManager.GetHandleResultList<NodeTypeData>("NodeType");

            if (nodeTypeDataList == null)
            {
                Debug.LogError("노드 타입 데이터를 불러오지 못했습니다.");
                LoadManualNodeTypeData(TYPE_DESC_LABEL);
                return;
            }

            SetNodeTypeData(nodeTypeDataList);
        }
        catch (Exception e)
        {
            Debug.LogError($"노드 타입을 불러오는 중 오류가 발생하였습니다. {e.Message}");
            throw;
        }
    }

    private void SetNodeTypeData(List<NodeTypeData> dataList)
    {
        // enum 순서에 맞게 정렬
        dataList.Sort((a, b) => ((int)a.Type).CompareTo((int)b.Type));

        // 설명 슬롯 생성 후 초기화 진행
        foreach (NodeTypeData data in dataList)
        {
            TypeDescBox typeDesc = Instantiate(_descPrefab, _descParent);
            typeDesc.SetData(data);
        }
    }
    #endregion
    
    #region 수동 데이터 로드
    private void LoadManualNodeTypeData(string label)
    {
        // NodeType 라벨을 가진 모든 NodeTypeData를 로드
        var handle = Addressables.LoadAssetsAsync<NodeTypeData>(label, null);
        // 로드 완료 시 실행 할 이벤트 등록
        handle.Completed += OnNodeTypeDataLoaded;
    }

    private void OnNodeTypeDataLoaded(AsyncOperationHandle<IList<NodeTypeData>> handle)
    {
        // 로드 성공 시
        if (handle.Status != AsyncOperationStatus.Succeeded) return;

        // 불러온 데이터를 리스트로 변환
        var nodeTypeDataList = new List<NodeTypeData>(handle.Result);

        SetNodeTypeData(nodeTypeDataList);

        Addressables.Release(handle);
    }
    #endregion
}