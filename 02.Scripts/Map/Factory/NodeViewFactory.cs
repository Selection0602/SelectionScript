using UnityEngine;

public class NodeViewFactory : MapObjectFactory<NodeView>
{
    [Header("Node Settings")]
    [SerializeField] private float _nodeSpacingX; // 노드 간 가로 간격
    [SerializeField] private float _nodeSpacingY; // 노드 간 세로 간격
    [SerializeField] private Vector2 _startPoint; // 노드 생성 시작 지점

    private NodeTable _nodeTable;
    
    /// <summary>
    /// 노드 테이블 설정 함수
    /// </summary>
    /// <param name="nodeTable">노드 색/이미지 데이터가 담긴 테이블</param>
    public void SetNodeTable(NodeTable nodeTable) => _nodeTable = nodeTable;
    
    /// <summary>
    /// NodeView 생성 함수
    /// </summary>
    /// <param name="nodeData">생성할 노드 데이터</param>
    /// <param name="mapRoomCount">맵의 넓이</param>
    /// <returns>생성한 노드뷰 반환</returns>
    public NodeView CreateNodeView(Node nodeData, int mapRoomCount)
    {
        if (nodeData == null || nodeData.Type == NodeType.None) return null;

        // 노드 오브젝트 생성 및 위치 기준으로 이름 설정
        NodeView nodeView = CreateObject();
        nodeView.name = $"Node:{nodeData.Layer},{nodeData.RoomPosition}";
        
        // 노드 오브젝트의 위치 설정
        RectTransform nodeRect = nodeView.RectTransform;
        nodeRect.anchoredPosition = CalculateNodePosition(nodeData, mapRoomCount);
        
        // NodeView 컴포넌트 가져오기 및 초기화
        nodeView.Setup(nodeData, _nodeTable.NodeTypeDataDict[nodeData.Type]);

        return nodeView;
    }
    
    // 씬에 생성 할 노드 위치 계산 함수
    private Vector2 CalculateNodePosition(Node node, int mapRoomCount)
    {
        // 옆으로 정렬하기 위해 x, y값을 반대로 계산
        // node.Y * 설정한 x 간격 + (+,- 랜덤값 주기) * 시작점의 X값
        float newX = node.Layer * Random.Range(_nodeSpacingX - 3, _nodeSpacingX + 3) + _startPoint.x;
        // -(node.X - 맵 넓이의 중앙) * 설정한 y 간격 + (+,- 랜덤값 주기) * 시작점의 Y값
        float newY = -(node.RoomPosition - (mapRoomCount - 1) / 2.0f) * Random.Range(_nodeSpacingY - 10, _nodeSpacingY + 10) + _startPoint.y;

        return new Vector2(newX, newY);
    }
}
