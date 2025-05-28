using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "NodeSceneMapping", menuName = "Map/NodeSceneMapping")]
public class NodeSceneMapping : ScriptableObject
{
    [System.Serializable]
    public class NodeTypeMapping
    {
        public string SceneName;
        public AssetLabelMapping[] LabelMappings;
        public float LoadingTipInterval = 2f;
    }

    [FormerlySerializedAs("nodeTypeMappings")] 
    [SerializeField] private SerializableDic<NodeType, NodeTypeMapping> _nodeTypeMappings =
        new SerializableDic<NodeType, NodeTypeMapping>(); // 노드 타입 별 매핑 데이터

    private readonly NodeType[] _miniGameTypes = 
    { 
        NodeType.MiniGame_01, 
        NodeType.MiniGame_02, 
        NodeType.MiniGame_03 
    };

    
    /// <summary>
    /// 노드 타입에 맞게 매핑데이터 가져오기
    /// </summary>
    /// <param name="nodeType">매핑데이터를 가져올 노드타입</param>
    /// <returns>타입에 맞는 매핑데이터 반환</returns>
    public NodeTypeMapping GetNodeTypeMapping(NodeType nodeType)
    {
        return nodeType switch
        {
            NodeType.Trap => _nodeTypeMappings[RandomUtility.GetRandomFromArray(_miniGameTypes)],
            _ => _nodeTypeMappings.GetValue(nodeType)
        };
    }
    
    /// <summary>
    /// 랜덤 이벤트 매핑데이터 및 랜덤 이벤트 enum 값 반환
    /// </summary>
    /// <returns></returns>
    public (NodeTypeMapping, RandomEventType) GetRandomEventMapping()
    {
        var randomEvent = RandomUtility.GetRandomEnum<RandomEventType>();
        return (_nodeTypeMappings[NodeType.RandomEvent], randomEvent);
    }
}
