using System;
using UnityEngine;

public class MapCreator
{
    public Node StartNode { get; private set; }
    private NodeTable _nodeTable;
    
    /// <summary>
    /// 맵 생성 함수
    /// </summary>
    /// <param name="mapRoomCount">생성할 층 당 방 수</param>
    /// <param name="mapLayerCount">생성할 층 수</param>
    /// <param name="nodeTable">확률을 적용할 노드 테이블</param>
    /// <param name="seed">랜덤 값 시드</param>
    /// <returns>생성된 맵 반환</returns>
    public Map CreateMap(int mapRoomCount, int mapLayerCount, NodeTable nodeTable, int seed = 0)
    {
        SetSeed(seed); // 시드 설정
        
        Map map = new Map(mapRoomCount, mapLayerCount);
        
        _nodeTable = nodeTable;

        CreateStartNode(map, mapRoomCount);                     // 시작 노드 설정
        CreateFirstFloorNodes(map);                             // 1층 노드 설정(현재 노말 배틀 3개 고정)
        CreateMiddleNodes(map, mapRoomCount, mapLayerCount);    // 2층 부터 보스 이전 층까지 노드 생성
        CreateBossNode(map, mapRoomCount, mapLayerCount);       // 보스 노드 생성
        AddExtraConnectNode(map, mapRoomCount, mapLayerCount);  // 특수 노드(스킵, 같은층 반복) 추가

        return map;
    }

    #region 맵 생성 관련 함수
    
    // 랜덤 시드 설정 함수
    private void SetSeed(int seed)
    {
        // 시드 값이 없다면 임의로 생성
        if (seed == 0)
            seed = UnityEngine.Random.Range(0, int.MaxValue);

        // 시드값 적용(받아온 값 또는 랜덤값)
        UnityEngine.Random.InitState(seed);
    }

    // 시작 노드 생성 함수
    private void CreateStartNode(Map map, int mapRoomCount)
    {
        // 0층 노드 설정 (시작 지점)
        // 시작 노드는 맵 넓이의 중간 값으로 설정
        int startNodeX = mapRoomCount / 2;
        int startNodeY = 0;
        StartNode = map.MapInfo[startNodeY][startNodeX] = new Node(startNodeX, startNodeY, NodeType.Start);
    }

    // 1층 노드 생성 함수
    private void CreateFirstFloorNodes(Map map)
    {
        int firstFloorNodeCount = 3;

        // 1층 노드 설정(1, 2, 3번 인덱스에 일반 전투 노드)
        for (int i = 1; i <= firstFloorNodeCount; i++)
        {
            map.MapInfo[1][i] = new Node(i, 1, NodeType.NormalBattle);
            // 1층에 생성한 노드의 연결된 노드 리스트에 시작 지점 추가
            StartNode.ConnectedNodes.Add(map.MapInfo[1][i]);
        }
    }
    
    // 2층부터 보스 이전 층까지의 노드 생성 함수
    private void CreateMiddleNodes(Map map, int mapRoomCount, int mapLayerCount)
    {
        int startRoomPos = mapRoomCount / 2;

        // 1층에 고정적으로 생성된 각 노드에서 위 층으로 이동하며 노드 생성
        for (int roomPos = startRoomPos - 1; roomPos <= startRoomPos + 2; roomPos++)
        {
            int currentRoomPos = roomPos;
            Node previousNode = map.MapInfo[1][currentRoomPos];

            // 선택지 추가를 위해 한번 더 반복
            if (roomPos > startRoomPos + 1)
            {
                currentRoomPos = roomPos - 1;
                previousNode = map.MapInfo[1][currentRoomPos];
            }
            
            CreatePathFromNode(map, mapRoomCount, mapLayerCount, currentRoomPos, previousNode);
        }
    }
    
    // 첫 노드(1층 노드)에서 다음 층으로 이동하며 노드 생성
    private void CreatePathFromNode(Map map, int mapRoomCount, int mapLayerCount, int startRoomPos, Node startNode)
    {
        int currentRoomPos = startRoomPos;
        Node previousNode = startNode;
        
        // 시작 지점을 제외 하기 위해 1층부터 시작 (보스 노드는 따로 생성하기 때문에 보스층 제외)
        for(int currentLayer = 1; currentLayer < mapLayerCount - 2; currentLayer++)
        {
            if (!map.MapInfo.ContainsKey(currentLayer + 1)) continue;
            
            // 다음층에 생성할 노드의 x값을 현재 위치에서 -1, 0, 1 중 랜덤값 뽑아 정한 후
            // 맵 범위를 벗어나지 않도록 제한하여 다시 조정
            int move = UnityEngine.Random.Range(-1, 2);
            int nextRoomPos = Mathf.Clamp(currentRoomPos + move, 0, mapRoomCount - 1);
            
            // 이동할 위치에 노드가 없다면 새로 생성 후 가져오기 / 노드가 있다면 기존 노드 가져오기
            Node targetNode = CreateOrGetNode(map, nextRoomPos, currentLayer + 1);
            
            // 이전 노드의 연결된 노드 리스트에 추가
            if(previousNode != null && !previousNode.ConnectedNodes.Contains(targetNode))
                previousNode.ConnectedNodes.Add(targetNode);
            
            // 현재 x값의 위치를 다음 위치로 변경
            currentRoomPos = nextRoomPos;
            previousNode = targetNode;
        }
    }

    // 현재 위치에 노드가 있는지 확인 후 없다면 새로 생성하는 함수
    private Node CreateOrGetNode(Map map, int roomPos, int layer)
    {
        // 현재 위치에 노드가 있는지 확인 후 없다면 새로 생성
        if (map.MapInfo[layer][roomPos] == null)
        {
            Node newNode = new Node(roomPos, layer, _nodeTable.GetNodeType(layer));
            map.MapInfo[layer][roomPos] = newNode;
            return newNode;
        }
        
        // 다음 위치에 노드가 있다면 존재하는 노드 그대로 사용
        Node existingNode = map.MapInfo[layer][roomPos];

        return existingNode;
    }
    
    // 마지막 층에 보스 노드 생성 함수
    private void CreateBossNode(Map map, int mapRoomCount, int mapLayerCount)
    {
        if (!map.MapInfo.ContainsKey(mapLayerCount - 1)) return;
        
        // 마지막 층의 가운데에 보스 노드 추가
        map.MapInfo[mapLayerCount - 1][mapRoomCount / 2] = new Node(mapRoomCount / 2, mapLayerCount - 1, NodeType.BossBattle);

        // 보스 층 이전 층의 노드와 보스 노드를 연결
        foreach (var node in map.MapInfo[mapLayerCount - 2])
            node?.ConnectedNodes.Add(map.MapInfo[mapLayerCount - 1][mapRoomCount / 2]);
    }
    #endregion

    #region 특수 노드 추가
    // 랜덤하게 특수 노드 추가
    private void AddExtraConnectNode(Map map, int mapRoomCount, int mapLayerCount)
    {
        int extraCount = 0;

        int extraNodeProbability = _nodeTable.ExtraNodeProbability;

        // 보스방을 제외하고 랜덤으로 특수 노드 추가
        for (int layer = 1; layer < mapLayerCount - 2; layer++)
        {
            bool isAddExtraNode = false;

            // 해당 층이 존재 한다면
            if (!map.MapInfo.ContainsKey(layer)) continue;

            // 노드 최대 수만큼 반복
            for (int roomPos = 0; roomPos < mapRoomCount; roomPos++)
            {
                Node node = map.MapInfo[layer][roomPos];

                // 현재 위치에 노드가 존재 && 정해둔 확률로 특수 노드 추가
                if (node == null || UnityEngine.Random.Range(0, 100) >= extraNodeProbability) continue;

                bool isSkip = UnityEngine.Random.Range(0, 100) >= 50;
                
                bool isSuccess = TryConnectExtraNode(map, mapRoomCount, node, layer, isSkip);
                
                if (!isSuccess) continue;
                
                extraCount++;
                isAddExtraNode = true;
                if (extraCount >= 3) return;
                break;
            }

            if(isAddExtraNode) layer++;
        }
    }
    
    // 특수 노드 추가
    private bool TryConnectExtraNode(Map map, int mapRoomCount, Node node, int currentLayer, bool isSkip)
    {
        // while문 실행 횟수 제한
        int attempts = 0;
        int maxAttempts = 5;

        do
        {
            // 스킵이라면 -2, -1, 0, 1, 2 중에서 랜덤값 뽑기 : 스킵이 아니라면 -1, 1 중 랜덤값 뽑기
            int move = isSkip ? UnityEngine.Random.Range(-2, 3) : UnityEngine.Random.Range(0, 100) < 50 ? -1 : 1;
            int nextRoomPos = Math.Clamp(node.RoomPosition + move, 0, mapRoomCount - 1);

            int layer = currentLayer;
            
            // 건너뛸 위치
            if(isSkip)
                layer = currentLayer + 2;
            int targetRoomPos = nextRoomPos;

            if (isSkip)
            {
                // 목표 층이 존재하는지 확인 && 맵 넓이를 벗어나지 않는지 확인
                if (!map.MapInfo.ContainsKey(layer) || targetRoomPos >= mapRoomCount)
                {
                    attempts++;
                    continue;
                }
            }
            else
            {
                // 맵 넓이를 벗어나거나 목표 x값이 현재 노드의 x값과 같다면 패스
                if (targetRoomPos >= mapRoomCount || targetRoomPos == node.RoomPosition)
                {
                    attempts++;
                    continue;
                }
            }
            Node targetNode = map.MapInfo[layer][targetRoomPos];
            
            // 타겟노드가 존재하지 않거나 이미 연결된 노드에 포함되어 있다면 패스
            if (targetNode == null || node.ConnectedNodes.Contains(targetNode))
            {
                attempts++;
                continue;
            }
        
            // 타겟 노드가 존재하고, 연결할 노드와 연결되어 있지 않다면 연결된 노드에 추가
            node.ConnectedNodes.Add(targetNode);
            
            // 같은층 반복 노드라면 타겟노드에도 연결된 노드에 추가
            if(!isSkip) targetNode.ConnectedNodes.Add(node);
            
            return true;
        } while (attempts < maxAttempts);

        return false;
    }
    #endregion
}
