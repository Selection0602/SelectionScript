#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class SceneChanger : Editor
{
    [MenuItem("Ironcow/Scenes/StartScene &1")]
    public static void ChangeStartScene()
    {
        EditorSceneManager.OpenScene(Application.dataPath + "/01.Scenes/StartScene.unity");
    }

    [MenuItem("Ironcow/Scenes/LoadingScene &2")]
    public static void ChangeLoadingScene()
    {
        EditorSceneManager.OpenScene(Application.dataPath + "/01.Scenes/LoadingScene.unity");
    }

    [MenuItem("Ironcow/Scenes/MapScene &3")]
    public static void ChangeMapScene()
    {
        EditorSceneManager.OpenScene(Application.dataPath + "/01.Scenes/MapScene.unity");
    }

    [MenuItem("Ironcow/Scenes/BattleScene &4")]
    public static void ChangeBattleScene()
    {
        EditorSceneManager.OpenScene(Application.dataPath + "/01.Scenes/BattleScene.unity");
    }

    [MenuItem("Ironcow/Scenes/MiniGameScene &5")]
    public static void ChangeMiniGameScene()
    {
        EditorSceneManager.OpenScene(Application.dataPath + "/01.Scenes/MiniGameScene.unity");
    }

    [MenuItem("Ironcow/Scenes/EndingScene &6")]
    public static void ChangeEndingScene()
    {
        EditorSceneManager.OpenScene(Application.dataPath + "/01.Scenes/EndingScene.unity");
    }
}
#endif