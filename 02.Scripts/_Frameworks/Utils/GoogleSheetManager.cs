#if UNITY_EDITOR
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Unity.EditorCoroutines.Editor;

public partial class GoogleSheetManager : EditorWindow
{
    string sheetAPIurl = "https://script.google.com/macros/s/AKfycbxvP93AbkgtlWp89hxttReZvTkJ_l_PdPqhZpWet68tGUkCW8tH9rKLpQ6QSM0g277R/exec";
    string sheeturl = "https://docs.google.com/spreadsheets/d/1ZxKiLpz3T-6SLVPkV_haOC71cf_OSp_ydi_bK4Ao6gk";

    private List<SheetData> sheets = new List<SheetData>();
    private int selectedSheetIndex = 0;
    private bool isFetching = false;

    [MenuItem("Tools/Google Sheet Parsing Tool")]
    public static void ShowWindow()
    {
        EditorWindow window = GetWindow(typeof(GoogleSheetManager));
        window.titleContent = new GUIContent("Google Sheet Parser");
        window.maxSize = new Vector2(600, 400);
        window.minSize = new Vector2(600, 400);
    }

#region OnGUI
    private void OnGUI()
    {
        GUILayout.Space(10);
        if (isFetching)
        {
            EditorGUILayout.LabelField("Fetching data...");
        }
        else
        {
            if (sheets.Count > 0)
            {
                string[] sheetNames = sheets.Select(s => s.sheetName).ToArray();
                selectedSheetIndex = EditorGUILayout.Popup("Select Sheet", selectedSheetIndex, sheetNames);
            }
            else
            {
                EditorGUILayout.LabelField("No sheets found.");
            }

            GUILayout.Space(20);
            if (GUILayout.Button("Fetch Sheets Data", GUILayout.Height(40)))
            {
                EditorCoroutineUtility.StartCoroutine(FetchSheetsData(), this);
            }
        }

        GUILayout.Space(30);
        if (GUILayout.Button("Parse Selected Sheet and Create Class", GUILayout.Height(40)))
        {
            if (sheets.Count > 0)
            {
                ParseSelectedSheet();
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Please fetch sheet names and select a sheet.", "OK");
            }
        }

        GUILayout.Space(30);
        if (GUILayout.Button("Make ScriptableObject from json", GUILayout.Height(40)))
        {
            if (sheets.Count > 0)
            {
                ParseJsonToSO();
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Please fetch sheet names and select a sheet.", "OK");
            }
        }
    }
    #endregion

#region FetchSheetData
    private IEnumerator FetchSheetsData()
    {
        isFetching = true;
        UnityWebRequest request = UnityWebRequest.Get(sheetAPIurl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log(request.downloadHandler.text);
            ProcessSheetsData(request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Error fetching data: " + request.error);
        }

        isFetching = false;
        Repaint();
    }

    private void ProcessSheetsData(string json)
    {
        var sheetsData = JsonUtility.FromJson<SheetDataList>(json);
        sheets.Clear();
        sheets.AddRange(sheetsData.sheetData);

        if (sheets.Count > 0)
        {
            selectedSheetIndex = 0;
        }
    }
#endregion

#region ParseSheet
    private void ParseSelectedSheet()
    {
        var selectedSheet = sheets[selectedSheetIndex];
        string jsonFileName = RemoveSpecialCharacters(selectedSheet.sheetName);
        Debug.Log($"Selected Sheet: {selectedSheet.sheetName}, Sheet ID: {selectedSheet.sheetId}");

        EditorCoroutineUtility.StartCoroutine(ParseGoogleSheet(jsonFileName, selectedSheet.sheetId.ToString()), this);
    }

    private string RemoveSpecialCharacters(string sheetName)
    {
        return Regex.Replace(sheetName, @"[^a-zA-Z0-9\s]", "").Replace(" ", "_");
    }

    private IEnumerator ParseGoogleSheet(string jsonFileName, string gid, bool notice = true)
    {
        string sheetUrl = $"{sheeturl}/export?format=tsv&gid={gid}";

        UnityWebRequest request = UnityWebRequest.Get(sheetUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            EditorUtility.DisplayDialog("Fail", "GoogleConnect Fail!", "OK");
            yield break;
        }

        string data = request.downloadHandler.text;
        List<string> rows = ParseTSVData(data);

        if (rows == null || rows.Count < 4)
        {
            Debug.LogError("Not enough data rows to parse.");
            yield break;
        }

        HashSet<int> dbIgnoreColumns = GetDBIgnoreColumns(rows[0]);
        var keys = rows[1].Split('\t').ToList();
        var types = rows[2].Split('\t').ToList();

        JArray jArray = new JArray();
        for (int i = 3; i < rows.Count; i++)
        {
            var rowData = rows[i].Split('\t').ToList();

            if (rowData[0].Equals("DB_IGNORE", StringComparison.OrdinalIgnoreCase))
            {
                Debug.Log($"Row {i + 1} ignored due to DB_IGNORE");
                continue;
            }

            var rowObject = ParseRow(keys, types, rowData, dbIgnoreColumns);
            if (rowObject != null)
            {
                jArray.Add(rowObject);
            }
        }

        SaveJsonToFile(jsonFileName, jArray);
        CreateDataClass(jsonFileName, keys, types, dbIgnoreColumns);
        CreateDataSO(jsonFileName, keys, types, dbIgnoreColumns);

        if (notice)
        {
            EditorUtility.DisplayDialog("Success", "Sheet parsed and saved as JSON successfully!", "OK");
            AssetDatabase.Refresh();
        }
    }

    private void SaveJsonToFile(string jsonFileName, JArray jArray)
    {
        string directoryPath = Path.Combine(Application.dataPath, "Resources", "JsonFiles");

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string jsonFilePath = Path.Combine(directoryPath, $"{jsonFileName}.json");
        string jsonData = "{\"datas\":" + jArray.ToString() + "}";

        File.WriteAllText(jsonFilePath, jsonData);
        Debug.Log($"Saved JSON to: {jsonFilePath}");
    }

    private string CreateDataSO(string fileName, List<string> keys, List<string> types, HashSet<int> dbIgnoreColumns)
    {
        string className = fileName;
        string directoryPath = Path.Combine(Application.dataPath, "02.Scripts/Data");

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string dataClassPath = Path.Combine(directoryPath, $"{className}SO.cs");

        using (StreamWriter writer = new StreamWriter(dataClassPath))
        {
            writer.WriteLine("using UnityEngine;");
            writer.WriteLine("using System.Collections.Generic;");
            writer.WriteLine($"[CreateAssetMenu(fileName = \"{className}\", menuName = \"GameData/Create{className}Data\")]\r\n");
            writer.WriteLine($"public class {className}SO : ScriptableObject");
            writer.WriteLine("{");

            for (int i = 0; i < keys.Count; i++)
            {
                if (dbIgnoreColumns.Contains(i)) continue;

                string fieldType = ConvertTypeToCSharp(types[i]);
                string fieldName = keys[i];

                if (!string.IsNullOrEmpty(fieldName))
                {
                    writer.WriteLine($"\tpublic {fieldType} {fieldName};");
                }
            }

            writer.WriteLine();
            writer.WriteLine("}");
        }

        Debug.Log($"Saved SO class to: {dataClassPath}");
        AssetDatabase.Refresh();

        return $"{className}SO";
    }

    private string CreateDataClass(string fileName, List<string> keys, List<string> types, HashSet<int> dbIgnoreColumns)
    {
        string className = fileName;
        string directoryPath = Path.Combine(Application.dataPath, "Resources/DataClass");

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string dataClassPath = Path.Combine(directoryPath, $"{className}Data.cs");

        using (StreamWriter writer = new StreamWriter(dataClassPath))
        {
            writer.WriteLine("using System.Collections.Generic;");
            writer.WriteLine("[System.Serializable]");
            writer.WriteLine($"public class {className}Data");
            writer.WriteLine("{");

            for (int i = 0; i < keys.Count; i++)
            {
                if (dbIgnoreColumns.Contains(i)) continue;

                string fieldType = ConvertTypeToCSharp(types[i]);
                string fieldName = keys[i];

                if (!string.IsNullOrEmpty(fieldName))
                {
                    writer.WriteLine($"\tpublic {fieldType} {fieldName};");
                }
            }
            writer.WriteLine();
            writer.WriteLine("}");
        }

        Debug.Log($"Saved ScriptableObject class to: {dataClassPath}");
        AssetDatabase.Refresh();

        return className;
    }
    #endregion

#region MakeParseSOMenu
    private void ParseJsonToSO()
    {
        EditorCoroutineUtility.StartCoroutine(ParseToSO(), this);
    }

    private IEnumerator ParseToSO(bool notice = true)
    {
        CreateToolMenu();
        yield return null;

        if (notice)
        {
            EditorUtility.DisplayDialog("Success", "Make Json to SO Menu successfully!", "OK");
            AssetDatabase.Refresh();
        }
    }

    private void CreateToolMenu()
    {
        string directoryPath = Path.Combine(Application.dataPath, "02.Scripts/Editor");

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string dataClassPath = Path.Combine(directoryPath, $"JsonToSO.cs");

        using (StreamWriter writer = new StreamWriter(dataClassPath))
        {
            writer.WriteLine("using UnityEngine;");
            writer.WriteLine("using UnityEditor;");
            writer.WriteLine("[System.Serializable]");
            writer.WriteLine($"public class JsonToSO : MonoBehaviour");
            writer.WriteLine("{");

            for (int i = 0; i < sheets.Count; i++)
            {
                writer.WriteLine($"\t[MenuItem(\"Tools/JsonToSO/Create{sheets[i].sheetName}SO\")]");
                writer.WriteLine($"\tstatic void {sheets[i].sheetName}DataInit()");
                writer.WriteLine("\t{");
                writer.WriteLine($"\t\tDynamicMenuCreator.CreateMenusFromJson<{sheets[i].sheetName}Data>(\"{sheets[i].sheetName}.json\", typeof({sheets[i].sheetName}SO));");
                writer.WriteLine("\t}");
            }
            writer.WriteLine();
            writer.WriteLine("}");
        }

        Debug.Log($"Saved ScriptableObject class to: {dataClassPath}");
        AssetDatabase.Refresh();
    }
    #endregion

#region Util
    // TSV Data Parsing
    private List<string> ParseTSVData(string data)
    {
        return data.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    // DB_IGNORE Column Filter
    private HashSet<int> GetDBIgnoreColumns(string headerRow)
    {
        var dbIgnoreColumns = new HashSet<int>();
        var firstRow = headerRow.Split('\t').ToList();

        for (int i = 0; i < firstRow.Count; i++)
        {
            if (firstRow[i].Equals("DB_IGNORE", StringComparison.OrdinalIgnoreCase))
            {
                dbIgnoreColumns.Add(i);
                Debug.Log($"Column {i + 1} ignored due to DB_IGNORE");
            }
        }

        return dbIgnoreColumns;
    }

    // Row Parse
    private JObject ParseRow(List<string> keys, List<string> types, List<string> rowData, HashSet<int> dbIgnoreColumns)
    {
        var rowObject = new JObject();

        for (int j = 0; j < keys.Count && j < rowData.Count; j++)
        {
            if (dbIgnoreColumns.Contains(j)) continue;

            string key = keys[j];
            string type = types[j];
            string value = rowData[j].Trim();

            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value)) continue;

            rowObject[key] = ConvertValue(value, type);
        }

        return rowObject.HasValues ? rowObject : null;
    }

    private JToken ConvertValue(string value, string type)
    {
        switch (type.Trim()) // 불필요한 공백 제거
        {
            case "int": return int.TryParse(value, out int intValue) ? intValue : 0;
            case "long": return long.TryParse(value, out long longValue) ? longValue : 0L;
            case "float": return float.TryParse(value, out float floatValue) ? floatValue : 0.0f;
            case "double": return double.TryParse(value, out double doubleValue) ? doubleValue : 0.0d;
            case "bool": return bool.TryParse(value, out bool boolValue) ? boolValue : false;
            case "byte": return byte.TryParse(value, out byte byteValue) ? byteValue : (byte)0;
            case "int[]": return JArray.FromObject(value.Split(',').Select(v => int.TryParse(v.Trim(), out int tempInt) ? tempInt : 0));
            case "float[]": return JArray.FromObject(value.Split(',').Select(v => float.TryParse(v.Trim(), out float tempFloat) ? tempFloat : 0.0f));
            case "string[]": return JArray.FromObject(value.Split(',').Select(v => v.Trim()));
            case "DateTime": return DateTime.TryParse(value, out DateTime dateTimeValue) ? dateTimeValue : DateTime.MinValue; // DateTime 변환
            case "TimeSpan": return TimeSpan.TryParse(value, out TimeSpan timeSpanValue) ? timeSpanValue : TimeSpan.Zero;
            case "Guid": return Guid.TryParse(value, out Guid guidValue) ? guidValue.ToString() : Guid.Empty.ToString();
            case "List<int>": return JArray.FromObject(value.Split(',').Select(v => int.TryParse(v.Trim(), out int tempInt) ? tempInt : 0).ToList());
            case "List<float>": return JArray.FromObject(value.Split(',').Select(v => float.TryParse(v.Trim(), out float tempFloat) ? tempFloat : 0.0f).ToList());
            case "List<string>": return JArray.FromObject(value.Split(',').Select(v => v.Trim()).ToList());
            case "List<bool>": return JArray.FromObject(value.Split(',').Select(v => bool.TryParse(v.Trim(), out bool tempBool) ? tempBool : false).ToList());
            case "List<long>": return JArray.FromObject(value.Split(',').Select(v => long.TryParse(v.Trim(), out long tempLong) ? tempLong : 0L).ToList());
            case "List<double>": return JArray.FromObject(value.Split(',').Select(v => double.TryParse(v.Trim(), out double tempDouble) ? tempDouble : 0.0d).ToList());
            case "Sprite": return null;
            default:
                // Enum 처리 시도
                var enumType = Type.GetType(type); 
                if (enumType != null && enumType.IsEnum)
                {
                    try
                    {
                        object enumValue = Enum.Parse(enumType, value, ignoreCase: true);
                        return JToken.FromObject(enumValue);
                    }
                    catch
                    {
                        Debug.LogWarning($"Enum 파싱 실패: value={value}, type={type}");
                    }
                }
                return value; // 기본적으로 문자열로 반환
        }
    }

    private string ConvertTypeToCSharp(string type)
    {
        switch (type.Trim()) 
        {
            case "int": return "int";
            case "long": return "long";
            case "float": return "float";
            case "double": return "double";
            case "bool": return "bool";
            case "byte": return "byte";
            case "int[]": return "int[]";
            case "float[]": return "float[]";
            case "string[]": return "string[]";
            case "DateTime": return "System.DateTime"; 
            case "TimeSpan": return "System.TimeSpan";
            case "Guid": return "System.Guid";
            case "List<int>": return "System.Collections.Generic.List<int>";
            case "List<float>": return "System.Collections.Generic.List<float>";
            case "List<string>": return "System.Collections.Generic.List<string>";
            case "List<bool>": return "System.Collections.Generic.List<bool>";
            case "List<long>": return "System.Collections.Generic.List<long>";
            case "List<double>": return "System.Collections.Generic.List<double>";
            case "List<byte>": return "System.Collections.Generic.List<byte>";
            case "Sprite": return "UnityEngine.Sprite";
            default:
                var enumType = Type.GetType(type);
                if (enumType != null && enumType.IsEnum)
                {
                    return enumType.ToString();
                }

                return "string";
        }
    }
#endregion
}

[System.Serializable]
public class SheetData
{
    public string sheetName;
    public int sheetId;
}

[System.Serializable]
public class SheetDataList
{
    public SheetData[] sheetData;
}
#endif