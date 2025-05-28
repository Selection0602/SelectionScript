using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AtlasLoader : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Image _targetImage;                //UI에 적용할 Image
    [SerializeField] private SpriteRenderer _spriteRenderer;    //Sprite Render에 적용할 Sprite  

    [Header("Addressables Settings")]
    [SerializeField] private string _atlasAddress;  //SpriteAtlas의 Address
    [SerializeField] private string _spriteName;    //Atlas 안에 있는 Sprite 이름

    [Header("Is Canvas?")]
    [SerializeField] private bool _isCanvas;

    private void Awake()
    {
        Addressables.LoadAssetAsync<SpriteAtlas>(_atlasAddress).Completed += OnAtlasLoaded;
    }

    private void OnAtlasLoaded(AsyncOperationHandle<SpriteAtlas> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            SpriteAtlas atlas = handle.Result;
            Sprite sprite = atlas.GetSprite(_spriteName);

            if (sprite != null)
            {
                if(_isCanvas) _targetImage.sprite = sprite;
                else _spriteRenderer.sprite = sprite;
            }
            else
            {
                Debug.LogWarning($"스프라이트 '{_spriteName}'을(를) 찾을 수 없습니다.");
            }
        }
        else
        {
            Debug.Log(gameObject.name);
            Debug.LogError("SpriteAtlas 로드 실패");
        }
    }
}
