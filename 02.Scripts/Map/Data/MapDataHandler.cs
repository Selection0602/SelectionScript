using System.Collections.Generic;
using UnityEngine;

public class MapDataHandler : MonoBehaviour
{
    /// <summary>
    /// 맵 데이터 가져오기
    /// </summary>
    /// <returns>저장된 맵 데이터 반환</returns>
    public MapData GetSavedMapData() => Manager.Instance.MapManager.SavedMapData;

    /// <summary>
    /// 맵 데이터 맵 매니저에 저장하기
    /// </summary>
    /// <param name="map">현재 맵</param>
    /// <param name="playerNode">플레이어가 위치한 노드</param>
    /// <param name="playerPath">플레이어가 지나온 경로 리스트</param>
    /// <param name="contentPosition">스크롤뷰 컨텐츠 현재 위치(rect.offSetMin.x, rect.offSetMax.x)</param>
    public void SaveMapData(Map map, Node playerNode, List<Node> playerPath, Vector2 contentPosition)
    {
        // 맵 데이터 저장
        MapData mapData = new MapData(map, playerNode, playerPath, contentPosition);
        
        // 맵 매니저에 저장
        Manager.Instance.MapManager.SavedMapData = mapData;
    }
}
