using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CursorManager : MonoBehaviour
{
    private Texture2D defaultCursor;
    private Texture2D clickCursor;
    
    Vector2 hotSpot = Vector2.zero;
    private void Awake()
    {
        LoadCursors();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SetClickCursor();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            SetDefaultCursor();
        }
    }

    private async void LoadCursors()
    {
        // 커서 이미지 로드
        var cursorHandle = Addressables.LoadAssetsAsync<Texture2D>("Cursor", null);
        await cursorHandle.Task;
        if (cursorHandle.Status == AsyncOperationStatus.Succeeded)
        {
            var cursors = cursorHandle.Result;
            foreach (var cursor in cursors)
            {
                if (cursor.name == "cursor")
                {
                    defaultCursor = cursor;
                    SetDefaultCursor();
                }
                else if (cursor.name == "cursor2")
                {
                    clickCursor = cursor;
                }
            }
        }
    }

    public void SetDefaultCursor()
    {
        if (defaultCursor != null)
        {
            Cursor.SetCursor(defaultCursor, hotSpot, CursorMode.Auto);
        }
    }

    public void SetClickCursor()
    {
        if (clickCursor != null)
        {
            Cursor.SetCursor(clickCursor, hotSpot, CursorMode.Auto);
        }
    }

    // 버튼에 이벤트 리스너 추가하는 메서드
    public void AddCursorEvents(UnityEngine.UI.Button button)
    {
        var trigger = button.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (trigger == null)
            trigger = button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            
        var pointerDown = new UnityEngine.EventSystems.EventTrigger.Entry 
        { 
            eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown 
        };
        pointerDown.callback.AddListener((data) => SetClickCursor());
        trigger.triggers.Add(pointerDown);
        
        var pointerUp = new UnityEngine.EventSystems.EventTrigger.Entry 
        { 
            eventID = UnityEngine.EventSystems.EventTriggerType.PointerUp 
        };
        pointerUp.callback.AddListener((data) => SetDefaultCursor());
        trigger.triggers.Add(pointerUp);
    }
}
