using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;

public class JenkinsAutoBuild
{
    public static void BuildAddressables()
    {
        AddressableAssetSettings.BuildPlayerContent();
    }

    public static void BuildWindows()
    {
        EnsurePlatform(BuildTarget.StandaloneWindows64, BuildTargetGroup.Standalone);

        BuildAddressables();
        BuildPipeline.BuildPlayer(
            FindEnabledEditorScenes(),
            "Builds/Windows/Selection.exe",
            BuildTarget.StandaloneWindows64,
            BuildOptions.None);
    }

    public static void BuildWebGL()
    {
        EnsurePlatform(BuildTarget.WebGL, BuildTargetGroup.WebGL);

        BuildAddressables();
        BuildPipeline.BuildPlayer(
            FindEnabledEditorScenes(),
            "Builds/WebGL",
            BuildTarget.WebGL,
            BuildOptions.None);
    }

    private static string[] FindEnabledEditorScenes()
    {
        List<string> EditorScenes = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (!scene.enabled) continue;
            EditorScenes.Add(scene.path);
        }
        return EditorScenes.ToArray();
    }

    private static void EnsurePlatform(BuildTarget target, BuildTargetGroup group)
    {
        if (EditorUserBuildSettings.activeBuildTarget != target)
        {
            UnityEngine.Debug.Log($"Switching platform to {target}...");
            EditorUserBuildSettings.SwitchActiveBuildTarget(group, target);
        }
    }
}
