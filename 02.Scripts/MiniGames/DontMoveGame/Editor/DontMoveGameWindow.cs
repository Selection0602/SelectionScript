#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class DontMoveGameWindow : EditorWindow
{
    private static DontMoveGameWindow _dontMoveGameEWindow;
    Vector2 _scrollPos;

    [MenuItem("Window/Dont Move Game Tester")]

    //창 초기 세팅
    private static void SetUp()
    {
        _dontMoveGameEWindow = GetWindow<DontMoveGameWindow>();

        _dontMoveGameEWindow.titleContent = new GUIContent("Dont Move Game Tester");

        _dontMoveGameEWindow.ShowUtility();
    }

    private void OnGUI()
    {
        //커스텀 에디터의 항목이 많을 경우 스크롤을 생성
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

        //안내문
        EditorGUILayout.HelpBox("플레이 모드일때, MiniGameScene_01에서만 활성화됩니다.", MessageType.Info);

        //테스트중일때 && MiniGameScene_01에서만 실행되도록
        if (Application.isPlaying && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MiniGameScene_01")
        {
            //매니저 찾기
            DontMoveManager manager = FindObjectOfType<DontMoveManager>();

            //CheatMode 변경
            if (GUILayout.Button("cheatMode 상태 변경 : " + manager.CheatMode))
            {
                manager.ChangeCheatMode();
            }

            //다음으로 등장하는 이벤트를 미리 결정하기
            GUILayout.Label("다음 이벤트 확정 등장");
            EditorGUILayout.BeginVertical("box");
            for (int i = 1; i <= manager.eventDict.Count; i++)
            {
                if (GUILayout.Button(i + "번 이벤트 - " + manager.eventDict[i].Description))
                {
                    manager.BookedEventNum = i;
                }
            }
            EditorGUILayout.EndVertical();

            //성공, 실패 스택 쌓기
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("성공/실패 스택 최대로 올리기 - 성공이 우선 판정");
            if (GUILayout.Button("성공 스택 채우기 / 현재 스택 : " + manager.CurSuccessCount))
            {
                manager.MakeGameSuccess();
            }
            if (GUILayout.Button("실패 스택 채우기 / 현재 스택 : " + manager.CurFailedCount))
            {
                manager.MakeGameFailed();
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndScrollView();
    }
}
#endif