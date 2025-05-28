using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreditPanelPool : MonoBehaviour
{
    public GameObject panelPrefab;
    public Transform parent;
    public int initialPoolSize = 11;
    private int maxPoolSize = 50;
 
    private List<GameObject> pool = new List<GameObject>();

    void Start()
    {
        InitializePool();
    }

    private void InitializePool()
    {
        foreach (var obj in pool)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        pool.Clear();

        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewPanel();
        }        
        // VerticalLayoutGroup 확인 및 추가
        CheckOrAddLayoutGroup();
    }

    private GameObject CreateNewPanel()
    {
        if (pool.Count >= maxPoolSize)
        {
            return null;
        }

        GameObject obj = Instantiate(panelPrefab, parent);
        obj.SetActive(false);
        pool.Add(obj);
        return obj;
    }
    
    // VerticalLayoutGroup 확인 및 추가
    private void CheckOrAddLayoutGroup()
    {
        if (parent != null)
        {
            VerticalLayoutGroup layoutGroup = parent.GetComponent<VerticalLayoutGroup>();
            if (layoutGroup == null)
            {
                              layoutGroup = parent.gameObject.AddComponent<VerticalLayoutGroup>();
                layoutGroup.childAlignment = TextAnchor.UpperCenter;
                layoutGroup.spacing = 10f;
                layoutGroup.padding = new RectOffset(10, 10, 10, 10);
                
                // ContentSizeFitter 추가 (레이아웃 자동 조정)
                ContentSizeFitter sizeFitter = parent.gameObject.GetComponent<ContentSizeFitter>();
                if (sizeFitter == null)
                {
                    sizeFitter = parent.gameObject.AddComponent<ContentSizeFitter>();
                    sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                }
            }
        }
    }

    // 패널 레이아웃 강제 업데이트
    public void UpdateLayout()
    {
        if (parent != null)
        {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(parent.GetComponent<RectTransform>());
        }
    }

    public GameObject GetPanel()
    {
        // 부모 오브젝트 활성화 상태 확인
        if (parent != null && !parent.gameObject.activeSelf)
        {
            parent.gameObject.SetActive(true);
            
            // 부모의 부모도 활성화
            Transform parentParent = parent.parent;
            if (parentParent != null && !parentParent.gameObject.activeSelf)
            {
                parentParent.gameObject.SetActive(true);
            }
        }
        
        GameObject availablePanel = pool.Find(obj => obj != null && !obj.activeInHierarchy);

        if (availablePanel == null && pool.Count < maxPoolSize)
        {
            availablePanel = CreateNewPanel();
        }

        if (availablePanel == null)
        {
            return null;
        }
        
        // 패널 초기화 및 활성화
        CreditPanel panel = availablePanel.GetComponent<CreditPanel>();
        if (panel != null)
        {
            panel.SetText(""); // 초기화
        }
        
        // 패널 위치와 스케일이 올바른지 확인
        RectTransform rectTransform = availablePanel.GetComponent<RectTransform>();
        if (rectTransform != null)
        { 
            // parent가 활성화되어 있는지 확인
            if (parent != null)
            {                
                Transform parentParent = parent.parent;
            }
        }
        
        // 패널 강제 활성화
        availablePanel.SetActive(true);
        
        // 패널을 계층 구조에서 최상위로 가져와 다른 패널에 가려지지 않도록 함
        availablePanel.transform.SetAsLastSibling();
        
        return availablePanel;
    }

    public void ReturnAllPanels()
    {
        foreach (GameObject obj in pool)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
        
        // 패널 반환 후 레이아웃 업데이트
        UpdateLayout();
    }

    // 활성화된 패널들의 상태를 리프레시
    public void RefreshActivePanels()
    {
        foreach (GameObject obj in pool)
        {
            if (obj.activeInHierarchy)
            {
                CreditPanel panel = obj.GetComponent<CreditPanel>();
                if (panel != null)
                {
                    // 패널이 이미 활성화된 경우 강제로 다시 한번 텍스트 적용
                    string currentText = panel.GetComponent<CreditPanel>().GetCurrentText();
                    panel.SetText(currentText);
                }
            }
        }
    }
}