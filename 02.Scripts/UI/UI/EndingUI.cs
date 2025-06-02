using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public interface IEndingView
{
    void Initialize(int endingIndex);
    void InitButton();
    IEnumerator UpdateUI(EndingSO ending);
}

public class EndingUI : BaseUI, IEndingView
{
    [SerializeField] private Image endingImage;
    [SerializeField] private TextMeshProUGUI endingText;
    [SerializeField] private TextMeshProUGUI endingNameText;
    [SerializeField] private Button returnButton;
    [SerializeField] private Image _fadeImage;
    [SerializeField] private Image _lastSiblingfadeImage;
    [SerializeField] private float _fadeDuration = 1.0f;
    [SerializeField] private Image _darkPanel;

    private int targetIndex;
    
    private const int SELECTION_ENDING_INDEX = 11;

    public void Initialize(int endingIndex)
    {
        StartCoroutine(LoadEnding(endingIndex));
        InitButton();
    }

    public void InitButton()
    {
        returnButton.gameObject.SetActive(false);
        returnButton.onClick.RemoveAllListeners();
        returnButton.onClick.AddListener(ReturnToLobby);
    }

    private IEnumerator LoadEnding(int endingIndex)
    {
        var handle = Addressables.LoadAssetsAsync<EndingSO>("Ending", null);
        yield return handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            foreach (var ending in handle.Result)
            {
                if (ending.Index == endingIndex)
                {
                    yield return UpdateUI(ending);
                    break;
                }
            }
        }
        Addressables.Release(handle);
    }

    public IEnumerator UpdateUI(EndingSO ending)
    {
        if (ending == null)
        {
            Debug.LogError("[EndingUI] 표시할 엔딩 데이터가 null입니다.");
            yield break;
        }

        if(ending.Index == SELECTION_ENDING_INDEX)
            endingImage.color = Color.black;
        
        // 본 엔딩을 DataManager에 기록
        Manager.Instance.DataManager.AddViewedEnding(ending.Index);

        if (CheckSelectionEnding())
        {
            returnButton.onClick.RemoveAllListeners();
            returnButton.onClick.AddListener(() =>
            {
                ClearEnding();
                Initialize(SELECTION_ENDING_INDEX);
            });
        }
            

        if (endingImage != null && ending.EndingImage != null)
        {
            endingImage.sprite = ending.EndingImage;
            yield return FadeOut();
        }

        if (endingText != null)
        {
            while (!Input.GetMouseButtonDown(0))
            {
                yield return null;
            }
            _darkPanel.gameObject.SetActive(true);
            yield return new WaitForSeconds(1f);
            string endingName = $"엔딩 {ending.Index} : {ending.EndingName}";
            string cleanedText = TextUtility.CleanLineBreaks(ending.EndingText);
            yield return new WaitForSeconds(0.5f);
            yield return FadeIn(_fadeImage, 0.5f);
            endingNameText.gameObject.SetActive(true);
            yield return TextUtility.AnimatorText(endingNameText, endingName, delegate { });
            endingText.gameObject.SetActive(true);
            yield return TextUtility.AnimatorText(endingText, cleanedText, delegate { });
            _lastSiblingfadeImage.gameObject.SetActive(true);
            yield return FadeIn(_lastSiblingfadeImage, 1.0f);
            returnButton.gameObject.SetActive(true);
        }
        else
        {
            returnButton.gameObject.SetActive(true);
        }
    }

    private IEnumerator FadeIn(Image target, float endValue)
    {
        float elapsed = 0.0f;
        Color color = target.color;
        color.a = 0f;
        target.color = color;
        while (elapsed < _fadeDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(0f, endValue, elapsed / _fadeDuration);
            target.color = color;
            yield return null;
        }
        color.a = endValue;
        target.color = color;
    }

    private IEnumerator FadeOut()
    {
        float elapsed = 0.0f;
        Color color = _lastSiblingfadeImage.color;
        color.a = 1f;
        _lastSiblingfadeImage.color = color;

        while (elapsed < _fadeDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, elapsed / _fadeDuration);
            _lastSiblingfadeImage.color = color;
            yield return null;
        }

        color.a = 0f;
        _lastSiblingfadeImage.color = color;
        _lastSiblingfadeImage.gameObject.SetActive(false);
    }

    private void ReturnToLobby()
    {
        Manager.Instance.DataManager.ClearCharacterData();
        Manager.Instance.SaveManager.ClearAllData();
        var labelMapping = new AssetLabelMapping[]
        {
            new AssetLabelMapping
            {
                label = "NodeType",
                assetType = AssetType._NodeTypeDataSO
            },
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
            }
        };

        var loadData = new LoadingSceneData
        {
            mappings = labelMapping,
            tipChangeInterval = 2f,
            nextSceneName = "StartScene",
            payload = new object[] { }
        };

        foreach (var mapping in labelMapping)
        {
            if (mapping.label is "BGM" or "SFX")
            {
                mapping.label += ", StartScene";
            }
        }

        SceneBase.LoadScene("LoadingScene", loadData);
    }

    private bool CheckSelectionEnding()
    {
        var dataManager = Manager.Instance.DataManager;
        var creditManager = Manager.Instance.CreditManager;
        
        return dataManager.ViewedEndings.Count == creditManager.EndingDatas.Count - 1 &&
               !dataManager.ViewedEndings.Contains(SELECTION_ENDING_INDEX);
    }
    
    private void ClearEnding()
    {
        endingImage.sprite = null;
        endingText.text = "";
        endingNameText.text = "";
        returnButton.gameObject.SetActive(false);
        _darkPanel.gameObject.SetActive(false);
    }
}

