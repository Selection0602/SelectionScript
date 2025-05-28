using UnityEngine;

public abstract class MapObjectFactory<T> : MonoBehaviour where T : Component
{
    [Header("Map Object Factory")]
    [SerializeField] protected T _prefab;
    [SerializeField] protected Transform _parent;

    /// <summary>
    /// 오브젝트 생성 함수
    /// </summary>
    /// <returns>생성된 오브젝트 반환</returns>
    protected virtual T CreateObject() => Instantiate(_prefab, _parent ?? transform);

    /// <summary>
    /// 특정 오브젝트 제거 함수
    /// </summary>
    /// <param name="obj">제거 할 오브젝트</param>
    public virtual void DestroyObject(T obj) => Destroy(obj.gameObject);
    
    /// <summary>
    /// 모든 오브젝트 제거 함수
    /// </summary>
    public virtual void ClearObjects()
    {
        foreach (Transform child in _parent)
        {
            Destroy(child.gameObject);
        }
    }
}
