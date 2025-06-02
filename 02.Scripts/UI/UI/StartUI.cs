using System;
using System.Collections;
using System.Collections.Generic;
using Live2D.Cubism.Core;
using Live2D.Cubism.Rendering;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.U2D;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public interface IStartView
{
    void Initialize();

    //Popup
    void LoadPopup();
    void ClosePopup(string objName);

    void CreditsText(string text);

    void ReorganizePanels();

    void SetBackGround(string target);

    //Live2D
    void InitObj();
    IEnumerator MoveObj(CubismModel model, float value);

    void ResetCard(CampCard card1, CampCard card2);
}


public class StartUI : BaseUI, IStartView
{
    #region ---------------------------------- 변수 ----------------------------------
    // 패널
    public SerializableDic<PanelKey, BaseUI> panels = new();
    public BaseUI this[PanelKey key] => panels[key];

    // 버튼
    [Header("Button")]
    public SerializableDic<string, Button> buttons = new();
    public SerializableDic<string, Image> buttonsSpriteAtlas = new();
    private SpriteAtlas _buttonAtlas;
    public Action<string> OnClickButton;


    [Header("Live2D")]
    public CubismModel mariaPrefab;
    public CubismModel eliosPrefab;

    [Header("Text")]
    public TextMeshProUGUI descText;
    public TextMeshProUGUI creditsText;
    [SerializeField] private TextMeshProUGUI popupText;
    [SerializeField] private TextMeshProUGUI startText;
    [SerializeField] private TextMeshProUGUI miniGameText;
    [SerializeField] private TextMeshProUGUI endingText;

    [Header("Card")]
    public CampCard campFlag;
    public CampCard campHexagram;
    public CampCard flag;
    public CampCard hexagram;

    [Header("Credits")]
    [SerializeField] private CreditPanel creditsButton;
    [SerializeField] private TextMeshProUGUI creditsPanelText;
    [SerializeField] private Transform creditsPanel;

    [Header("BackGround")]
    [SerializeField]
    private SerializableDic<string, GameObject> backGround;

    public CreditPanelPool creditPanelPool;
    [SerializeField] private GameObject lockPopup;
    [SerializeField] private GameObject loadPopup;
    #endregion
    #region ---------------------------------- 초기화 ----------------------------------
    public override async void Initialize()
    {
        base.Initialize();

        InitObj();

        // 버튼 액션 설정
        foreach (var btn in buttons.dataList)
        {
            string key = btn.Key;
            Button button = btn.Value;
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => OnClickButton(key));
                Manager.Instance.CursorManager.AddCursorEvents(button);
            }
        }

        // 어드레서블로 아틀라스 로드
        var handle = Addressables.LoadAssetAsync<SpriteAtlas>("Buttons");
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _buttonAtlas = handle.Result;
            SetButtonSprites();
        }

        SetText();

        if (creditPanelPool != null && creditPanelPool.parent != null)
        {
            PreserveChildState(creditPanelPool.parent);
        }
    }
    #endregion
    #region ---------------------------------- 이미지 및 배경 ----------------------------------
    private void SetButtonSprites()
    {
        if (_buttonAtlas == null) return;

        foreach (var img in buttonsSpriteAtlas.dataList)
        {
            string key = img.Key;
            Image image = img.Value;
            if (image != null)
            {
                image.sprite = _buttonAtlas.GetSprite(key);
            }
        }
    }

    public void SetBackGround(string target)
    {
        foreach (var bg in backGround.dataList)
        {
            if (bg.Value != null)
                bg.Value.SetActive(false);
        }
        backGround[target]?.SetActive(true);
    }
    #endregion
    #region ---------------------------------- Obj ----------------------------------
    public void InitObj()
    {
        mariaPrefab.gameObject.SetActive(false);
        eliosPrefab.gameObject.SetActive(false);

        eliosPrefab.transform.position = new Vector3(-4f, -0.5f, 3);
        mariaPrefab.transform.position = new Vector3(4f, -0.8f, 3);

        // 카드 상태 초기화
        if (flag != null) flag.ResetCard();
        if (hexagram != null) hexagram.ResetCard();

        // 카드 활성화 상태 확인
        if (flag != null) flag.SetActive(true);
        if (hexagram != null) hexagram.SetActive(true);
    }
    public IEnumerator MoveObj(CubismModel model, float value) // Live2D 프리팹 움직임
    {
        model.gameObject.SetActive(true);
        CubismRenderController controller = model.GetComponent<CubismRenderController>();

        Vector3 startPos = model.transform.position;
        Vector3 endPos = new(value, startPos.y, startPos.z);
        float duration = 0.15f;
        float elapsed = 0f;
        float initOpacity = .1f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            model.transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            controller.Opacity = initOpacity + (1 - initOpacity) * Mathf.Clamp01(elapsed / duration);

            yield return null;
        }
        model.transform.position = endPos;
    }
    public void ResetCard(CampCard card1, CampCard card2)
    {
        if (card1 != null) card1.ResetCard();
        if (card2 != null) card2.ResetCard();
    }
    #endregion
    #region ---------------------------------- 텍스트 설정 ----------------------------------
    private void SetText()
    {
        startText.text = "스테이지 입장";
        miniGameText.text = "함정 파훼";
        endingText.text = "엔딩 관람";
    }
    public void CreditsText(string text)
    {
        // 텍스트 설정 전에 초기화
        if (creditsText != null)
        {
            creditsText.text = "";

            // 약간의 딜레이 후 텍스트 설정 (UI 업데이트를 위해)
            StartCoroutine(SetTextWithDelay(text));
        }
    }

    private IEnumerator SetTextWithDelay(string text)
    {
        // 한 프레임 대기 (UI 갱신을 위해)
        yield return null;

        // 이제 텍스트 설정
        creditsText.text = text;
    }
    #endregion
    #region ---------------------------------- 패널 ----------------------------------
    public void CreateMiniGamePanels(List<MiniGameDataSO> miniGames, Action<int> onClick)
    {
        creditPanelPool.ReturnAllPanels();

        // 패널 부모 오브젝트 활성화 상태 확인 및 조정
        if (creditPanelPool.parent != null && !creditPanelPool.parent.gameObject.activeSelf)
        {
            creditPanelPool.parent.gameObject.SetActive(true);
        }


        // 인덱스 순서로 정렬
        var sortedMiniGames = miniGames.OrderBy(mg => mg.index).ToList();

        // 패널들을 담을 리스트
        List<GameObject> activePanels = new List<GameObject>();

        var CreditManager = Manager.Instance.CreditManager;

        for (int i = 0; i < sortedMiniGames.Count; i++)
        {
            var data = sortedMiniGames[i];
            int index = data.index;
            string title = data.title;
            bool isLocked = Manager.Instance.CreditManager.IsMiniGameLocked(index);

            var panelObj = creditPanelPool.GetPanel();
            if (panelObj == null) continue;

            var panel = panelObj.GetComponent<CreditPanel>();
            panel.isLocked = isLocked;
            string displayText = isLocked ? $"{index}. ???" : $"{index}. {title}";
            panel.SetText(displayText);

            // 리스트에 추가
            activePanels.Add(panelObj);

            int capturedIndex = index; // 클로저에서 사용할 인덱스를 명시적으로 캡처
            panel.SetButtonAction(() =>
            {
                if (panel.isLocked)
                {
                    LockPopup();
                }
                else
                {
                    onClick?.Invoke(capturedIndex);
                }
            });

            // 잠긴 패널이더라도 항상 활성화 상태 유지
            panelObj.SetActive(true);
        }

        // 패널 위치 수동 조정 (중요!)
        float panelHeight = 90f; // 패널 높이 + 간격
        for (int i = 0; i < activePanels.Count; i++)
        {
            RectTransform rectTransform = activePanels[i].GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                // Y 위치만 변경하여 수직으로 배치
                Vector2 position = rectTransform.anchoredPosition;
                position.y = -i * panelHeight;
                rectTransform.anchoredPosition = position;

                // 패널을 명시적으로 다시 활성화
                activePanels[i].SetActive(false);
                activePanels[i].SetActive(true);
            }
        }

        // 레이아웃 강제 업데이트
        if (creditPanelPool.parent != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(creditPanelPool.parent as RectTransform);
        }

        // 추가: CreditPanelPool의 UpdateLayout 메서드 호출
        creditPanelPool.UpdateLayout();
    }

    public void CreateEndingPanels(SerializableDic<int, EndingSO> endings, System.Action<int> onClick)
    {
        creditPanelPool.ReturnAllPanels();

        if (creditPanelPool.parent != null && !creditPanelPool.parent.gameObject.activeSelf)
        {
            creditPanelPool.parent.gameObject.SetActive(true);
        }

        var soList = new List<EndingSO>();
        foreach (var kv in endings.dataList)
        {
            soList.Add(kv.Value);
        }
        soList.Sort((a, b) => a.Index.CompareTo(b.Index));

        List<GameObject> activePanels = new List<GameObject>();

        // EndingManager를 통해 잠금 상태 확인
        var CreditManager = Manager.Instance.CreditManager;

        for (int i = 0; i < soList.Count; i++)
        {
            var so = soList[i];
            int capturedIdx = so.Index;
            string endingName = so.EndingName;
            bool isLocked = CreditManager.IsEndingLocked(capturedIdx); // EndingManager로 잠금 상태 확인

            var panelObj = creditPanelPool.GetPanel();
            if (panelObj == null) continue;

            var panel = panelObj.GetComponent<CreditPanel>();
            panel.isLocked = isLocked;

            string displayText = isLocked ? $"{capturedIdx}. ???" : $"{capturedIdx}. {endingName}";
            panel.SetText(displayText);

            activePanels.Add(panelObj);

            panel.SetButtonAction(() =>
            {
                if (panel.isLocked)
                {
                    LockPopup();
                }
                else
                {
                    onClick?.Invoke(capturedIdx);
                }
            });

            panelObj.SetActive(true);
        }

        // 패널 위치 수동 조정 (중요!)
        float panelHeight = 90f; // 패널 높이 + 간격
        for (int i = 0; i < activePanels.Count; i++)
        {
            RectTransform rectTransform = activePanels[i].GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                // Y 위치만 변경하여 수직으로 배치
                Vector2 position = rectTransform.anchoredPosition;
                position.y = -i * panelHeight;
                rectTransform.anchoredPosition = position;


                // 패널을 명시적으로 다시 활성화
                activePanels[i].SetActive(false);
                activePanels[i].SetActive(true);
            }
        }

        // 레이아웃 강제 업데이트
        if (creditPanelPool.parent != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(creditPanelPool.parent as RectTransform);
        }

        // 추가: CreditPanelPool의 UpdateLayout 메서드 호출
        creditPanelPool.UpdateLayout();
    }

    // 패널 위치 재조정 메서드
    public void ReorganizePanels()
    {

        if (creditPanelPool == null || creditPanelPool.parent == null)
        {
            return;
        }

        // 활성화된 패널들 수집
        List<GameObject> activePanels = new List<GameObject>();
        foreach (Transform child in creditPanelPool.parent)
        {
            if (child.gameObject.activeSelf)
            {
                activePanels.Add(child.gameObject);
            }
        }


        // 패널 위치 재조정
        float panelHeight = 90f; // 패널 높이 + 간격
        for (int i = 0; i < activePanels.Count; i++)
        {
            RectTransform rectTransform = activePanels[i].GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                // Y 위치만 변경하여 수직으로 배치
                Vector2 position = rectTransform.anchoredPosition;
                position.y = -i * panelHeight;
                rectTransform.anchoredPosition = position;


                // 계층 구조에서 위치 조정 (최상위 패널이 맨 앞에 보이도록)
                activePanels[i].transform.SetSiblingIndex(i);
            }
        }

        // 레이아웃 강제 업데이트
        LayoutRebuilder.ForceRebuildLayoutImmediate(creditPanelPool.parent as RectTransform);
    }

    #endregion
    #region ---------------------------------- 팝업 ----------------------------------
    private void LockPopup()
    {
        lockPopup.SetActive(true);
        popupText.text = "아직 접근할 수 없습니다.";
    }

    public void LoadPopup()
    {
        loadPopup.SetActive(true);
    }

    public void ClosePopup(string objName)
    {
        switch (objName)
        {
            case "lockPopup":
                lockPopup.SetActive(false);
                break;
            case "loadPopup":
                loadPopup.SetActive(false);
                break;
        }
    }
    #endregion
    #region ---------------------------------- UI Show/Hide ----------------------------------
    protected override void OnShow()
    {
        base.OnShow();

        // OnShow()가 호출될 때 활성화된 패널들이 있다면 그대로 유지
        if (creditPanelPool != null)
        {
            // 활성화된 패널들을 리프레시
            creditPanelPool.RefreshActivePanels();
        }
    }


    protected override void OnHide()
    {
        base.OnHide();

        // UI가 숨겨질 때 텍스트 초기화
        if (descText != null)
        {
            descText.text = "";
        }

        if (creditsText != null)
        {
            creditsText.text = "";
        }

        // 패널 반환 (숨겨질 때 패널 초기화)
        if (creditPanelPool != null)
        {
            creditPanelPool.ReturnAllPanels();
        }

        // 캐릭터 초기화
        if (mariaPrefab != null)
        {
            mariaPrefab.gameObject.SetActive(false);
        }

        if (eliosPrefab != null)
        {
            eliosPrefab.gameObject.SetActive(false);
        }

        // 카드 초기화
        if (flag != null)
        {
            flag.ResetCard();
            flag.SetActive(true);
        }

        if (hexagram != null)
        {
            hexagram.ResetCard();
            hexagram.SetActive(true);
        }
    }
    #endregion
}

