using UnityEngine;

public class NodeActionHandler : MonoBehaviour
{
    [SerializeField] private NodeTypeEventChannel _nodeTypeEventChannel;
    
    /// <summary>
    /// 노드 타입에 맞게 씬 변경
    /// </summary>
    /// <param name="type">이동 할 씬을 판단할 노드 타입</param>
    public void HandleNodeAction(NodeType type) => _nodeTypeEventChannel?.Invoke(type);
}
