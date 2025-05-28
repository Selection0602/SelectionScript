using System.Collections.Generic;

public class Map
{
    public int RoomCount { get; private set; } // 층 당 방의 개수
    public int LayerCount { get; private set; } // 층의 개수
    
    // 맵 정보 저장(키 : 층수, 값 : 해당 층의 노드들)
    public Dictionary<int, Node[]> MapInfo { get; private set; }

    public Map(int roomCount, int layerCount)
    {
        RoomCount = roomCount;
        LayerCount = layerCount;
        MapInfo = new Dictionary<int, Node[]>();
        for (int layer = 0; layer < layerCount; layer++)
        {
            MapInfo[layer] = new Node[roomCount];
        }
    }
}