using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializableDic<T1, T2>
{
    public List<SerializableData<T1, T2>> dataList = new();

    // 인덱서 정의
    public T2 this[T1 key]
    {
        get
        {
            try //키값 반환 시도
            {
                return GetValue(key); //문제 없을시 키값 반환 
            }
            catch (KeyNotFoundException e)
            {
                Debug.LogError($"{e.Message}"); // 키값 반환에 문제가 있으면 에러 표시
                return default; //default 반환
            }
        }
        set
        {
            for (int i = 0; i < dataList.Count; i++)
            {
                if (dataList[i].Key.Equals(key))
                {
                    dataList[i].Value = value; //vaule값 설정
                    return;
                }
            }
            // 키가 없으면 새로 추가
            dataList.Add(new SerializableData<T1, T2>() { Key = key, Value = value });
        }
    }

    //키값으로 value값을 가져오기
    public T2 GetValue(T1 key)
    {
        foreach (var data in dataList)
        {
            if (data.Key.Equals(key))
            {
                return data.Value;
            }
        }
        throw new KeyNotFoundException($"{key}의 vaule값이 존재하지않음");
    }
}

[System.Serializable]
public class SerializableData<T1, T2>
{
    public T1 Key;
    public T2 Value;
}