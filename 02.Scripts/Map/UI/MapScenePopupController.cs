using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface IPopUp
{
    public event Action OnClose;
}

public class MapScenePopupController : MonoBehaviour
{
    [SerializeField] private SerializableDic<GameObject, Button> _popUpUIToButton; // 버튼 클릭 시 열릴 팝업 : 버튼
    [SerializeField] private Transform _popUpUIParent; // 팝업 UI 부모 오브젝트
    [SerializeField] private GameObject _panel;

    private GameObject _activeUIInstance; // 현재 열려있는 팝업 UI의 인스턴스

    private Dictionary<GameObject, GameObject> _prefabToInstance = new Dictionary<GameObject, GameObject>(); // 프리팹과 생성된 오브젝트 보관
    private Dictionary<GameObject, Button> _instanceToButton = new Dictionary<GameObject, Button>(); // 생성된 오브젝트와 버튼 보관

    private PopUpAnimation _animator;

    private void Awake()
    {
        _animator = new PopUpAnimation();
        
        _panel.SetActive(false);
        
        foreach (var data in _popUpUIToButton.dataList)
        {
            GameObject prefab = data.Key;
            Button button = data.Value;
            

            // 버튼 비활성화 색상 설정
            if(button != null)
            {
                var colorBlock = button.colors;
                colorBlock.disabledColor = Color.white;
                button.colors = colorBlock;
            }
            
            button?.onClick.AddListener(() =>
            {
                Manager.Instance.SoundManager.PlaySFX(SFXType.SFX_Click);
                OpenPopUpUI(button, prefab);
            });
            
            // 열려는 UI가 생성되지 않았다면 생성 후 딕셔너리에 추가
            if (!_prefabToInstance.TryGetValue(prefab, out var targetUI) && prefab.TryGetComponent<IPopUp>(out _))
            {
                targetUI = CreatePopUpUI(prefab, button);
                targetUI.SetActive(false);
            }
        }
    }
    
    private GameObject CreatePopUpUI(GameObject prefab, Button sourceButton)
    {
        var targetUI = Instantiate(prefab, _popUpUIParent);
        _prefabToInstance[prefab] = targetUI;
        _instanceToButton[targetUI] = sourceButton;

        if (targetUI.TryGetComponent(out IPopUp popUp))
            popUp.OnClose += () => ClosePopUpUI(targetUI);

        // 레이아웃 강제 갱신 추가
        var rect = targetUI.transform as RectTransform;
        if (rect != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

        return targetUI;
    }

    #region 팝업 UI 열기 및 닫기
    public void OpenPopUpUI(Button sourceButton, GameObject prefab)
    {
        // 열려는 UI가 생성되지 않았다면 생성 후 딕셔너리에 추가
        if (!_prefabToInstance.TryGetValue(prefab, out var targetUI))
            targetUI = CreatePopUpUI(prefab, sourceButton);
        
        // 열려는 UI가 이미 열려있다면 닫기
        if (_activeUIInstance != null && _activeUIInstance.activeSelf && targetUI != _activeUIInstance)
            ClosePopUpUI(_activeUIInstance);
        
        // 열려는 UI가 이미 열려있고 열려있는 UI와 같다면 닫기
        else if (_activeUIInstance == targetUI)
        {
            ClosePopUpUI(_activeUIInstance);
            return;
        }
        
        if (!targetUI.activeSelf)
            targetUI.SetActive(true);
        
        // 내부 컨텐츠를 애니메이션 시작 전에 바로 활성화
        if (targetUI.TryGetComponent(out CardPanel cardPanel))
            cardPanel.ShowContent();
        if (targetUI.TryGetComponent(out MemoryPanel memoryPanel))
            memoryPanel.ShowContent();

        _activeUIInstance = targetUI;
        
        // 애니메이션 재생 전 모든 버튼 비활성화
        SetAllButtonsInteractable(false);
        
        // 팝업 UI 열기 애니메이션 재생 완료 후 모든 버튼 활성화
        _animator.PlayOpenAnimation(targetUI, sourceButton, _panel , () =>
        {
            SetAllButtonsInteractable(true);
        });
    }
    
    private void ClosePopUpUI(GameObject targetUI)
    {
        if (!targetUI.activeSelf) return;
        
        _activeUIInstance = null;
        
        _instanceToButton.TryGetValue(targetUI, out var sourceButton);
        
        // 애니메이션 재생 전 모든 버튼 비활성화
        SetAllButtonsInteractable(false);
        
        // 닫기 애니메이션 재생 완료 후 팝업 UI 비활성화, 모든 버튼 활성화
        _animator.PlayCloseAnimation(targetUI, sourceButton, _panel, () =>
        {
            SetAllButtonsInteractable(true);
            targetUI.SetActive(false);
        });
    }
    
    private void SetAllButtonsInteractable(bool isInteractable)
    {
        // 모든 버튼 활성화 상태 설정
        foreach (var data in _popUpUIToButton.dataList)
            data.Value.interactable = isInteractable;
    }
    #endregion

}