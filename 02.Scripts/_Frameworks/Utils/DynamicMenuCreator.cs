using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Reflection;

public class DynamicMenuCreator : MonoBehaviour
{
    public static void CreateMenusFromJson<T>(string jsonFileName, System.Type type) where T : new()
    {
        string path = $"Assets/Resources/JsonFiles/{jsonFileName}";
        string json = File.ReadAllText(path);
        List<T> dataArray = new List<T>(Utils.FromJson<T>(json));

        for (int i = 0; i < dataArray.Count; i++)
        {
            CreateScriptableObject(dataArray[i], type, i);
        }
    }

    static void CreateScriptableObject<T>(T data, System.Type type, int index) where T : new()
    {
        ScriptableObject asset = ScriptableObject.CreateInstance(type);
        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(data), asset);

        string directoryPath = $"Assets/05.ScriptableObjects/{type.Name}";

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
#if UNITY_EDITOR
        AssetDatabase.CreateAsset(asset, $"{directoryPath}/{GetFirstVariableValue(data)}.asset");
        AssetDatabase.SaveAssets();
#endif
    }

    static string GetFirstVariableValue<T>(T data)
    {
        FieldInfo firstField = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance)[0];
        object value = firstField.GetValue(data);
        return value.ToString();
    }
}
