using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BattleDebugWindow : EditorWindow
{
    private NodeType _selectedNodeType = NodeType.NormalBattle;
    private DebuffType _debuffType = DebuffType.Burn;

    [MenuItem("Tools/Battle Debugger")]
    public static void ShowWindow()
    {
        GetWindow<BattleDebugWindow>("Battle Debugger");
    }

    private void OnGUI()
    {
        GUILayout.Label("배틀씬 테스트 툴", EditorStyles.boldLabel);

        GUILayout.Space(10);
        _selectedNodeType = (NodeType)EditorGUILayout.EnumPopup("노드 타입 선택", _selectedNodeType);

        GUI.enabled = Application.isPlaying;

        GUILayout.Space(10);
        if (GUILayout.Button("선택한 노드타입 스테이지 클리어"))
        {
            var battleManager = (SceneBase.Current as BattleSceneController).BattleManager;
            battleManager.Node = _selectedNodeType;
            battleManager.GameClear();
        }

        GUILayout.Space(10);
        if (GUILayout.Button("플레이어 10데미지"))
        {
            var battleManager = (SceneBase.Current as BattleSceneController).BattleManager;
            battleManager.Player.Damage(10);
        }

        GUILayout.Space(10);
        _debuffType = (DebuffType)EditorGUILayout.EnumPopup("디버프 선택", _debuffType);

        GUILayout.Space(10);
        if (GUILayout.Button("플레이어에게 선택한 디버프 부여"))
        {
            var player = (SceneBase.Current as BattleSceneController).BattleManager.Player;
            var debuff = (SceneBase.Current as BattleSceneController).BattleDataManager.DebuffDatas[_debuffType];
            var command = new UseDebuffCard(new List<CharacterBase> { player }, debuff);
            command.Execute();
        }

        GUILayout.Space(10);
        if (GUILayout.Button("플레이어 공격력 30 상승"))
        {
            var player = (SceneBase.Current as BattleSceneController).BattleManager.Player;
            player.IncreaseAttackPower(30);
        }

        GUILayout.Space(10);
        if (GUILayout.Button("일반 공격 카드 뽑기"))
        {
            var player = (SceneBase.Current as BattleSceneController).BattleManager.Player;
            player.DeckHandler.DrawCard("Attack",true);
        }

        GUILayout.Space(10);
        if (GUILayout.Button("도깨비 불 카드 뽑기"))
        {
            var player = (SceneBase.Current as BattleSceneController).BattleManager.Player;
            player.DeckHandler.DrawCard("WillOWisp",true);
        }

        GUILayout.Space(10);
        if (GUILayout.Button("등가교환 카드 뽑기"))
        {
            var player = (SceneBase.Current as BattleSceneController).BattleManager.Player;
            player.DeckHandler.DrawCard("Exchange", true);
        }

        GUILayout.Space(10);
        if (GUILayout.Button("일반공격강화 카드 뽑기"))
        {
            var player = (SceneBase.Current as BattleSceneController).BattleManager.Player;
            player.DeckHandler.DrawCard("ReinforceAttack", true);
        }

        GUILayout.Space(10);
        if (GUILayout.Button("흡혈강화 카드 뽑기"))
        {
            var player = (SceneBase.Current as BattleSceneController).BattleManager.Player;
            player.DeckHandler.DrawCard("ReinforceDrainLife", true);
        }

        GUILayout.Space(10);
        if (GUILayout.Button("연속공격강화 카드 뽑기"))
        {
            var player = (SceneBase.Current as BattleSceneController).BattleManager.Player;
            player.DeckHandler.DrawCard("ReinforceCAttack", true);
        }

        GUILayout.Space(10);
        if (GUILayout.Button("카드 1장 뽑기"))
        {
            var player = (SceneBase.Current as BattleSceneController).BattleManager.Player;
            player.DeckHandler.DrawCard(1, true, player);
        }
        
        GUILayout.Space(10);
        if (GUILayout.Button("코스트 회복"))
        {
            var player = (SceneBase.Current as BattleSceneController).BattleManager.Player;
            player.Cost.RecoveryCost();
        }

        GUILayout.Space(10);
        if (GUILayout.Button("블록커 활성"))
        {
            BlockerManager.Instance.Active();
        }

        GUILayout.Space(10);
        if (GUILayout.Button("블록커 비활성"))
        {
            BlockerManager.Instance.InActive();
        }

        GUI.enabled = true;
    }
}
