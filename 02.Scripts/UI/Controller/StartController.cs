using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;


public class StartController : ControllerUI
{
    #region --------------------------- 변수 및 구조체 ---------------------------
    public StartUI startUI;
    private IStartView _view;

    [SerializeField] private bool isFirst;
    private Dictionary<string, Action> buttonActions = new();
    private SerializableDic<int, EndingSO> endingList = new();
    private List<MiniGameDataSO> miniGameList = new();
    private List<SpriteAtlas> buttonAtlases = new();
    private string selectedFaction;
    private string selectedCard;
    private bool isProcessingClick = false;
    private BaseUI currentStartUI; // 현재 활성화된 UI 트래킹

    // UI 이미지 속성을 저장하기 위한 구조체
    [System.Serializable]
    private struct UIImageProperties
    {
        public Vector2 Position;  // posX, posY
        public Vector2 Size;      // width, height
        public Vector3 Scale;     // scaleX, scaleY, scaleZ
    }

    // UI 이미지별 속성 정의 (위치, 크기, 스케일)
    private readonly Dictionary<string, UIImageProperties> imageProperties = new()
    {

        {
            "hexagram",
            new UIImageProperties {
                Position = new Vector2(0f, 0f),
                Size = new Vector2(200f, 200f),
                Scale = new Vector3(1.0f, 1.0f, 1.0f)
            }
        },
        {
            "flag",
            new UIImageProperties {
                Position = new Vector2(0f, 0f),
                Size = new Vector2(200f, 200f),
                Scale = new Vector3(1.0f, 1.0f, 1.0f)
            }
        },
    };
    #endregion
    #region --------------------------- 초기화 및 시작 ---------------------------
    private void Awake()
    {
        _view = startUI;
    }
    protected override async void Start()
    {
        InitPanel();
        isFirst = IsFirst();
        base.Start();

        // 엔딩과 미니게임 SO 로드
        await LoadEndingSOs();
        await LoadMiniGameSOs();
        
        await LoadDatas();

        _view.Initialize();
        startUI.OnClickButton += OnButtonClicked;
        RegisterButtonActions();

        // 시작 시 타이틀 화면으로 이동 (WaitForInput은 페이드 완료 후 자동 시작)
        ShowUI(startUI[PanelKey.Title]);

        bool hasSaveData = Manager.Instance.SaveManager.HasSaveFile();
        bool HasPermanentSaveData = Manager.Instance.SaveManager.HasPermanentSaveFile();
        if (hasSaveData)
        {
            await Manager.Instance.SaveManager.LoadGame();
        }
        else if (!hasSaveData && HasPermanentSaveData)
        {
            await Manager.Instance.SaveManager.LoadCredits();
        }
    }
    #endregion
    #region --------------------------- 어드레서블 로드 ---------------------------

    private async Task LoadDatas()
    {
        await LoadRewardDatas();
        await LoadMemoryDatas();
    }
    private async Task LoadRewardDatas()
    {
        var bootyIcons = await Manager.Instance.AddressableManager.Load<SpriteAtlas>("BootyIcons");
        var rewardImages = await Manager.Instance.AddressableManager.Load<SpriteAtlas>("RewardImages");
        var rewardDatas = await Manager.Instance.AddressableManager.GetHandleResultList<RewardSO>("Reward");

        foreach (var data in rewardDatas)
        {
            if (data.TypeValue != RewardType.Card)
            {
                data.Image = rewardImages.GetSprite($"{data.FileName}");
            }

            if (data.TypeValue == RewardType.Booty)
            {
                data.Icon = bootyIcons.GetSprite($"{data.FileName}");
            }
        }
    }
    private async Task LoadMemoryDatas()
    {
        var memoryImages = await Manager.Instance.AddressableManager.Load<SpriteAtlas>("MemoryImages");
        var memoryDatas = await Manager.Instance.AddressableManager.GetHandleResultList<MemorySO>("MemoryData");
        
        foreach (var data in memoryDatas)
        {
            if (data.Image == null)
            {
                data.Image = memoryImages.GetSprite($"{data.FileName}");
            }
        }
    }
    private async System.Threading.Tasks.Task LoadEndingSOs()
    {
        var handle = Addressables.LoadAssetsAsync<EndingSO>("Ending", null);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            endingList.dataList.Clear();
            foreach (var ending in handle.Result)
            {
                endingList[ending.Index] = ending;
            }
        }
    }

    private async System.Threading.Tasks.Task LoadMiniGameSOs()
    {
        var handle = Addressables.LoadAssetsAsync<MiniGameDataSO>("MiniGame", null);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            miniGameList = handle.Result.ToList();
        }
    }
    #endregion
    #region --------------------------- 버튼 --------------------------
    public void OnButtonClicked(string key)
    {
        if (isProcessingClick) return;
        ProcessButtonClick(key);
    }

    private void ProcessButtonClick(string key)
    {
        isProcessingClick = true;

        if (buttonActions.TryGetValue(key, out var action))
        {
            action?.Invoke();
        }
        isProcessingClick = false;
    }

    private void RegisterButtonActions()
    {
        buttonActions["Select_Start"] = CheckSaveData;
        buttonActions["Select_Mini"] = MiniGame;
        buttonActions["Select_Ending"] = Ending;
        buttonActions["Select_Return"] = ReturnToStartUI;

        buttonActions["Camp_Hexa"] = () => SelectFaction("Hexa");
        buttonActions["Camp_Flag"] = () => SelectFaction("Flag");
        buttonActions["Camp_Return"] = ReturnToSelect;

        buttonActions["Char_Start"] = GameStart;
        buttonActions["Char_Return"] = ReturnToCamp;

        buttonActions["Credits_Return"] = ReturnCredits;

        buttonActions["CloseLockPopup"] = () => _view.ClosePopup("lockPopup");
        buttonActions["LoadBtn"] = LoadGame;
        buttonActions["NewGameBtn"] = InitCamp;
        buttonActions["CloseLoadPopup"] = () => _view.ClosePopup("loadPopup");
    }
    #endregion
    #region --------------------------- 게임 시작 ---------------------------
    private void GameStart()
    {
        if (Manager.Instance.DataManager.CharacterData == null)
        {
            InitializeCharacterBasedOnFaction();
            AssetLoad();
        }
        else
        {
            AssetLoad();
        }
    }

    private void AssetLoad() // 필요한 데이터 전달
    {
        var labelMapping = new AssetLabelMapping[]
        {
            new AssetLabelMapping
            {
                label = "Character",
                assetType = AssetType.CharacterSO
            },
            new AssetLabelMapping
            {
                label = "BGM",
                assetType = AssetType.AudioClip
            },
            new AssetLabelMapping
            {
                label = "SFX",
                assetType = AssetType.AudioClip
            },
            new AssetLabelMapping
            {
                label = "MemoryData",
                assetType = AssetType.MemorySO
            },
            new AssetLabelMapping
            {
                label = "Card",
                assetType = AssetType.CardSO
            },
            new AssetLabelMapping
            {
                label = "Reward",
                assetType = AssetType.RewardSO
            }

        };

        var loadData = CreateLoadingSceneData(labelMapping, "MapScene", new object[] { });

        LoadScene("LoadingScene", loadData);
    }
    private void LoadGame()
    {
        AssetLoad();
    }
    private void CheckSaveData()
    {
        // 세이브 파일이 있고, 데이터가 유효한 경우에만 로드 팝업을 표시
        if (Manager.Instance.SaveManager.HasSaveFile() && Manager.Instance.DataManager.CharacterData != null)
        {
            _view.LoadPopup();
        }
        else
        {
            InitCamp();
        }
    }
    #endregion
    #region --------------------------- 패널 전환 ---------------------------
    private IEnumerator ShowCreditsWithDelay(string text)
    {
        ShowUI(startUI[PanelKey.Credits]);
        _view.CreditsText(text);

        // 페이드 효과가 완료될 때까지 잠시 대기 (시간 단축)
        yield return new WaitForSeconds(fadeDuration + 0.1f);

        // 이제 패널 표시
        if (startUI.creditPanelPool != null && startUI.creditPanelPool.parent != null)
        {
            startUI.creditPanelPool.parent.gameObject.SetActive(true);
            // 패널 위치 재조정
            _view.ReorganizePanels();
        }
    }
    public void Credits(string text)
    {
        StartCoroutine(ShowCreditsWithDelay(text));
    }

    private void ReturnToStartUI() // 타이틀로 되돌아가기
    {
        // 모든 패널 초기화
        // 캐릭터 선택 패널 명시적으로 비활성화
        if (startUI[PanelKey.CharacterSelect] != null)
        {
            startUI[PanelKey.CharacterSelect].gameObject.SetActive(false);
        }

        // 크레딧 패널 초기화
        if (startUI.creditPanelPool != null)
        {
            startUI.creditPanelPool.ReturnAllPanels();
        }

        // 카드 상태 초기화
        if (startUI.hexagram != null) startUI.hexagram.ResetCard();
        if (startUI.flag != null) startUI.flag.ResetCard();

        // 타이틀 화면으로 이동 (WaitForInput은 페이드 완료 후 자동 시작)
        ShowUI(startUI[PanelKey.Title]);
    }
    private void InitCamp()
    {
        Manager.Instance.SaveManager.ClearAllData();
        _view.ClosePopup("loadPopup");

        // 캐릭터 선택 패널 명시적으로 비활성화
        if (startUI[PanelKey.CharacterSelect] != null)
        {
            startUI[PanelKey.CharacterSelect].gameObject.SetActive(false);
        }

        ShowUI(startUI[PanelKey.Camp]);
        _view.SetBackGround("Sub");
        _view.InitObj();

        _view.ResetCard(startUI.campHexagram, startUI.campFlag);
    }
    private void ReturnToCamp()
    {
        // 캐릭터 선택 패널 명시적으로 비활성화
        if (startUI[PanelKey.CharacterSelect] != null)
        {
            startUI[PanelKey.CharacterSelect].gameObject.SetActive(false);
        }

        ShowUIWithoutFade(startUI[PanelKey.Camp]);
        startUI.SetBackGround("Sub");
        startUI.InitObj();

        switch (selectedCard)
        {
            case "Hexagram":
                CardReverseFlipAnim(startUI.campHexagram, startUI.campFlag);
                break;
            case "Flag":
                CardReverseFlipAnim(startUI.campFlag, startUI.campHexagram);
                break;
        }
    }

    private void ReturnToSelect()
    {
        // 캐릭터 선택 패널 명시적으로 비활성화
        if (startUI[PanelKey.CharacterSelect] != null)
        {
            startUI[PanelKey.CharacterSelect].gameObject.SetActive(false);
        }

        ShowUI(startUI[PanelKey.Select]);
        _view.SetBackGround("Sub");
        
        _view.ResetCard(startUI.hexagram, startUI.flag);
    }

    private void ReturnCredits()
    {
        // 크레딧 패널 초기화
        if (startUI.creditPanelPool != null)
        {
            startUI.creditPanelPool.ReturnAllPanels();
        }

        ShowUI(startUI[PanelKey.Select]);
    }

    private void MiniGame()
    {
        ShowUI(startUI[PanelKey.Credits]);
        _view.SetBackGround("Sub");

        // Credits 호출 - 이 내부에서 페이드 효과 후 패널 표시
        Credits("함정 파훼");

        // 페이드 효과가 완료된 후 CreateMiniGamePanels 호출
        StartCoroutine(DelayedCreateMiniGamePanels());
    }
    private void Ending()
    {
        ShowUI(startUI[PanelKey.Credits]);
        _view.SetBackGround("Sub");

        // Credits 호출 - 이 내부에서 페이드 효과 후 패널 표시
        Credits("엔딩 목록");

        // EndingCombiner 인스턴스 찾기
        EndingCombiner endingCombiner = FindObjectOfType<EndingCombiner>();
        if (endingCombiner != null)
        {
            // 이미 본 엔딩만 언락 상태로 유지
            endingCombiner.UnlockViewedEndings();
        }

        endingList.dataList.Sort((a, b) => a.Value.Index.CompareTo(b.Value.Index));

        // 페이드 효과가 완료된 후 CreateEndingPanels 호출
        StartCoroutine(DelayedCreateEndingPanels());
    }
    private IEnumerator DelayedCreateMiniGamePanels()
    {
        // 페이드 효과가 완료될 때까지 잠시 대기 (시간 단축)
        yield return new WaitForSeconds(fadeDuration + 0.1f);

        // 패널 생성
        startUI.CreateMiniGamePanels(miniGameList, LoadMiniGame);
    }
    // 페이드 효과 없이 UI 직접 전환을 위한 메서드
    private void ShowUIWithoutFade(BaseUI targetUI)
    {
        if (targetUI == null) return;

        // 현재 UI 비활성화
        if (currentStartUI != null)
        {
            currentStartUI.Hide();
            currentStartUI.gameObject.SetActive(false);
        }

        // 새 UI 바로 활성화
        targetUI.gameObject.SetActive(true);
        targetUI.Show();
        currentStartUI = targetUI;
    }

    #endregion
    #region --------------------------- 크레딧 로드 ---------------------------
    private void LoadMiniGame(int index)
    {
        Addressables.LoadAssetsAsync<MiniGameDataSO>("MiniGame", null).Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                var allMiniGames = handle.Result.ToList();
                var data = allMiniGames.Find(x => x.index == index);

                if (data != null)
                {
                    var mappings = new AssetLabelMapping[]
                    {
                        new AssetLabelMapping { label = "MiniGame", assetType = AssetType.MiniGameDataSO }
                    };

                    Manager.Instance.AnalyticsManager.LogEvent(EventName.MAIN_MENU_FEATURE, EventParam.CATEGORY_NAME, data.title);

                    // 세 번째 파라미터로 StartScene에서 시작했다는 정보 전달
                    var loadData = CreateLoadingSceneData(mappings, data.sceneName, new object[] { index, true, true });
                    LoadScene("LoadingScene", loadData);
                }
            }
        };
    }
    private void LoadEndingScene(int endingIndex)
    {
        var mappings = new AssetLabelMapping[]
        {
            new AssetLabelMapping { label = "Ending", assetType = AssetType.EndingSO }
        };

        var loadData = SceneBase.CreateLoadingSceneData(mappings, "EndingScene", endingIndex);
        Manager.Instance.AnalyticsManager.LogEvent(EventName.MAIN_MENU_FEATURE, EventParam.CATEGORY_NAME, endingList[endingIndex].EndingName);
        LoadScene("LoadingScene", loadData);
    }
    #endregion
    #region ---------------------------- 진영 선택 ----------------------------
    private void SelectFaction(string faction)
    {
        // 페이드 효과 없이 직접 UI 전환
        ShowUIWithoutFade(startUI[PanelKey.CharacterSelect]);
        selectedFaction = faction;

        // 진영에 맞는 배경 설정
        if (faction == "Hexa")
        {
            _view.SetBackGround("Hexa");
            CardFlipAnim(startUI.hexagram, startUI.flag);
            StartCoroutine(_view.MoveObj(startUI.mariaPrefab, 1.96f));
            selectedCard = "Hexagram";
        }
        else if (faction == "Flag")
        {
            _view.SetBackGround("Flag");
            CardFlipAnim(startUI.flag, startUI.hexagram);
            StartCoroutine(_view.MoveObj(startUI.eliosPrefab, -2.18f));
            selectedCard = "Flag";
        }
    }

    #endregion
    #region --------------------------- 카드 애니메이션 ---------------------------
    private void CardFlipAnim(CampCard selectedCard, CampCard unselectedCard)
    {
        unselectedCard.SetActive(false);
        StartCoroutine(selectedCard.Flip());

    }
    private void CardReverseFlipAnim(CampCard selectedCard, CampCard unselectedCard)
    {
        unselectedCard.ResetCard();
        StartCoroutine(selectedCard.ReverseFlip());
    }
    #endregion
    #region --------------------------- 패널 ---------------------------
    private void InitPanel()
    {
        foreach (var panel in startUI.panels.dataList)
        {
            allUIs.Add(panel.Value);
        }

        // UI 요소 초기화 시 속성 적용
        InitializeUIElements();
    }
    // 패널 이름에 따른 배경 설정
    private void SetBackgroundForPanel(string panelName)
    {
        if (panelName == PanelKey.Title.ToString())
        {
            _view.SetBackGround("Title");
        }
        else if (panelName == PanelKey.Select.ToString() ||
                 panelName == PanelKey.Camp.ToString() ||
                 panelName == PanelKey.Credits.ToString())
        {
            _view.SetBackGround("Sub");
        }
    }
    // BaseUI로부터 패널 이름 가져오기
    private string GetPanelNameFromUI(BaseUI targetUI)
    {
        if (targetUI == null || startUI.panels == null) return "";

        foreach (var pair in startUI.panels.dataList)
        {
            if (pair.Value == targetUI)
            {
                return pair.Key.ToString();
            }
        }

        return "";
    }
    private IEnumerator DelayedCreateEndingPanels()
    {
        // 페이드 효과가 완료될 때까지 잠시 대기 (시간 단축)
        yield return new WaitForSeconds(fadeDuration + 0.1f);

        // 패널 생성
        startUI.CreateEndingPanels(endingList, index => LoadEndingScene(index));
    }
    #endregion
    #region --------------------------- 페이드 효과 ---------------------------
    // 커스텀 페이드 전환을 구현하는 코루틴
    protected override IEnumerator FadeUITransition(BaseUI targetUI)
    {
        // 페이드 캔버스 활성화 - 완전히 투명한 상태로 시작
        fadeCanvasGroup.alpha = 0f;
        fadeCanvas.SetActive(true);

        // 페이드 인 (투명 -> 불투명) - 화면이 점점 어두워짐
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsed / fadeDuration);
            fadeCanvasGroup.alpha = normalizedTime;
            yield return null;
        }
        fadeCanvasGroup.alpha = 1f; // 완전히 불투명하게 설정

        // 현재 UI 비활성화
        if (currentStartUI != null)
        {
            currentStartUI.Hide();
            currentStartUI.gameObject.SetActive(false);
        }

        // 새 UI 활성화
        targetUI.gameObject.SetActive(true);
        targetUI.Show();
        currentStartUI = targetUI;

        // 현재 패널에 맞는 배경 설정 (화면이 완전히 검은색일 때)
        string panelName = GetPanelNameFromUI(targetUI);
        if (!string.IsNullOrEmpty(panelName))
        {
            SetBackgroundForPanel(panelName);
        }

        // 약간의 지연을 주어 배경이 완전히 변경되도록 함
        yield return new WaitForSeconds(0.05f);

        // 페이드 아웃 (불투명 -> 투명) - 화면이 점점 밝아짐
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsed / fadeDuration);
            fadeCanvasGroup.alpha = 1f - normalizedTime;
            yield return null;
        }
        fadeCanvasGroup.alpha = 0f; // 완전히 투명하게 설정

        // 페이드 완료 후 캔버스 비활성화
        fadeCanvas.SetActive(false);
        isTransitioning = false;
        fadeCoroutine = null;

        // 페이드가 완료된 후 UI에 따른 추가 작업 (타이틀 화면이면 입력 감지 시작)
        if (panelName == PanelKey.Title.ToString())
        {
            StartCoroutine(WaitForInput());
        }
    }
    #endregion
    #region --------------------------- 설명 ---------------------------
    IEnumerator WaitForInput() // 타이틀에서 입력시에 패널 변경
    {
        // 전환 중에는 입력을 무시하기 위한 플래그
        bool isProcessingKeyInput = false;

        // 키 입력 감지 루프
        while (true)
        {
            // 이미 처리 중이거나 전환 중이면 무시
            if (isProcessingKeyInput || base.isTransitioning)
            {
                yield return null;
                continue;
            }

            // 키 입력 감지
            if (Input.anyKeyDown)
            {
                isProcessingKeyInput = true;
                if (isFirst)
                {
                    PlayerPrefs.SetInt("Confirm", 1);
                    ShowUI(startUI[PanelKey.Desc]);
                    yield return StartCoroutine(TextUtility.AnimatorText(startUI.descText, "여러분은 지금부터 선과 악의 경계를 새롭게 정의하는 이야기를 시작하게 됩니다.\n흔히 마족은 악, 용사는 선이라고 생각하기 쉽지만,\n이 이야기에서는 그러한 고정관념에서 벗어나게 될 것입니다.\n그렇다면 마족이 선하고 용사가 악한 존재일까요?\n그것 또한 아닙니다.\n여러분은 이야기를 써 내려가며 기억의 조각을 얻게 되고,\n선과 악이 무엇인지 그리고 누구인지 스스로 결정하게 될 것입니다.\n우리는 전능한 존재를 신이라고 부릅니다.\n이제 여러분은 이야기를 창조하며 자신의 전능함을 보여주게 될 것입니다.\n작은 선택 하나하나가 이 이야기를 완성시키는 중요한 요소가 될 것입니다.\n더욱 완벽한 엔딩을 만들어가기 위해 여러분의 도움이 필요합니다.",
                         () => { ShowUI(startUI[PanelKey.Select]); }));
                }
                else
                {
                    ShowUI(startUI[PanelKey.Select]);
                }

                // 루프 종료
                yield break;
            }
            yield return null;
        }
    }
    private bool IsFirst() // 초회인지 판별
    {
        return PlayerPrefs.GetInt("Confirm", 0) == 0;
    }

    #endregion
    #region --------------------------- 진영 선택 후 캐릭터 초기화 ---------------------------  
    private async void InitializeCharacterBasedOnFaction()
    {
        if (string.IsNullOrEmpty(selectedFaction))
        {
            return;
        }

        var allChars = new List<CharacterSO>();
        var charHandle = Addressables.LoadAssetsAsync<CharacterSO>(
            "Character",
            so => allChars.Add(so)
        );
        await charHandle.Task;
        if (charHandle.Status != AsyncOperationStatus.Succeeded || allChars.Count == 0)
        {
            return;
        }

        // 진영에 따라 다른 캐릭터 ID 설정
        int characterId = selectedFaction == "Hexa" ? 1 : 2; // 헥사는 마리아(1), 플래그는 엘리오스(2)라고 가정

        var selectedSO = allChars.FirstOrDefault(c => c.Index == characterId);
        if (selectedSO == null)
        {
            return;
        }
        if (Manager.Instance.DataManager.CharacterData != null)
        {
            return;
        }
        // 선택된 캐릭터 데이터 초기화
        await Manager.Instance.DataManager.InitializeCharacter(selectedSO);
    }
    #endregion
    #region --------------------------- UI 요소 ---------------------------
    // UI 이미지에 미리 정의된 속성 적용 메서드
    private void ApplyImageProperties(string imageName, RectTransform imageTransform)
    {
        if (imageProperties.TryGetValue(imageName, out var properties))
        {
            // 위치 적용
            imageTransform.anchoredPosition = properties.Position;

            // 크기 적용
            if (properties.Size != Vector2.zero)
            {
                imageTransform.sizeDelta = properties.Size;
            }

            // 스케일 적용
            imageTransform.localScale = properties.Scale;
        }
    }

    // UI 요소 초기화 시 속성 적용
    private void InitializeUIElements()
    {
        // hexagram 카드 속성 적용
        if (startUI.hexagram != null && startUI.hexagram.transform is RectTransform hexagramTrans)
        {
            ApplyImageProperties("hexagram", hexagramTrans);
        }

        // flag 카드 속성 적용
        if (startUI.flag != null && startUI.flag.transform is RectTransform flagTrans)
        {
            ApplyImageProperties("flag", flagTrans);
        }

        // 필요한 경우 다른 UI 요소들에도 적용
    }
    public override void ShowUI(BaseUI targetUI)
    {
        if (isTransitioning || targetUI == null) return;
        isTransitioning = true;

        // 이전에 실행 중이던 페이드 코루틴 시작
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        // 커스텀 페이드 전환 코루틴 시작
        fadeCoroutine = StartCoroutine(FadeUITransition(targetUI));
    }
    #endregion
}

