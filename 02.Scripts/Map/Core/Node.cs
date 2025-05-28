using System.Collections.Generic;

public class Node
{
    public int RoomPosition { get; private set; } // 노드가 위치한 방의 인덱스
    public int Layer { get; private set; } // 노드가 위치한 층의 인덱스
    public NodeType Type { get; private set; } // 노드 타입

    // 연결된 노드들을 저장해둘 리스트(노드 연결 시 사용)
    public List<Node> ConnectedNodes { get; private set; } = new List<Node>();

    public Node(int roomPosition, int layer, NodeType type)
    {
        RoomPosition = roomPosition;
        Layer = layer;
        Type = type;
    }
}