using UnityEngine;

public class MapInitializer : MonoBehaviour
{
    private Map _currentMap;
    private Node _startNode;
    
    private MapCreator _mapCreator = new MapCreator();
    
    /// <summary>
    /// 맵 생성 함수
    /// </summary>
    /// <param name="width">층 당 방 수</param>
    /// <param name="height">층 수</param>
    /// <param name="nodeTable">노드 확률 등 정보가 담긴 테이블</param>
    /// <param name="seed">랜덤 시드값</param>
    /// <returns>생성된 맵 반환</returns>
    public Map CreateMap(int width, int height, NodeTable nodeTable,int seed)
    {
        _currentMap = _mapCreator.CreateMap(width, height, nodeTable, seed);
        _startNode = _mapCreator.StartNode;
        return _currentMap;
    }

    #region 시작 노드 가져오기
    /// <summary>
    /// 시작 노드 반환 함수
    /// </summary>
    /// <returns>시작 노드 반환</returns>
    public Node GetStartNode() => _startNode;
    #endregion
}
