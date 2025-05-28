using UnityEngine;
using System.Collections.Generic;

public class BaseUI : MonoBehaviour
{
  protected bool isLocked = false;
  protected CanvasGroup canvasGroup;
  // 보존할 자식 오브젝트 상태를 저장하는 사전
  protected Dictionary<Transform, bool> preservedChildStates;
  
  public virtual void Initialize()
  {
    canvasGroup = GetComponent<CanvasGroup>();
    if (canvasGroup == null)
    {
      canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }
    
    preservedChildStates = new Dictionary<Transform, bool>();
  }

  // 특정 자식 오브젝트의 상태를 보존하도록 등록
  public virtual void PreserveChildState(Transform child)
  {
    if (preservedChildStates == null)
      preservedChildStates = new Dictionary<Transform, bool>();
      
    if (!preservedChildStates.ContainsKey(child))
      preservedChildStates.Add(child, child.gameObject.activeSelf);
  }

  public virtual void Show()
  {
    // 상태 저장
    SaveChildStates();
    
    gameObject.SetActive(true);
    if (isLocked)
    {
      if (canvasGroup != null)
      {
        canvasGroup.alpha = 0.5f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
      }
    }
    else
    {
      if (canvasGroup != null)
      {
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
      }
    }
    OnShow();
    
    // 저장된 상태 복원
    RestoreChildStates();
  }

  public virtual void Hide()
  {
    // 상태 저장
    SaveChildStates();
    
    OnHide();
    gameObject.SetActive(false);
  }

  public virtual void SetLocked(bool locked)
  {
    isLocked = locked;
    if (gameObject.activeSelf)
    {
      Show(); // 잠금 상태가 변경되면 UI 상태를 업데이트
    }
  }
  
  // 보존할 자식 오브젝트의 상태 저장
  protected virtual void SaveChildStates()
  {
    if (preservedChildStates == null) return;
    
    foreach (var child in preservedChildStates.Keys)
    {
      if (child != null)
        preservedChildStates[child] = child.gameObject.activeSelf;
    }
  }
  
  // 보존된 자식 오브젝트 상태 복원
  protected virtual void RestoreChildStates()
  {
    if (preservedChildStates == null) return;
    
    foreach (var pair in preservedChildStates)
    {
      if (pair.Key != null)
        pair.Key.gameObject.SetActive(pair.Value);
    }
  }
  
  protected virtual void OnShow() { }
  protected virtual void OnHide() { }
}

