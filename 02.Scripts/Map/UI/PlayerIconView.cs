using DG.Tweening;
using UnityEngine;

public class PlayerIconView : MonoBehaviour
{
    [Header("Player Icon")]
    [SerializeField] private GameObject _playerIconPrefab;
    [SerializeField] private Transform _playerIconParent;
    
    [Header("Player Icon Settings")]
    [SerializeField] private float _iconYOffset = 25f;
    [SerializeField] private float _dotweenPositionY = 20f;
    [SerializeField] private float _dotweenDuration = 0.5f;
    
    private GameObject _playerIconObject;
    
    public void ShowPlayerPosition(Vector2 position)
    {
        // 플레이어 아이콘이 존재하지 않는다면 생성
        if (_playerIconObject == null && _playerIconPrefab != null)
        {
            _playerIconObject = Instantiate(_playerIconPrefab, _playerIconParent);
            _playerIconObject.name = "PlayerIcon";
        }
        
        // 아이콘 위치를 해당 노드뷰 위치 살짝 위로 설정
        _playerIconObject.transform.position = position + Vector2.up * _iconYOffset;
        // 아이콘이 가려지지 않도록 순서를 맨 아래로 내림
        _playerIconObject.transform.SetAsLastSibling();
                
        // doTween을 사용하여 아이콘 위치를 위아래로 움직이도록 설정
        // DoMove 사용 시 스크롤뷰를 움직여도 따라오는 문제가 발생하여 DoLocalMove 사용
        _playerIconObject.transform.DOPause();
        _playerIconObject.transform.DOLocalMove(new Vector2
        (
            _playerIconObject.transform.localPosition.x,
            _playerIconObject.transform.localPosition.y + _dotweenPositionY
        ), _dotweenDuration).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
    }

    /// <summary>
    /// 생성된 플레이어 아이콘 오브젝트 삭제
    /// </summary>
    public void ClearObject()
    {
        if (_playerIconObject == null) return;
        
        _playerIconObject.transform.DOKill();
        Destroy(_playerIconObject);
        _playerIconObject = null;
    }

    private void OnDestroy() => ClearObject();
}
