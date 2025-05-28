using System.Collections.Generic;
using UnityEngine;

// 씬 이동 시 저장해둘 데이터
public class MapData
{
    public Node PlayerNode { get; } // 현재 플레이어 위치
    public List<Node> PlayerPath { get; } // 플레이어가 지나온 경로
    public Map CurrentMap { get; } // 현재 맵
    
    public Vector2 ContentPosition { get; } // 스크롤뷰의 위치(불러 올 때 스크롤 뷰 위치 조절 용)
    
    public MapData(Map currentMap, Node playerNode, List<Node> playerPath, Vector2 contentPosition)
    {
        CurrentMap = currentMap;
        PlayerNode = playerNode;
        PlayerPath = playerPath;
        ContentPosition = contentPosition;
    }
}
