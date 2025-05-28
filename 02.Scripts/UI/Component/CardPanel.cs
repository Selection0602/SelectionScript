using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardPanel : MonoBehaviour, IPopUp
{
    [SerializeField] private ScrollRect scrollView;
    [SerializeField] private Transform content;
    [SerializeField] private GameObject cardPrefab;
    
    private List<GameObject> cardPool = new List<GameObject>();
    private DataManager dataManager;
    private bool isInitialized = false;
    
    public event Action OnClose;
    
    private void Awake()
    {
        // 컴포넌트 확인 및 설정
        if (GetComponent<RectTransform>() == null)
        {
            gameObject.AddComponent<RectTransform>();
        }
        
        if (scrollView == null)
        {
            scrollView = GetComponentInChildren<ScrollRect>();
        }
        
        if (content == null && scrollView != null)
        {
            content = scrollView.content;
        }
        
        dataManager = Manager.Instance.DataManager;
        
        // 시작 시 비활성화 상태로 설정
        if (scrollView != null)
        {
            scrollView.gameObject.SetActive(false);
        }
        
        isInitialized = true;
    }
    
    private void OnEnable()
    {
        // 패널이 활성화될 때마다 DataManager를 다시 확인
        if (dataManager == null)
        {
            dataManager = Manager.Instance.DataManager;
            if (dataManager == null)
            {
                return;
            }
        }
        // 비동기로 카드 표시 (프레임 드랍 방지)
        if (isInitialized)
        {
            StartCoroutine(DisplayCardCollectionCoroutine());
        }
    }
    
    private IEnumerator DisplayCardCollectionCoroutine()
    {
        ReturnAllPanels();
        
        if (dataManager == null || dataManager.PlayerDeck == null || content == null) 
        {
            yield break;
        }
        
        if (cardPrefab == null)
        {
            yield break;
        }
        
        // 덱 데이터가 비어있는지 확인
        if (dataManager.PlayerDeck.Count == 0)
        {
            yield break;
        }
            
        int count = 0;
        foreach (var cardData in dataManager.PlayerDeck.Values)
        {
            if (cardData == null) continue;
            
            var panelObj = GetCardPanel();
            var ui = panelObj.GetComponent<RewardCell>();
            
            if (ui != null)
            {
                RewardData rewardData = new RewardData
                {
                    RewardName = cardData.CardName,
                    Desc = cardData.Desc,
                    Image = cardData.Image,
                    GetCardIndex = cardData.Index
                };
                ui.Set(rewardData);
                
                count++;
                
                // 10개마다 프레임 건너뛰기 (성능 최적화)
                if (count % 10 == 0)
                {
                    yield return null;
                }
            }
        }
        
        // 스크롤뷰 리셋
        if (scrollView != null)
        {
            scrollView.normalizedPosition = new Vector2(0, 1);
            yield return null;
            
            // 스크롤뷰 콘텐츠 크기 업데이트
            LayoutRebuilder.ForceRebuildLayoutImmediate(content as RectTransform);
        }
    }
    
    private GameObject GetCardPanel()
    {
        foreach (var obj in cardPool)
        {
            if (obj != null && !obj.activeInHierarchy)
            {
                obj.SetActive(true);
                return obj;
            }
        }
        
        GameObject newObj = Instantiate(cardPrefab, content);
        
        // RectTransform 확인 및 추가
        if (newObj.GetComponent<RectTransform>() == null)
        {
            newObj.AddComponent<RectTransform>();
        }
        
        cardPool.Add(newObj);
        return newObj;
    }
    
    private void ReturnAllPanels()
    {
        foreach (var obj in cardPool)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }
    
    private void OnDisable()
    {
        // 코루틴 중지
        StopAllCoroutines();
        
        if (scrollView != null)
        {
            scrollView.gameObject.SetActive(false);
        }
        
        ReturnAllPanels();
    }
    
    // ESC 키를 눌러 패널 닫기
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnClose?.Invoke();
        }
    }

    public void ShowContent()
    {
        if (scrollView != null) scrollView.gameObject.SetActive(true);
        if (content != null) content.gameObject.SetActive(true);
    }
} 