using System.Collections.Generic;
using UnityEngine;

public class PlayerPathController : MonoBehaviour
{
    // 플레이어가 위치한 노드 변경 시 발생하는 이벤트 채널(MapView.ShowPlayerPosition 함수 등록)
    [SerializeField] private NodeEventChannel _playerNodeEventChannel;
    
    public Node CurrentNode { get; private set; } // 플레이어가 현재 위치한 노드
    public List<Node> Path { get; private set; } = new List<Node>(); // 플레이어가 지나온 노드 리스트

    #region 플레이어 위치 변경 및 설정
    /// <summary>
    /// 플레이어 위치 변경 시도
    /// </summary>
    /// <param name="targetNode">이동 할 노드</param>
    /// <returns>성공 여부 반환</returns>
    public bool TryMoveToNode(Node targetNode)
    {
        if(CurrentNode == null || targetNode == null) return false;
        
        // 현재 노드와 타겟노드가 연결되어 있지 않거나 타겟 노드가 이미 플레이어가 지나온 노드에 포함되었다면 이동 불가
        if(!CurrentNode.ConnectedNodes.Contains(targetNode) || Path.Contains(targetNode)) return false;
        
        // 플레이어가 지나온 경로에 타겟 노드 추가
        Path.Add(targetNode);
        // 플레이어 노드 변경
        CurrentNode = targetNode;
        
        // 플레이어 노드 이벤트 발생
        _playerNodeEventChannel?.Invoke(CurrentNode);

        return true;
    }

    /// <summary>
    /// 플레이어 위치 설정(불러오기 시 사용)
    /// </summary>
    /// <param name="node">이동할 노드</param>
    /// <param name="path">이동 한 경로</param>
    public void SetPosition(Node node, List<Node> path = null)
    {
        CurrentNode = node;
        Path = path ?? new List<Node> { node };
        
        _playerNodeEventChannel?.Invoke(CurrentNode);
    }
    #endregion
    
    /// <summary>
    /// 플레이어가 지나온 노드인지 확인
    /// </summary>
    /// <param name="node">확인 할 노드</param>
    /// <returns>경로에 포함되어있다면 true</returns>
    public bool HasVisitedNode(Node node) => Path.Contains(node);
}
