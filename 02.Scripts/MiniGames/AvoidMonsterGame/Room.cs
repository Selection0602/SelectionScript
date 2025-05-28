using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Room : MonoBehaviour
{
    //방의 입구 Dictionary
    public Dictionary<Direction, Enterance> EnteranceDict = new Dictionary<Direction, Enterance>();
    //Dctionary에 넣기 위한 List
    public List<GameObject> EnteranceObjects;

    public float RoomWidth;     //방 한변의 길이
    public RoomType RoomType;   //방 타입
    
    [SerializeField] private Tilemap _tilemap; //타일맵
    [SerializeField] private LayerMask _obstacleLayer; // 장애물 레이어
    private List<Vector3Int> _spawnableTileList = new List<Vector3Int>(); // 아이템 스폰 가능 위치 리스트

    //Dictionary에 입구들 넣어주기
    private void Awake()
    {
        foreach (GameObject go in EnteranceObjects)
        {
            EnteranceDict.Add(go.GetComponent<Enterance>().Direction, go.GetComponent<Enterance>());
            EnteranceDict[go.GetComponent<Enterance>().Direction].Block.SetActive(false);
        }
    }

    public Vector3 GetRandomSpawnPosition()
    {
        // 아이템 스폰 가능한 타일 리스트 초기화
        InitSpawnableTiles();
        
        if (_spawnableTileList.Count == 0)
            return transform.position;
        
        // 랜덤한 타일 가져오기
        int randomIndex = Random.Range(0, _spawnableTileList.Count);
        Vector3Int tilePos = _spawnableTileList[randomIndex];
        
        // 타일 위치 월드좌표로 변환
        Vector3 worldPos = _tilemap.GetCellCenterWorld(tilePos);
        // 장애물 존재하는지 체크
        bool hasObstacle = Physics2D.OverlapCircle(worldPos, _tilemap.cellSize.x, _obstacleLayer);
        
        if (hasObstacle)
        {
            // 장애물이 있으면 현재 타일을 리스트에서 제거하고 다시 시도
            _spawnableTileList.RemoveAt(randomIndex);
            return GetRandomSpawnPosition();
        }
        
        return worldPos;
    }

    private void InitSpawnableTiles()
    {
        _spawnableTileList.Clear();

        if (!_tilemap) return;
        
        // 타일맵의 셀(그리드?) 가져오기
        BoundsInt bounds = _tilemap.cellBounds;
        
        // 타일맵의 모든 셀 체크
        for (int x = bounds.min.x; x < bounds.max.x; x++)
        {
            for (int y = bounds.min.y; y < bounds.max.y; y++)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0);
                
                // tilePos에 타일이 있는지 체크
                if (_tilemap.HasTile(tilePos))
                {
                    // 타일이 있다면 장애물 체크 후 리스트에 추가
                    Vector3 worldPos = _tilemap.GetCellCenterWorld(tilePos);
                    bool hasObstacle = Physics2D.OverlapCircle(worldPos, _tilemap.cellSize.x * 0.4f, _obstacleLayer);
                        
                    if (!hasObstacle)
                        _spawnableTileList.Add(tilePos);
                }
            }
        }
    }
    
    //방 입구 봉쇄
    public void CloseEnterance(Direction dir)
    {
        EnteranceDict[dir].gameObject.SetActive(false);
        EnteranceDict[dir].Block.SetActive(true);
    }

    //방 입구 개방
    public void OpenEnterance(Direction dir)
    {
        EnteranceDict[dir].gameObject.SetActive(true);
        EnteranceDict[dir].Block.SetActive(false);
    }
    
    public void LockEntrance(Direction dir, int keyItemId, bool isExit = false)
    {
        if (EnteranceDict.ContainsKey(dir) && EnteranceDict[dir].gameObject.activeSelf)
        {
            EnteranceDict[dir].IsLocked = true;
            EnteranceDict[dir].RequiredKeyItemId = keyItemId;
            if (isExit)
                EnteranceDict[dir].IsExit = true;

            if(dir is Direction.Up)
                EnteranceDict[dir].LockBlock?.SetActive(true);
        }
    }
    
    public void UnlockEntrance(Direction dir)
    {
        if (EnteranceDict.ContainsKey(dir))
        {
            EnteranceDict[dir].IsLocked = false;
            EnteranceDict[dir].RequiredKeyItemId = -1;

            if (dir is Direction.Up)
                EnteranceDict[dir]?.LockBlock?.SetActive(false);
        }
    }

    public void UnlockAllEntrances()
    {
        Vector2Int? roomPosition = null;
        
        foreach (var kvp in MapGenerator.RoomDict)
        {
            if (kvp.Value == this)
            {
                roomPosition = kvp.Key;
                break;
            }
        }

        if (roomPosition == null) return;

        foreach (Direction dir in EnteranceDict.Keys)
        {
            if (EnteranceDict[dir].gameObject.activeSelf)
            {
                Vector2Int connectedRoomPos = roomPosition.Value + MapGenerator.MapVector2Int(dir);
                if (MapGenerator.RoomDict.ContainsKey(connectedRoomPos))
                {
                    Direction oppositeDir = MapGenerator.OppositeDirection(dir);
                    Room connectedRoom = MapGenerator.RoomDict[connectedRoomPos];

                    if ((connectedRoom.RoomType is RoomType.Library or RoomType.Piano &&
                         connectedRoom.EnteranceDict[oppositeDir].IsLocked))
                        continue;
                    
                    if (connectedRoom.EnteranceDict.ContainsKey(oppositeDir) || connectedRoom.RoomType == RoomType.Corridor)
                        connectedRoom.UnlockEntrance(oppositeDir);
                    
                    UnlockEntrance(dir);
                }
            }
        }
    }
    
    public void LockAllEntrances(int keyItemId)
    {
        foreach (Direction dir in EnteranceDict.Keys)
        {
            if (EnteranceDict[dir].gameObject.activeSelf)
            {
                LockEntrance(dir, keyItemId);
            }
        }
    }
}
