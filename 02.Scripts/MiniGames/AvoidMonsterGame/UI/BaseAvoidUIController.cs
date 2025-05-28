using UnityEngine;

public abstract class BaseAvoidUIController : MonoBehaviour
{
    [SerializeField] protected GameObject uiObject;
    
    public abstract void OnMoveUp();
    public abstract void OnMoveDown();
    public abstract void OnMoveLeft();
    public abstract void OnMoveRight();
    public abstract void OnSubmit();
    public abstract void OnCancel();
    
    public virtual void Show()
    {
        uiObject.SetActive(true);
    }
    
    public virtual void Hide()
    {
        uiObject.SetActive(false);
    }
}
