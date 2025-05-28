using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class RestScene : SceneBase
{
    [SerializeField] private Image _characterImage;
    
    protected override async void OnStart(object data)
    {
        base.OnStart(data);
        await SetupCharacter();
    }

    private async Task SetupCharacter()
    {
        try
        {
            List<CharacterSO> characterList =
                await Manager.Instance.AddressableManager.GetHandleResultList<CharacterSO>("Character");

            if (characterList == null)
            {
                Debug.Log("캐릭터 SO 정보를 가져오지 못했습니다.");
                await ManualSetupCharacter();
                return;
            }

            foreach (var character in characterList)
            {
                if (character.Index == Manager.Instance.DataManager.CurrentCharcterId)
                {
                    _characterImage.sprite = character.Image;
                    _characterImage.SetNativeSize();
                    break;
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log($"캐릭터 SO 정보를 가져오는 중 문제가 발생하였습니다. {e.Message}");
            await ManualSetupCharacter();
        }
    }
    
    private async Task ManualSetupCharacter()
    {
        _characterImage.sprite = await SetCharacterSprite();
        _characterImage.SetNativeSize();
    }
    
    private async Task<Sprite> SetCharacterSprite()
    {
        try
        {
            var playerData = await Manager.Instance.AddressableManager.Load<CharacterSO>(
                Manager.Instance.DataManager.CurrentCharcterId.ToString());
        
            if (playerData == null)
            {
                Debug.LogWarning("캐릭터 데이터를 찾을 수 없습니다.");
                return null;
            }
        
            return playerData.Image;
        }
        catch (Exception e)
        {
            Debug.LogError($"캐릭터 스프라이트 로딩 오류: {e.Message}");
            return null;
        }
    }

    public void MoveToMapScene()
    {
        DOTween.KillAll();
        
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
            nextSceneName = "MapScene",
            payload = new object[] { }
        };

        foreach (var mappings in labelMapping)
        {
            if (mappings.label is "BGM" or "SFX")
            {
                mappings.label += $", {loadData.nextSceneName}";
            }
        }

        LoadScene("LoadingScene", loadData);
    }
}
