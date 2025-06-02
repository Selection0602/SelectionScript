using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UpperBar : MonoBehaviour
{
    #region ------------------------------변수------------------------------
    private DataManager DM;
    [Header("HP")] [SerializeField] private TextMeshProUGUI text;
    [Header("Booty")] public List<Image> slots = new();
    [SerializeField] private List<bootyPanel> infoPanels = new();
    
    [Header("팝업 설정")]
    [SerializeField] private GameObject memoryPanelPrefab; // MemoryPanel 프리팹
    [SerializeField] private GameObject cardPanelPrefab; // CardPanel 프리팹
    
    [Header("UI 버튼")]
    [SerializeField] private Transform buttonsParent; // Buttons 게임오브젝트
    private GameObject settingButton;
    private GameObject typeDescButton;
    private GameObject cardBtn;
    private GameObject memoryBtn;
    
    
    private readonly string[] validScenes = { "MapScene", "BattleScene", "RandomEventScene", "MiniGameScene_01", "MiniGameScene_02", "MiniGameScene_03", "RestScene" };
    
    private readonly string[] uiDisplayScenes = { "MapScene", "BattleScene", "RestScene", "RandomEventScene" };
    
    // HP UI가 표시될 씬들
    private readonly string[] hpDisplayScenes = { "MapScene", "BattleScene", "RestScene" , "RandomEventScene", "MiniGameScene_01", "MiniGameScene_02", "MiniGameScene_03"};

    // 맵씬과 미니게임씬 목록 (TypeDescButton이 표시될 씬들)
    private readonly string[] typeDescButtonScenes = { "MapScene"};
    
    // MapScenePopupController 참조
    private MapScenePopupController popupController;
    #endregion
    #region ------------------------------초기화------------------------------
    private void Init()
    {
        DM = Manager.Instance.DataManager;
        
        // 팝업 컨트롤러 찾기
        popupController = FindObjectOfType<MapScenePopupController>();

        
        // 필요한 UI 컴포넌트 보장
        EnsureUIComponents();

        
        // 슬롯의 자식에서 bootyPanel 자동 탐색 (비활성 포함)
        infoPanels.Clear();
        foreach (var slot in slots)
        {
            if (slot != null)
            {
                var panel = slot.GetComponentInChildren<bootyPanel>(true);
                if (panel != null)
                {
                    infoPanels.Add(panel);
                    panel.Hide(); // 초기화 후 즉시 숨김
                }
            }
        }
        
        // 씬 전환 이벤트 구독
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }
    #endregion
    #region ------------------------------생명 주기------------------------------
    private void Start()
    {
        // 현재 씬 이름 가져오기
        string currentSceneName = SceneManager.GetActiveScene().name;
        
        // DataManager 초기화
        StartCoroutine(WaitForDataManager());
                
        // 버튼 참조 초기화
        InitButtonReferences();
        
        UpdateHPText();
        // 현재 씬에 따라 UI 표시 여부 설정
        UpdateUIVisibility(currentSceneName);
        
        // 스타트 씬에서 시작하는 것이 아니라면 즉시 DataManager 초기화 시도
        if (currentSceneName != "StartScene" && Manager.Instance != null && Manager.Instance.DataManager != null)
        {
            DM = Manager.Instance.DataManager;
            if (DM != null)
            {
                // 이벤트 등록
                DM.OnSanityChanged += UpdateHPText;
                DM.OnBootyChanged += UpdateBootySlots;
                                
                // 씬이 맵씬인 경우 즉시 UI 업데이트
                if (currentSceneName == "MapScene")
                {
                    // UI 강제 업데이트
                    if (text != null)
                    {
                        text.text = $"정신력 {DM.CurrentSanity} / {DM.MaxSanity}";
                    }
                    
                    UpdateBootySlots();
                }
            }
        }
    }
    private void OnDestroy()
    {
        // 이벤트 구독 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        
        // DataManager 이벤트 구독 해제
        if (DM != null)
        {
            DM.OnSanityChanged -= UpdateHPText;
            DM.OnBootyChanged -= UpdateBootySlots;
        }
        
        // 자원 해제
    }
    
    private void OnDisable()
    {
        // 게임오브젝트가 비활성화될 때도 이벤트 구독 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }
    #endregion
    private IEnumerator WaitForDataManager()
    {
        // DataManager가 준비될 때까지 대기 (최대 10초까지)
        float waitTime = 0f;
        float maxWaitTime = 10f;
        
        while ((Manager.Instance == null || Manager.Instance.DataManager == null) && waitTime < maxWaitTime)
        {
            waitTime += Time.deltaTime;
            yield return null;
        }

        if (waitTime >= maxWaitTime)
        {
            yield break;
        }

        DM = Manager.Instance.DataManager;
        
        if (DM != null)
        {
            string currentScene = SceneManager.GetActiveScene().name;            
            DM.OnSanityChanged += UpdateHPText;
            DM.OnBootyChanged += UpdateBootySlots;

            Init();
            InitButton();
            
            // 현재 씬이 미니게임 씬인지 확인
            bool isMiniGameScene = currentScene.StartsWith("MiniGameScene");
            
            // 미니게임 씬이 아닐 경우에만 DataManager.CurrentSanity 참조
            if (!isMiniGameScene)
            {
                if (DM.CurrentSanity > 0)
                {
                   UpdateHPText();
                }

                if (DM.Booties != null)
                {
                    UpdateBootySlots();
                }
            }
        }
    }


#region ------------------------------전리품------------------------------
    public void UpdateBootySlots()
    {
        // 현재 씬이 미니게임 씬인지 확인
        string currentScene = SceneManager.GetActiveScene().name;
        bool isMiniGameScene = currentScene.StartsWith("MiniGameScene");
        
        // 미니게임 씬인 경우 업데이트하지 않음
        if (isMiniGameScene)
            return;
            
        // DM이 null인지 확인
        if (DM == null)
            return;
        
        // slots가 null인지 확인
        if (slots == null || slots.Count == 0)
            return;
        
        // Booties가 null인지 확인
        if (DM.Booties == null)
            return;
        
        // 모든 슬롯 비활성화 (또는 초기화)
        foreach (var slot in slots)
        {
            if (slot == null) continue;
            
            slot.sprite = null;
            slot.gameObject.SetActive(false); // 일단 다 끄기
            
            // 기존 이벤트 리스너 제거
            EventTrigger eventTrigger = slot.gameObject.GetComponent<EventTrigger>();
            if (eventTrigger != null)
            {
                Destroy(eventTrigger);
            }
        }
        
        // 모든 패널 비활성화
        if (infoPanels != null)
        {
            foreach (var panel in infoPanels)
            {
                if (panel != null)
                    panel.Hide();
            }
        }

        // 유물 개수만큼 슬롯 설정
        for (int i = 0; i < DM.Booties.Count && i < slots.Count; i++)
        {
            var booty = DM.Booties[i];
            if (booty == null || slots[i] == null) continue;

            slots[i].sprite = booty.Icon;
            slots[i].gameObject.SetActive(true);

            // infoPanels[i]가 유효한지 확인 후 데이터 설정
            if (infoPanels != null && i < infoPanels.Count && infoPanels[i] != null)
            {
                // Booty 데이터가 유효한지 확인
                if (booty != null && !string.IsNullOrEmpty(booty.RewardName))
                {

                    // 정보 설정 전에 부티 패널 초기화 확인
                    // Desc 텍스트에 CleanLineBreaks 적용
                    string cleanDesc = TextUtility.CleanLineBreaks(booty.Desc ?? "");
                    infoPanels[i].SetInfo(booty.RewardName, cleanDesc);
                    AddSlotInfoTrigger(slots[i].gameObject, infoPanels[i]);
                }
            }
        }
    }
    
    private void AddSlotInfoTrigger(GameObject slotObject, bootyPanel panel)
    {
        if (slotObject == null || panel == null) return;
        
        // 기존 트리거 제거 (중복 방지)
        EventTrigger existingTrigger = slotObject.GetComponent<EventTrigger>();
        if (existingTrigger != null)
        {
            Destroy(existingTrigger);
        }
        
        EventTrigger trigger = slotObject.AddComponent<EventTrigger>();
        
        // 마우스 진입 이벤트
        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((data) => {
            if (panel != null) // null 체크 다시 한번
            {
                panel.Show();
            }
        });
        trigger.triggers.Add(enterEntry);
        
        // 마우스 이탈 이벤트
        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((data) => {
            if (panel != null) // null 체크 다시 한번
            {
                panel.Hide();
            }
        });
        trigger.triggers.Add(exitEntry);
    }
#endregion

#region ------------------------------정신력------------------------------
    public void UpdateHPText()
    {  
        text.text = $"정신력 {DM.CurrentSanity} / {DM.MaxSanity}";
    }
#endregion

#region ------------------------------버튼------------------------------
    private void InitButton()
    {
        Transform buttonsTransform = transform.Find("Buttons");
        if (buttonsTransform == null)
        {
            return;
        }
        
        Transform memoryBtnTransform = buttonsTransform.Find("MemoryBtn");
        Transform cardBtnTransform = buttonsTransform.Find("CardBtn");
        
        if (memoryBtnTransform == null || cardBtnTransform == null)
        {
            return;
        }
        
        Button memoryBtnComponent = memoryBtnTransform.GetComponent<Button>();
        Button cardBtnComponent = cardBtnTransform.GetComponent<Button>();
        
        // 시작 시 팝업 인스턴스가 있으면 비활성화 (권장 방법)
        if (memoryPanelPrefab != null && memoryPanelPrefab.activeSelf)
        {
            memoryPanelPrefab.SetActive(false);
        }
        
        if (cardPanelPrefab != null && cardPanelPrefab.activeSelf)
        {
            cardPanelPrefab.SetActive(false);
        }
        
        // 메모리 버튼 클릭 이벤트 설정
        if (memoryBtnComponent != null)
        {
            memoryBtnComponent.onClick.RemoveAllListeners(); // 기존 리스너 제거
            memoryBtnComponent.onClick.AddListener(() => {
            if (popupController != null && memoryPanelPrefab != null)
            {
               popupController.OpenPopUpUI(memoryBtnComponent, memoryPanelPrefab);
            }
            });
        }
        
        // 카드 버튼 클릭 이벤트 설정
        if (cardBtnComponent != null)
        {
            cardBtnComponent.onClick.RemoveAllListeners(); // 기존 리스너 제거
            cardBtnComponent.onClick.AddListener(() => {
            if (popupController != null && cardPanelPrefab != null)
            {
               popupController.OpenPopUpUI(cardBtnComponent, cardPanelPrefab);
            }
            });
        }
    }
    #endregion
    
#region ---------------------------- 씬 로드/언로드 ----------------------------
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // this가 null이거나 게임오브젝트가 파괴된 경우 처리하지 않음
        if (this == null || !gameObject)
            return;

        if (Array.IndexOf(validScenes, scene.name) >= 0)
        {
            // 유효한 씬일 경우만 에셋 로드
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            // DataManager가 초기화되었는지 확인하고 필요시 재초기화
            if (DM == null && Manager.Instance != null)
            {
                DM = Manager.Instance.DataManager;

                // 이벤트 리스너 등록
                if (DM != null)
                {
                    DM.OnSanityChanged += UpdateHPText;
                    DM.OnBootyChanged += UpdateBootySlots;
                }
            }

            // UI 표시 여부 설정
            UpdateUIVisibility(scene.name);
            // 팝업 컨트롤러 참조 업데이트
            popupController = FindObjectOfType<MapScenePopupController>();

            // 맵씬인 경우 추가 로그 출력
            if (scene.name == "MapScene")
            {

                // DataManager가 초기화되었으면 UI 강제 업데이트
                if (DM != null)
                {
                    // 미니게임 씬 체크 로직은 필요 없음 (맵씬이므로)
                    if (text != null)
                    {
                        text.text = $"정신력 {DM.CurrentSanity} / {DM.MaxSanity}";
                    }

                    UpdateBootySlots();
                }
            }
        }
        else
        {
            // 유효하지 않은 씬에서는 UI 비활성화
            gameObject.SetActive(false);
        }
    }
    
    private void OnSceneUnloaded(Scene scene)
    {
        // this가 null이거나 게임오브젝트가 파괴된 경우 처리하지 않음
        if (this == null || !gameObject)
            return;
            
        // 유효한 씬을 떠날 때 자원 해제 (필요한 경우)
        if (Array.IndexOf(validScenes, scene.name) >= 0)
        {
        }
    }
    #endregion

    // 씬에 따라 UI 요소 표시/숨김 설정
    private void UpdateUIVisibility(string sceneName)
    {
        // 1. 전리품/메모리/덱 UI 표시 여부 (맵, 배틀, 휴식씬)
        bool shouldDisplayCollectionUI = Array.IndexOf(uiDisplayScenes, sceneName) >= 0;
        
        // 2. typeDescButton 표시 여부 (맵과 미니게임 씬에서만)
        bool shouldDisplayTypeDesc = Array.IndexOf(typeDescButtonScenes, sceneName) >= 0;
        
        // 3. 유효한 씬 여부 (현재 씬이 상단바가 표시되는 씬인지)
        bool isValidScene = Array.IndexOf(validScenes, sceneName) >= 0;
        
        // 4. HP 표시 여부 (hpDisplayScenes에 정의된 씬에서만)
        bool shouldDisplayHP = Array.IndexOf(hpDisplayScenes, sceneName) >= 0;
        
        // DataManager가 초기화되었는지 확인
        bool isDataManagerInitialized = (DM != null);
        
        // 1. 전리품 UI 설정 (collection UI)
        if (shouldDisplayCollectionUI && isDataManagerInitialized && DM.Booties != null)
        {
            // 각 슬롯이 보이도록 설정
            foreach (var slot in slots)
            {
                if (slot != null)
                {
                    slot.gameObject.SetActive(true);
                }
            }
            
            // 슬롯 내용 업데이트
            UpdateBootySlots();
        }
        else
        {
            // 전리품을 표시하지 않는 씬에서는 모든 슬롯 비활성화
            foreach (var slot in slots)
            {
                if (slot != null)
                {
                    slot.gameObject.SetActive(false);
                }
            }
        }
        
        if (cardBtn != null)
        {
            cardBtn.SetActive(shouldDisplayCollectionUI);
        }
        
        if (memoryBtn != null)
        {
            memoryBtn.SetActive(shouldDisplayCollectionUI);
        }
        
        if (text != null && text.transform.parent != null) // 체력바
        {
            // HP UI는 hpDisplayScenes에 정의된 씬에서만 표시
            text.transform.parent.gameObject.SetActive(shouldDisplayHP);
        }
        
        if (settingButton != null)
        {
            settingButton.SetActive(isValidScene);
        }
        
        if (typeDescButton != null)
        {
            typeDescButton.SetActive(shouldDisplayTypeDesc);
        }
    }

    private void InitButtonReferences()
    {
        // Buttons 게임오브젝트 내의 버튼들 참조 가져오기
        if (buttonsParent != null)
        {
            settingButton = buttonsParent.Find("SettingButton")?.gameObject;
            typeDescButton = buttonsParent.Find("TypeDescButton")?.gameObject;
            cardBtn = buttonsParent.Find("CardBtn")?.gameObject;
            memoryBtn = buttonsParent.Find("MemoryBtn")?.gameObject;
        }
        else
        {
            // Buttons 부모 객체 찾기 시도
            buttonsParent = transform.Find("Buttons");
            if (buttonsParent != null)
            {
                settingButton = buttonsParent.Find("SettingButton")?.gameObject;
                typeDescButton = buttonsParent.Find("TypeDescButton")?.gameObject;
                cardBtn = buttonsParent.Find("CardBtn")?.gameObject;
                memoryBtn = buttonsParent.Find("MemoryBtn")?.gameObject;
            }
        }
    }

    // UI 컴포넌트 초기화 로직 추가
    private void EnsureUIComponents()
    {
        // 메모리 패널 프리팹에 RectTransform 확인 및 추가
        if (memoryPanelPrefab != null && memoryPanelPrefab.GetComponent<RectTransform>() == null)
        {
            memoryPanelPrefab.AddComponent<RectTransform>();
        }
        
        // 카드 패널 프리팹에 RectTransform 확인 및 추가
        if (cardPanelPrefab != null && cardPanelPrefab.GetComponent<RectTransform>() == null)
        {
            cardPanelPrefab.AddComponent<RectTransform>();
        }
    }
}

