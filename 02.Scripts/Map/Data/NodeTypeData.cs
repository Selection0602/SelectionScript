using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Node Type", menuName = "Map/Node Type")]
public class NodeTypeData : ScriptableObject
{
    [FormerlySerializedAs("type")] public NodeType Type;
    [FormerlySerializedAs("typeName")] public string TypeName;
    
    [FormerlySerializedAs("nodeColor")] public Color NodeColor;
    [FormerlySerializedAs("nodeIcon")] public Sprite NodeIcon;
    [FormerlySerializedAs("nodeScale")] public Vector3 NodeScale = Vector3.one; // 노드 크기(시작, 보스 제외 모두 1,1,1)
}