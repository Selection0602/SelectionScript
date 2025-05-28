using System;
using System.Collections.Generic;

public static class RandomUtility
{
    /// <summary>
    /// 딕셔너리 안에서 랜덤한 딕셔너리 값을 반환
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="dictionary"></param>
    /// <returns></returns>
    public static KeyValuePair<TKey, TValue> GetRandomDic<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> dictionary)
    {
        int randomIndex = UnityEngine.Random.Range(0, dictionary.Count);
        int current = 0;

        foreach (var data in dictionary)
        {
            if (current == randomIndex)
            {
                return data;
            }
            current++;
        }
        return default;
    }

    /// <summary>
    /// 딕셔너리 안에서 랜덤한 vaule값을 반환
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="dictionary"></param>
    /// <returns></returns>
    public static TValue GetRandomValue<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> dictionary)
    {
        int randomIndex = UnityEngine.Random.Range(0, dictionary.Count);
        int current = 0;

        foreach (var data in dictionary.Values)
        {
            if (current == randomIndex)
            {
                return data;
            }
            current++;
        }
        return default;
    }

    /// <summary>
    /// 딕셔너리 안에서 랜덤한 키값을 반환
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="dictionary"></param>
    /// <returns></returns>
    public static TKey GetRandomKey<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> dictionary)
    {
        int randomIndex = UnityEngine.Random.Range(0, dictionary.Count);
        int current = 0;

        foreach (var data in dictionary.Keys)
        {
            if (current == randomIndex)
            {
                return data;
            }
            current++;
        }
        return default;
    }

    /// <summary>
    /// 리스트 안에서 랜덤한 값을 반환
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public static T GetRandomInList<T>(List<T> list)
    {
        int randomIndex = UnityEngine.Random.Range(0, list.Count);
        int current = 0;

        foreach (var data in list)
        {
            if (current == randomIndex)
            {
                return data; 
            }
            current++;
        }
        return default;
    }

    /// <summary>
    /// 랜덤한 int값을 반환
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static int GetRandomIndex(int min,int max)
    {
        return UnityEngine.Random.Range(min, max);
    }
    
    /// <summary>
    /// 랜덤한 enum 값 반환
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T GetRandomEnum<T>() where T : Enum
    {
        T[] values = (T[])Enum.GetValues(typeof(T));
        int randomIndex = UnityEngine.Random.Range(0, values.Length);
        return values[randomIndex];
    }
    
    /// <summary>
    /// 배열 내 랜덤한 값 반환
    /// </summary>
    /// <param name="array"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T GetRandomFromArray<T>(T[] array)
    {
        int randomIndex = UnityEngine.Random.Range(0, array.Length);
        return array[randomIndex];
    }
}
