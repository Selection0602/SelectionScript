using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using DG.Tweening;
using Random = UnityEngine.Random;

[Serializable]
public class AssetLabelMapping
{
    public string label;
    public AssetType assetType;
}

public class LoadingController : ControllerUI
{
    #region ------------------- SerializeField -------------------
    [SerializeField] private LoadingUI loadingUI;
    private ILoadingView _view;

    [SerializeField] private AssetLabelMapping[] labelMappings;
    [SerializeField] private float tipChangeInterval = 2f;
    [SerializeField] private List<TipSO> tips;
    [SerializeField] private float smoothSpeed = 1f;
    [SerializeField] private Image loadingImage;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeTime = 0.5f;
    [SerializeField] private string loadingAnimatorLabel = "Mon_Anim";
    [SerializeField] private Animator imageAnimator;
    [SerializeField] private float animationChangeInterval = 3f;
    [SerializeField] private Vector3 defaultScale = new Vector3(1f, 1f, 1f);
    #endregion
    #region ------------------- Private Variables -------------------
    private LoadingSceneData config;

    private float targetProgress = 0f;
    private float displayedProgress = 0f;
    private bool isUnloading = false;
    private bool areAllAssetsLoaded = false;

    private AddressableManager addrManager;
    private Dictionary<string, object> loadedAssets = new();
    private Dictionary<string, AsyncOperationHandle> handles = new();
    private Dictionary<string, int> labelAssetCounts = new();
    private int totalAssetsToLoad = 0;

    private List<RuntimeAnimatorController> loadedAnimators = new();
    private AsyncOperationHandle<IList<RuntimeAnimatorController>> animatorLoadHandle;

    // 애니메이터 이름과 이미지 속성 키 매핑
    private readonly Dictionary<string, string> animatorToImagePropertyMapping = new()
    {
        { "Palm", "Palm" },
        { "Ophanim", "Ophanim" },
        { "Agis", "Agis" },
        { "Demon", "Demon" },
        { "Lib", "Lib" },
        { "Mask", "Mask" },
        { "SweetPotato", "SweetPotato" },
        { "Valkyrie", "Valkyrie" },
        { "Bacteriaman", "Bacteriaman" },
        { "Lacoste", "Lacoste" },
        { "Miggi", "Miggi" },
        { "Nunna", "Nunna" },
        { "Whitehair", "Whitehair" }
    };

    private readonly Dictionary<string, (int stateCount, string[] stateNames)> controllerStates = new() // 애니메이터 클립 설정
    {
        {
            "Palm",
            (4, new[] { "Idle", "Attack", "Move", "Death" })
        },
        {
            "Ophanim",
            (1, new[] { "ophanim" })
        },
        {
            "Demon",
            (8, new[] { "Idle", "Attack_1", "Attack_2", "Attack_4", "Attack_5", "Death", "Teleport_In", "Teleport_Out" })
        },
        {
            "Lib",
            (5, new[] { "Idle", "Attack_1", "Attack_2", "Death" , "Move"})
        },
        {
            "Mask",
            (4, new[] { "Idle", "Move","Attack", "Death" })
        },
        {
            "SweetPotato",
            (5, new[] { "Idle", "Move", "Attack_1", "Attack_2", "Death" })
        },
        {
            "Valkyrie",
            (4, new[] { "Idle", "Attack", "Death", "Move" })
        },
        {
            "Bacteriaman",
            (5, new[] { "Idle", "Attack_1", "Attack_2", "Move","Death" })
        },
        {
            "Lacoste",
            (5, new[] { "Idle", "Attack_1", "Attack_2", "Move","Death" })
        },
        {
            "Miggi",
            (4, new[] { "Idle", "Attack", "Move", "Death" })
        },
        {
            "Nunna",
            (4, new[] { "Idle", "Attack", "Move", "Death" })
        },
        {
            "Whitehair",
            (5, new[] { "Idle", "Attack", "Walk", "Run","Death" })
        }
    };

    // UI 이미지별 속성 정의 (위치, 크기, 스케일)
    private readonly Dictionary<string, UIImageProperties> imageProperties = new()
    {
        {
            "Lib",
            new UIImageProperties
            {
                Position = new Vector2(81f, 90f),
                Size = new Vector2(432f, 288f),
                Scale = new Vector3(1.3f, 1.3f, 1.0f)
            }
        },
        {
            "Mask",
            new UIImageProperties
            {
                Position = new Vector2(27f, 97f),
                Size = new Vector2(447.36f, 298.24f),
                Scale = new Vector3(1.3f, 1.3f, 1.0f)
            }
        },
        {
            "Ophanim",
            new UIImageProperties {
                Position = new Vector2(-15f, 90f),
                Size = new Vector2(320f, 320f),
                Scale = new Vector3(1.3f, 1.3f, 1.0f)
            }
        },
        {
            "Palm",
            new UIImageProperties
            {
                Position = new Vector2(64f, 90f),
                Size = new Vector2(441.6f, 294.4f),
                Scale = new Vector3(1.3f, 1.3f, 1.0f)
            }
        },
        {
            "SweetPotato",
            new UIImageProperties
            {
                Position = new Vector2(29f, 107f),
                Size = new Vector2(435.2f, 272f),
                Scale = new Vector3(1.3f, 1.3f, 1.0f)
            }
        },
        {
           "Valkyrie",
           new UIImageProperties {
               Position = new Vector2(-13f, 98f),
               Size = new Vector2(288f, 336f),
               Scale = new Vector3(1.3f, 1.3f, 1.0f)
           }
        },
        {
            "Demon",
            new UIImageProperties
            {
                Position = new Vector2(0f, 99f),
                Size = new Vector2(300f, 300f),
                Scale = new Vector3(1.3f, 1.3f, 1.0f)
            }
        },
        {
            "Bacteriaman",
            new UIImageProperties
            {
                Position = new Vector2(100f, 106f),
                Size = new Vector2(422.4f, 316.8f),
                Scale = new Vector3(1.3f, 1.3f, 1.0f)
            }
        },
        {
            "Lacoste",
            new UIImageProperties
            {
                Position = new Vector2(-41f, 45f),
                Size = new Vector2(475.2f, 240),
                Scale = new Vector3(1.3f, 1.3f, 1.0f)
            }
        },
        {
            "Miggi",
            new UIImageProperties
            {
                Position = new Vector2(-15f, 90f),
                Size = new Vector2(490f, 360f),
                Scale = new Vector3(1.3f, 1.3f, 1.0f)
            }
        },
        {
           "Nunna",
           new UIImageProperties
           {
               Position = new Vector2(-7f, 104f),
               Size = new Vector2(368f, 331.2f),
               Scale = new Vector3(1.3f, 1.3f, 1.0f)
           }
        },
        {
            "Whitehair",
            new UIImageProperties
            {
                Position = new Vector2(69f, 90f),
                Size = new Vector2(480f, 336f),
                Scale = new Vector3(1.3f, 1.3f, 1.0f)
            }
        }
    };
    #endregion
    #region ------------------- Struct -------------------
    // UI 이미지 속성을 저장하기 위한 구조체
    [System.Serializable]
    private struct UIImageProperties
    {
        public Vector2 Position;  // posX, posY
        public Vector2 Size;      // width, height
        public Vector3 Scale;     // scaleX, scaleY, scaleZ
    }
    #endregion
    #region ------------------- 생명주기 -------------------
    private void Awake() // Awake에서 초기화
    {
        _view = loadingUI;
    }
    private void OnEnable() // 시작시에 로드 되어있는 데이터들 언로드
    {
        if (imageAnimator != null)
        {
            imageAnimator.enabled = true;
        }
        ClearLoadedAssets();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
    #endregion
    #region ------------------- Setup -------------------
    public async Task Setup(LoadingSceneData data)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        addrManager = Manager.Instance.AddressableManager;
        config = data;
        _view.Initialize();
        labelMappings = config.mappings;
        tipChangeInterval = config.tipChangeInterval;

        await LoadAnimations();

        if (imageAnimator != null)
        {
            imageAnimator.enabled = true;
        }

        StartCoroutine(ChangeTipsLoop());
        StartCoroutine(SmoothProgressCoroutine());

        await FadeIn();
        StartLoadingProcess();
    }
    #endregion
    #region ------------------- 어드레서블 로드 -------------------
    public async void StartLoadingProcess() // 에셋 로딩
    {

        List<Task> loadingTasks = new List<Task>();
        foreach (var mapping in labelMappings)
        {
            loadingTasks.Add(LoadAssetsByLabel(mapping));
        }
        await Task.WhenAll(loadingTasks);
        OnAllAssetsLoaded();
    }

    private async Task LoadAssetsByLabel(AssetLabelMapping mapping) // 라벨에 따른 에셋 로드
    {

        List<string> splitLabels = new List<string>();

        if (mapping.label.Contains(","))
            splitLabels = mapping.label.Split(',').Select(label => label.Trim()).ToList();

        var locHandle = splitLabels.Count > 1 ?
            Addressables.LoadResourceLocationsAsync(splitLabels, Addressables.MergeMode.Intersection) :
            Addressables.LoadResourceLocationsAsync(mapping.label);

        await locHandle.Task;

        if (locHandle.Status != AsyncOperationStatus.Succeeded ||
            locHandle.Result == null ||
            locHandle.Result.Count == 0)
        {
            return;
        }

        int assetCount = locHandle.Result.Count;
        labelAssetCounts[mapping.label] = assetCount;
        totalAssetsToLoad += assetCount;

        var loadHandle = splitLabels.Count > 1 ? Addressables.LoadAssetsAsync<UnityEngine.Object>
        (
            splitLabels,
            null,
            Addressables.MergeMode.Intersection
        ) :
            Addressables.LoadAssetsAsync<UnityEngine.Object>
            (
                mapping.label,
                null
            );
        handles[mapping.label] = loadHandle;
        addrManager.RegisterHandle(mapping.label, loadHandle);

        await LoadAssetsWithProgress(loadHandle, mapping.label);
    }

    private async Task LoadAssetsWithProgress(AsyncOperationHandle<IList<UnityEngine.Object>> handle, string label) // 로딩 진행률 업데이트
    {
        while (!handle.IsDone)
        {
            float progress = handle.GetDownloadStatus().Percent;
            UpdateWeightedProgress(label, progress);
            await Task.Yield();
        }

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            loadedAssets[label] = handle.Result;
        }
        UpdateWeightedProgress(label, 1f);
    }

    private void ClearLoadedAssets() // 로드된 에셋 언로드
    {
        if (isUnloading) return;
        isUnloading = true;

        foreach (var kv in handles)
            if (kv.Value.IsValid())
                Addressables.Release(kv.Value);

        Manager.Instance.AddressableManager.AllUnloadAssets();
        handles.Clear();
        loadedAssets.Clear();
        Resources.UnloadUnusedAssets();
    }

    private void OnAllAssetsLoaded() // 모든 에셋이 로드시에 호출
    {
        areAllAssetsLoaded = true;
    }
    #endregion
    #region ------------------- 진행 바 -------------------
    private void UpdateWeightedProgress(string label, float progress) // 라벨 별 진행바 증가
    {
        if (!labelAssetCounts.ContainsKey(label) || totalAssetsToLoad == 0) return;

        float weight = (float)labelAssetCounts[label] / totalAssetsToLoad;
        targetProgress = Mathf.Clamp01(targetProgress + (progress * weight));
    }

    private IEnumerator SmoothProgressCoroutine() // 진행바가 부드럽게 증가가 되도록 함
    {
        while (displayedProgress < 1f || !areAllAssetsLoaded)
        {
            displayedProgress = Mathf.MoveTowards(
                displayedProgress,
                targetProgress,
                smoothSpeed * Time.deltaTime
            );
            _view.SetProgress(displayedProgress);
            yield return null;
        }
        _view.SetProgress(1f);
        while (!areAllAssetsLoaded)
        {
            yield return null;
        }

        yield return StartCoroutine(FadeOutAndLoadScene());
    }
    #endregion
    #region ------------------- UI 효과 -------------------
    private async Task FadeIn() // UI 페이드 인 효과
    {
        if (canvasGroup == null) return;

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        await canvasGroup.DOFade(1f, fadeTime)
            .SetEase(Ease.InOutSine)
            .AsyncWaitForCompletion();
    }

    private async Task FadeOut() // UI 페이드 아웃 효과
    {
        if (canvasGroup == null) return;

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        await canvasGroup.DOFade(0f, fadeTime)
            .SetEase(Ease.InOutSine)
            .AsyncWaitForCompletion();
    }

    private IEnumerator FadeOutAndLoadScene() // 페이드 아웃 후 씬 전환
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            yield return canvasGroup.DOFade(0f, fadeTime)
                .SetEase(Ease.InOutSine)
                .WaitForCompletion();
        }
        LoadScene(config.nextSceneName, config.payload);
    }
    #endregion
    #region ------------------- 팁 -------------------
    private IEnumerator ChangeTipsLoop() // 일정 시간마다 팁 변경
    {
        while (true)
        {
            if (tips.Count > 0)
            {
                var randomTip = tips[Random.Range(0, tips.Count)];
                if (randomTip != null)
                {
                    _view.SetText(randomTip);
                }
            }
            yield return new WaitForSeconds(tipChangeInterval);
        }
    }
    #endregion
    #region ------------------- 애니메이션 -------------------
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

    private async Task LoadAnimations() // 애니메이터 컨트롤러 로드
    {

        animatorLoadHandle = Addressables.LoadAssetsAsync<RuntimeAnimatorController>(loadingAnimatorLabel, null);
        await animatorLoadHandle.Task;

        if (animatorLoadHandle.Status == AsyncOperationStatus.Succeeded)
        {
            loadedAnimators = new List<RuntimeAnimatorController>(animatorLoadHandle.Result);


            if (loadedAnimators.Count > 0 && imageAnimator != null)
            {
                SetRandomAnimatorController();
            }

        }
    }

    private void SetRandomAnimatorController()
    {
        if (loadedAnimators == null || loadedAnimators.Count == 0 || imageAnimator == null)
        {
            return;
        }


        // 랜덤 컨트롤러 선택
        int randomIndex = Random.Range(0, loadedAnimators.Count);
        RuntimeAnimatorController selectedController = loadedAnimators[randomIndex];
        string controllerName = selectedController.name;

        // 애니메이션 상태 정보 가져오기
        int stateCount = 0;
        string[] stateNames = null;
        GetAnimationStateInfo(selectedController, controllerName, ref stateCount, ref stateNames);

        // 랜덤 상태 선택 및 적용
        ApplyRandomAnimationState(selectedController, stateCount, stateNames);

        // 이미지 스타일 적용
        ApplyImageStyle(controllerName);

        // 애니메이션 상태 변경 코루틴 시작
    }

    // 애니메이션 상태 정보를 가져오는 메서드
    private void GetAnimationStateInfo(RuntimeAnimatorController controller, string controllerName,
                                    ref int stateCount, ref string[] stateNames)
    {

        string simpleName = controllerName;
        int lastSlash = controllerName.LastIndexOf('/');
        if (lastSlash >= 0 && lastSlash < controllerName.Length - 1)
        {
            simpleName = controllerName.Substring(lastSlash + 1);
        }


        // 사전에 정의된 상태 정보가 있는지 확인
        bool hasDefinedStates = controllerStates.TryGetValue(simpleName, out var states);

        if (hasDefinedStates)
        {
            stateCount = states.stateCount;
            stateNames = states.stateNames;
            return;
        }

        if (controller != null && controller.animationClips != null && controller.animationClips.Length > 0)
        {
            AnimationClip[] clips = controller.animationClips;
            stateCount = clips.Length;
            stateNames = new string[clips.Length];


            for (int i = 0; i < clips.Length; i++)
            {
                stateNames[i] = clips[i].name;
            }

            // 상태가 없으면 기본값으로 설정
            if (stateCount == 0)
            {
                stateCount = 1;
                stateNames = new[] { "Idle" };
            }
            return;
        }

        // 기본값 설정
        stateCount = 1;
        stateNames = new[] { "Idle" };
    }

    // 랜덤 애니메이션 상태를 선택하고 적용하는 메서드
    private void ApplyRandomAnimationState(RuntimeAnimatorController controller, int stateCount, string[] stateNames)
    {
        // 애니메이터 컨트롤러 설정
        imageAnimator.runtimeAnimatorController = controller;

        // 컨트롤러가 설정되었는지 확인
        if (imageAnimator.runtimeAnimatorController == null)
        {
            return;
        }


        // 랜덤 상태 인덱스 선택 - Idle(0)을 피하기 위해 1부터 시작
        int randomState = 0;
        if (stateCount > 1)
        {
            randomState = Random.Range(1, stateCount);
        }

        // 상태 이름 가져오기
        string stateName = randomState < stateNames.Length ? stateNames[randomState] : "Idle";


        // 애니메이터 파라미터 설정 시도

        imageAnimator.SetInteger("isActive", randomState);


        // 직접 해당 상태로 전환 시도

        // Play 메서드 사용
        imageAnimator.Play(stateName, 0, 0f);


        // 대체 방법 시도

        imageAnimator.CrossFade(stateName, 0.1f, 0);



        // 애니메이터 업데이트 강제 실행
        imageAnimator.Update(0f);

    }
    
    // 이미지 스타일 적용 메서드 (스케일, 위치, 크기를 포함)
    private void ApplyImageStyle(string controllerName)
    {
        if (loadingImage == null) return;
        
        // 기본 설정 적용
        loadingImage.SetNativeSize();
        
        // 기본 스케일 설정
        Vector3 defaultScaleValue = defaultScale * 2.0f;
        
        // RectTransform인 경우 미리 정의된 속성 적용
        if (loadingImage.transform is RectTransform rectTransform)
        {
            // 애니메이터 이름에 따라 적절한 이미지 속성 키를 결정
            string propertyKey = controllerName;
            
            // 매핑 딕셔너리에서 이미지 속성 키 찾기
            if (animatorToImagePropertyMapping.TryGetValue(controllerName, out var mappedKey))
            {
                propertyKey = mappedKey;
            }
            
            // 이미지 속성 적용
            ApplyImageProperties(propertyKey, rectTransform);
        }
        else
        {
            // RectTransform이 아닌 경우에는 기본 스케일 적용
            loadingImage.transform.localScale = defaultScaleValue;
        }
    }
    #endregion
}
