using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializableMapData
{
    public SerializableMap currentMap;
    public SerializableNode playerNode;
    public List<SerializableNode> playerPath = new List<SerializableNode>();
    public float contentPositionX;
    public float contentPositionY;
}

[System.Serializable]
public class SerializableMap
{
    public int width;
    public int height;
    public List<SerializableMapLayer> layers = new List<SerializableMapLayer>();
}

[System.Serializable]
public class SerializableMapLayer
{
    public int layerIndex;
    public List<SerializableNode> nodes = new List<SerializableNode>();
}

[System.Serializable]
public class SerializableNode
{
    public int x;
    public int y;
    public int nodeType; // NodeType을 int로 저장
    public List<Vector2Int> connectedCoords = new List<Vector2Int>(); // 연결된 노드 좌표들
}

// 어댑터 클래스 - 변환 담당
public static class MapDataSerializer
{
    /// <summary>
    /// MapData를 JSON 문자열로 변환
    /// </summary>
    public static string ToJson(MapData mapData)
    {
        if (mapData == null) return null;
        
        SerializableMapData serializableData = ConvertToSerializable(mapData);
        return JsonUtility.ToJson(serializableData, true);
    }
    
    public static MapData FromJson(string json)
    {
        if (string.IsNullOrEmpty(json)) return null;
        
        try
        {
            SerializableMapData serializableData = JsonUtility.FromJson<SerializableMapData>(json);
            return ConvertFromSerializable(serializableData);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"JSON 파싱 오류: {e.Message}");
            return null;
        }
    }
    
    // MapData → SerializableMapData 변환
    private static SerializableMapData ConvertToSerializable(MapData mapData)
    {
        SerializableMapData result = new SerializableMapData();
        
        // Map 변환
        if (mapData.CurrentMap != null)
        {
            result.currentMap = ConvertMapToSerializable(mapData.CurrentMap);
        }
        
        // Player Node 변환
        if (mapData.PlayerNode != null)
        {
            result.playerNode = ConvertNodeToSerializable(mapData.PlayerNode);
        }
        
        // Player Path 변환
        if (mapData.PlayerPath != null)
        {
            foreach (var node in mapData.PlayerPath)
            {
                if (node != null)
                {
                    result.playerPath.Add(ConvertNodeToSerializable(node));
                }
            }
        }
        
        // Content Position 변환
        result.contentPositionX = mapData.ContentPosition.x;
        result.contentPositionY = mapData.ContentPosition.y;
        
        return result;
    }
    
    // SerializableMapData → MapData 변환
    private static MapData ConvertFromSerializable(SerializableMapData serializableData)
    {
        // Map 복원
        Map map = null;
        if (serializableData.currentMap != null)
        {
            map = ConvertMapFromSerializable(serializableData.currentMap);
        }
        
        // 모든 노드를 좌표 기준으로 딕셔너리에 저장 (연결 복원용)
        Dictionary<Vector2Int, Node> nodeDict = CreateNodeDictionary(map);
        
        // Player Node 복원
        Node playerNode = null;
        if (serializableData.playerNode != null)
        {
            Vector2Int playerCoord = new Vector2Int(serializableData.playerNode.x, serializableData.playerNode.y);
            nodeDict.TryGetValue(playerCoord, out playerNode);
        }
        
        // Player Path 복원
        List<Node> playerPath = new List<Node>();
        foreach (var serNode in serializableData.playerPath)
        {
            Vector2Int coord = new Vector2Int(serNode.x, serNode.y);
            if (nodeDict.TryGetValue(coord, out Node node))
            {
                playerPath.Add(node);
            }
        }
        
        // Content Position 복원
        Vector2 contentPosition = new Vector2(serializableData.contentPositionX, serializableData.contentPositionY);
        
        return new MapData(map, playerNode, playerPath, contentPosition);
    }
    
    // Map → SerializableMap 변환
    private static SerializableMap ConvertMapToSerializable(Map map)
    {
        SerializableMap result = new SerializableMap
        {
            width = map.RoomCount,
            height = map.LayerCount
        };
        
        foreach (var kvp in map.MapInfo)
        {
            SerializableMapLayer layer = new SerializableMapLayer
            {
                layerIndex = kvp.Key
            };
            
            foreach (var node in kvp.Value)
            {
                if (node != null)
                {
                    layer.nodes.Add(ConvertNodeToSerializable(node));
                }
                else
                {
                    layer.nodes.Add(null); // null도 위치 유지
                }
            }
            
            result.layers.Add(layer);
        }
        
        return result;
    }
    
    // SerializableMap → Map 변환
    private static Map ConvertMapFromSerializable(SerializableMap serializableMap)
    {
        Map result = new Map(serializableMap.width, serializableMap.height);
        
        // 기존 초기화된 배열들을 클리어
        result.MapInfo.Clear();
        
        foreach (var layer in serializableMap.layers)
        {
            Node[] nodes = new Node[layer.nodes.Count];
            
            for (int i = 0; i < layer.nodes.Count; i++)
            {
                if (layer.nodes[i] != null)
                {
                    nodes[i] = ConvertNodeFromSerializable(layer.nodes[i]);
                }
            }
            
            result.MapInfo[layer.layerIndex] = nodes;
        }
        
        // 노드 연결 관계 복원
        RestoreNodeConnections(result, serializableMap);
        
        return result;
    }
    
    // Node → SerializableNode 변환
    private static SerializableNode ConvertNodeToSerializable(Node node)
    {
        SerializableNode result = new SerializableNode
        {
            x = node.RoomPosition,
            y = node.Layer,
            nodeType = (int)node.Type
        };
        
        // 연결된 노드들의 좌표 저장
        foreach (var connectedNode in node.ConnectedNodes)
        {
            result.connectedCoords.Add(new Vector2Int(connectedNode.RoomPosition, connectedNode.Layer));
        }
        
        return result;
    }
    
    // SerializableNode → Node 변환
    private static Node ConvertNodeFromSerializable(SerializableNode serializableNode)
    {
        return new Node(serializableNode.x, serializableNode.y, (NodeType)serializableNode.nodeType);
    }
    
    // 모든 노드를 좌표 기준 딕셔너리로 생성
    private static Dictionary<Vector2Int, Node> CreateNodeDictionary(Map map)
    {
        Dictionary<Vector2Int, Node> nodeDict = new Dictionary<Vector2Int, Node>();
        
        if (map?.MapInfo != null)
        {
            foreach (var layer in map.MapInfo)
            {
                foreach (var node in layer.Value)
                {
                    if (node != null)
                    {
                        Vector2Int coord = new Vector2Int(node.RoomPosition, node.Layer);
                        nodeDict[coord] = node;
                    }
                }
            }
        }
        
        return nodeDict;
    }
    
    // 노드 연결 관계 복원
    private static void RestoreNodeConnections(Map map, SerializableMap serializableMap)
    {
        Dictionary<Vector2Int, Node> nodeDict = CreateNodeDictionary(map);
        
        foreach (var layer in serializableMap.layers)
        {
            foreach (var serNode in layer.nodes)
            {
                if (serNode != null)
                {
                    Vector2Int nodeCoord = new Vector2Int(serNode.x, serNode.y);
                    if (nodeDict.TryGetValue(nodeCoord, out Node actualNode))
                    {
                        // 연결된 노드들 복원
                        foreach (var connectedCoord in serNode.connectedCoords)
                        {
                            if (nodeDict.TryGetValue(connectedCoord, out Node connectedNode))
                            {
                                if (!actualNode.ConnectedNodes.Contains(connectedNode))
                                {
                                    actualNode.ConnectedNodes.Add(connectedNode);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
