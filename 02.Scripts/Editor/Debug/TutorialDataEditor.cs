using UnityEngine;
using UnityEditor;

public class TutorialDataEditor : EditorWindow
{
    static void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("PlayerPrefs 데이터 모두 삭제");
    }

    [MenuItem("Tools/Tutorial Data Editor")]
    public static void ShowWindow()
    {
        GetWindow<TutorialDataEditor>("Tutorial Data Editor");
    }

    void OnGUI()
    {
        if (GUILayout.Button("PlayerPrefs 데이터 모두 삭제하기"))
        {
            ClearPlayerPrefs();
        }
    }
}