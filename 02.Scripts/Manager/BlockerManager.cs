using UnityEngine;
using UnityEngine.UI;

public class BlockerManager : Singleton<BlockerManager>
{
    private GameObject _blocker;
    private Canvas _blockerCanvas;

    protected override void Awake()
    {
        base.Awake();
        SetupCanvasAndBlocker();
    }

    private void SetupCanvasAndBlocker()
    {
        // Canvas 생성
        GameObject canvasObj = new GameObject("BlockerCanvas");
        canvasObj.transform.SetParent(transform);
        _blockerCanvas = canvasObj.AddComponent<Canvas>();
        _blockerCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _blockerCanvas.sortingOrder = 1;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // Blocker Image 생성
        _blocker = new GameObject("BlockerImage");
        _blocker.transform.SetParent(canvasObj.transform);
        _blocker.SetActive(false); // 초기 비활성화

        Image img = _blocker.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0);

        RectTransform rect = _blocker.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    public void Active()
    {
        _blocker.gameObject.SetActive(true);
    }

    public void InActive()
    {
        _blocker.gameObject.SetActive(false);
    }
}
