using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "Node Table", menuName = "Map/Node Table")]
public class NodeTable : ScriptableObject
{
    #region NodeTable에서만 사용하는 클래스들
    [System.Serializable]
    private class FixedFloorNode // 고정 층 노드 클래스
    {
        public NodeType NodeType; // 고정 층 노드 타입
        public List<int> Floors; // 고정 층 노드가 생성될 층 리스트
    }
    
    [System.Serializable]
    private class NodeProbability // 노드 타입 확률 클래스
    {
        public NodeType NodeType; // 확률을 적용할 노드 타입
        [Range(0, 100)] public int Probability; // 확률
    }
    #endregion
    
    [Header("모든 노드 타입 데이터")]
    [SerializeField] private List<NodeTypeData> _nodeTypeDataList; // 모든 노드 타입 데이터들
    public readonly Dictionary<NodeType, NodeTypeData> NodeTypeDataDict = new Dictionary<NodeType, NodeTypeData>();
    
    [Header("노드 타입 확률 및 고정 층 노드")]
    [SerializeField] private List<NodeProbability> _nodeProbabilities; // 노드별 등장 확률
    [SerializeField] private List<FixedFloorNode> _fixedFloorNodes = new List<FixedFloorNode>(); // 고정된 층에 등장할 노드들
    private readonly Dictionary<int, NodeType> _fixedFloorNodeDict = new Dictionary<int, NodeType>();

    [Header("특수 노드 등장 확률")]
    [SerializeField] private int _extraNodeProbability; // 특수 노드 등장 확률
    public int ExtraNodeProbability => _extraNodeProbability;
    
    private bool _isInitialized = false; // 초기화 여부

    public void Initialize()
    {
        NodeTypeDataDict.Clear();
        _fixedFloorNodeDict.Clear();
        
        // 리스트에 넣어둔 노드 타입 별 데이터 딕셔너리로 캐싱
        foreach (var typeData in _nodeTypeDataList)
            NodeTypeDataDict[typeData.Type] = typeData;

        // 리스트에 있는 고정 층 노드 타입 딕셔너리로 캐싱
        foreach (var fixedFloor in _fixedFloorNodes)
        {
            foreach (var floor in fixedFloor.Floors)
            {
                if (_fixedFloorNodeDict.ContainsKey(floor))
                    continue;

                // 고정 층 노드 타입 딕셔너리에 추가
                _fixedFloorNodeDict[floor] = fixedFloor.NodeType;
            }
        }
    }
    
    /// <summary>
    /// 노드 타입을 가져오는 함수(맵 생성 시 사용)
    /// </summary>
    /// <param name="floor">현재 층</param>
    /// <returns>확률 및 고정 층 노드가 적용된 노드 타입 반환</returns>
    public NodeType GetNodeType(int floor)
    {
        if(_fixedFloorNodeDict.TryGetValue(floor, out var fixedNodeType))
        {
            // 고정 층 노드 타입이 존재한다면 해당 타입 반환
            return fixedNodeType;
        }
        
        return CalculateNodeProbabilities();
    }
    
    /// <summary>
    /// 노트 타입 확률 계산 함수
    /// </summary>
    /// <param name="excludedType">제외할 타입</param>
    /// <returns>확률로 뽑힌 노드 타입 반환</returns>
    private NodeType CalculateNodeProbabilities(NodeType excludedType = NodeType.None)
    {
        int randomValue = Random.Range(0, 100);
        int totalProbability = 0;
        
        foreach (var nodeProbability in _nodeProbabilities)
        {
            // 제외할 노드 타입과 일치한다면 패스
            if(excludedType != NodeType.None && nodeProbability.NodeType == excludedType)
                continue;
            
            totalProbability += nodeProbability.Probability;
            if (randomValue < totalProbability)
                return nodeProbability.NodeType;
        }

        return NodeType.NormalBattle;
    }
}
