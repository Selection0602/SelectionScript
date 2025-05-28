using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class ControllerUI : SceneBase
{
   protected List<BaseUI> allUIs = new();
   protected BaseUI currentUI;
   // 페이드 효과를 위한 변수들
   protected Image fadeImage;
   protected CanvasGroup fadeCanvasGroup;
   protected float fadeDuration = 0.3f;
   protected GameObject fadeCanvas;
   protected bool isTransitioning = false;  // 외부에서 접근할 수 있도록 protected로 변경
   protected Coroutine fadeCoroutine = null; // 현재 실행 중인 페이드 코루틴 참조 저장

   protected virtual void Start()
   {
      InitializeFadeEffect();
      InitAllUIs();
      HideAllUIs();
   }

   // 페이드 효과 초기화
   protected virtual void InitializeFadeEffect()
   {
      // 페이드 효과를 위한 Canvas 생성
      fadeCanvas = new GameObject("FadeCanvas");
      fadeCanvas.transform.SetParent(transform);
      Canvas canvas = fadeCanvas.AddComponent<Canvas>();
      canvas.renderMode = RenderMode.ScreenSpaceOverlay;
      canvas.sortingOrder = 9999;

      // CanvasScaler 설정
      CanvasScaler scaler = fadeCanvas.AddComponent<CanvasScaler>();
      scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
      scaler.referenceResolution = new Vector2(1920, 1080);

      // 검은 이미지 생성
      GameObject imageObj = new GameObject("FadeImage");
      imageObj.transform.SetParent(fadeCanvas.transform, false);
      fadeImage = imageObj.AddComponent<Image>();
      fadeImage.color = new Color(0, 0, 0, 1f); // 완전한 검은색 설정
      fadeImage.raycastTarget = true; // 페이드 중 입력 차단을 위해 활성화
      
      // 전체 화면을 덮도록 설정
      RectTransform rect = fadeImage.rectTransform;
      rect.anchorMin = Vector2.zero;
      rect.anchorMax = Vector2.one;
      rect.sizeDelta = Vector2.zero;
      
      // CanvasGroup 설정
      fadeCanvasGroup = imageObj.AddComponent<CanvasGroup>();
      fadeCanvasGroup.alpha = 0;
      fadeCanvas.SetActive(false);
   }

   // 페이드 색상 설정 메서드 추가
   protected void SetFadeColor(Color color)
   {
      if (fadeImage != null)
      {
         fadeImage.color = color;
      }
   }

   // 페이드 스프라이트 설정 메서드 추가
   protected void SetFadeSprite(Sprite sprite)
   {
      if (fadeImage != null)
      {
         fadeImage.sprite = sprite;
      }
   }

   private void HideAllUIs()
   {
      foreach (var ui in allUIs)
      {
         if (ui != null)
         { 
            ui.gameObject.SetActive(false);
         }
      }
   }

   public virtual void InitAllUIs()
   {
      foreach (var ui in allUIs)
      {
         if (ui != null)
         {
            ui.Initialize();
         }
      }
   }

   public virtual void ShowUI(BaseUI targetUI)
   {
      // 이미 전환 중이면 중단
      if (isTransitioning || targetUI == null) return;
      isTransitioning = true;
      
      // 이전에 실행 중인 페이드 코루틴이 있으면 중지
      if (fadeCoroutine != null)
      {
         StopCoroutine(fadeCoroutine);
         fadeCoroutine = null;
      }

      // UI 전환을 위한 코루틴 시작
      fadeCoroutine = StartCoroutine(FadeUITransition(targetUI));
   }
   
   // 페이드 전환 코루틴
   protected virtual IEnumerator FadeUITransition(BaseUI targetUI)
   {
      // 페이드 캔버스 활성화 - 완전히 투명한 상태로 시작
      fadeCanvasGroup.alpha = 0f;
      fadeCanvas.SetActive(true);
      
      // 페이드 인 (투명 -> 불투명)
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
      if (currentUI != null)
      {
         currentUI.Hide();
         currentUI.gameObject.SetActive(false);
      }
      
      // 새 UI 활성화
      targetUI.gameObject.SetActive(true);
      targetUI.Show();
      currentUI = targetUI;
      
      // 페이드 아웃 (불투명 -> 투명)
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
   }
   
   // 페이드 없이 UI를 직접 전환하는 메서드
   public virtual void ShowUIDirectly(BaseUI targetUI)
   {
      if (targetUI == null) return;
      
      // 이미 전환 중이면 이전 전환 중단
      if (isTransitioning)
      {
         if (fadeCoroutine != null)
         {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
         }
         
         // 페이드 효과 즉시 해제
         fadeCanvasGroup.alpha = 0f;
         fadeCanvas.SetActive(false);
      }
      
      isTransitioning = false;
      
      // 현재 UI 비활성화
      if (currentUI != null)
      {
         currentUI.Hide();
         currentUI.gameObject.SetActive(false);
      }
      
      // 새 UI 활성화
      targetUI.gameObject.SetActive(true);
      targetUI.Show();
      currentUI = targetUI;
   }
   
   // 게임 시작 시 즉시 검은 화면으로 가리기
   protected void CoverScreenWithBlack()
   {
      if (fadeCanvas != null)
      {
         fadeCanvas.SetActive(true);
         fadeCanvasGroup.alpha = 1f;
      }
   }
   
   // 시작 화면 준비가 끝난 후 페이드 아웃
   protected IEnumerator FadeOutInitialScreen()
   {
      if (fadeCanvas == null || !fadeCanvas.activeSelf)
      {
         yield break;
      }
      
      // 페이드 아웃 (불투명 -> 투명)
      float elapsed = 0f;
      while (elapsed < fadeDuration)
      {
         elapsed += Time.deltaTime;
         float normalizedTime = Mathf.Clamp01(elapsed / fadeDuration);
         fadeCanvasGroup.alpha = 1f - normalizedTime;
         yield return null;
      }
      
      fadeCanvasGroup.alpha = 0f;
      fadeCanvas.SetActive(false);
   }
}
