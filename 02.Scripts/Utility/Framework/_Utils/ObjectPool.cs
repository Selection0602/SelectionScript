using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : MonoBehaviour
{
    private Queue<T> _pool = new Queue<T>();
    protected GameObject _prefab;
    protected Transform _parent { get; set; }

    public ObjectPool(GameObject prefab) 
    {
        _prefab = prefab;
    }

    public void SetParent(Transform parent) 
    {
        _parent = parent;
    }
    
    public void InitializePool(int count) 
    {
        for (int i = 0; i < count; i++) 
        {
            T obj = CreateNewObject();
            obj.gameObject.SetActive(false);
            Return(obj);
        }
    }
    
    // 오브젝트 가져오기
    public T Get() 
    {
        if (_pool.Count > 0) 
        {
            T obj = _pool.Dequeue();
            obj.gameObject.SetActive(true);
            return obj;
        } 
        else 
        {
            return CreateNewObject();
        }
    }

    // 오브젝트 반환하기
    public void Return(T obj) 
    {
        obj.gameObject.SetActive(false);
        _pool.Enqueue(obj);
    }

    // 새로운 오브젝트 생성
    protected T CreateNewObject()
    {
        GameObject obj = Object.Instantiate(_prefab);
        if (_parent != null)
        {
            obj.transform.SetParent(_parent);
        }
        return obj.GetComponent<T>();
    }
}