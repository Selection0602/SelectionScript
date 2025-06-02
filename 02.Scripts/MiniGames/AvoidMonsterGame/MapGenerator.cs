using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MapGenerator : MonoBehaviour
{
    #region Variable Declaration
    [Header("Room Settings")]
    public List<Room> RoomPrefabList = new List<Room>();        //방 프리팹들
    public Room Porch;              //현관
    public Room Library;            //도서돤
    public Room Piano;              //피아노방
    public ItemForEscape Item;      //아이템

    [Header("Map Settings")]
    [SerializeField] private int _mapWidth;             //맵의 한변 길이
    [SerializeField] private int _interval;             //방 사이의 간격
    private Direction _closedDir;                       //닫힐 예정인 문의 방향
    private Vector2Int _closedRoomPos;                  //닫힐 예정인 방 위치
    [SerializeField] public static int ItemCount = 4;   //아이템 생성 횟수

    //실제로 생성된 맵 안의 방 Dictionary
    public static Dictionary<Vector2Int, Room> RoomDict = new Dictionary<Vector2Int, Room>();
    //아이템이 있는 방
    private Dictionary<Vector2Int, Room> _itemRoomDict = new Dictionary<Vector2Int, Room>();
    //방의 타입
    private Dictionary<RoomType, int> _roomTypeKeyDict = new Dictionary<RoomType, int>()
    {
        { RoomType.Library, 3 },
        { RoomType.Piano, 4 },
        { RoomType.Porch, 2 }
    };
    //아이템 테이블
    [SerializeField] private AvoidItemTable _itemTable;
    #endregion

    #region Unity Life Cycle
    /// <summary>
    /// -Start에서 맵을 생성하는 과정-
    /// 1. 맵을 _mapWidth * _mapWidth의 크기로 생성
    /// 2. 방을 생성하면서 방들을 몇번째 방인지(Vector2Int)를 Key로 삼아서 Dictionary에 저장
    /// 3. 맵 안의 랜덤한 방을 골라서 랜덤한 입구와 이와 연결되는 방의 입구를 제거
    /// 4. DFS로 입구 제거 후의 순회 가능 여부를 판단, 가능하다고 판단되면 입구 제거 반복
    /// 5. 불가능하다고 판단 될 경우, 최근에 닫아준 입구를 열어줌
    /// 6. 랜덤한 방에, 정해진 범위 안에 _itemCount 아이템을 생성 
    /// 7. 방의 타입에 따라, 문을 잠글지 말지 결정한다.
    /// 8. 현관 밑의 입구는 열어주어 탈출구로 만들어준다.
    /// 9. 완성
    /// </summary>
    private void Start()
    {
        Manager.Instance.CreditManager.UnlockMiniGameAndSave(2);

        //랜덤 생성될 곳 정하기 (도서관, 피아노 방)
        int widthRand = 0, heightRand = 0;

        widthRand = UnityEngine.Random.Range(1, _mapWidth - 1);
        heightRand = UnityEngine.Random.Range(1, _mapWidth - 1);

        Vector2Int randIntForLibrary = new Vector2Int(widthRand, heightRand);

        while (widthRand == randIntForLibrary.x && heightRand == randIntForLibrary.y)
        {
            widthRand = UnityEngine.Random.Range(1, _mapWidth - 1);
            heightRand = UnityEngine.Random.Range(1, _mapWidth - 1);
        }
        Vector2Int randIntForPiano = new Vector2Int(widthRand, heightRand);

        //테두리에 있는 입구를 제외하고 기초적인 맵 생성
        for (int index = 0; index < _mapWidth * _mapWidth; index++)
        {
            int i = index / _mapWidth;
            int j = index % _mapWidth;

            Room RoomPrefab = RoomPrefabList[UnityEngine.Random.Range(0, RoomPrefabList.Count)];

            //현관은 초반에 생성, 위에서 따로 정한 도서관, 피아노 방 생성
            if (i == 0 && j == 0) RoomPrefab = Porch;
            else if (i == randIntForLibrary.x && j == randIntForLibrary.y) RoomPrefab = Library;
            else if (i == randIntForPiano.x && j == randIntForPiano.y) RoomPrefab = Piano;

            var roomGO = Instantiate(RoomPrefab, new Vector2((RoomPrefab.RoomWidth + _interval) * j - 0.5f,
                (RoomPrefab.RoomWidth + _interval) * i - 0.5f), Quaternion.identity, this.transform);

            CheckEnterance(roomGO, i, j);
            Vector2Int key = new Vector2Int(i, j);
            RoomDict.Add(key, roomGO);
        }

        //순회 가능 여부 조회(불가능 할때까지 조회, DFS 이용)
        while (CheckDFS(new Vector2Int(0, 0)))
        {
            CloseRandomEnterance();
            if (!CheckDFS(new Vector2Int(0, 0)))
            {
                RoomDict[_closedRoomPos].OpenEnterance(_closedDir);
                OpenNextRoom(_closedRoomPos, _closedDir);
                break;
            }
        }

        _itemTable.InitializeRuntimeItems();                    // 아이템 테이블 초기화
        int itemCount = _itemTable.GetSpawnItemCount();

        for (int i = 0; i < itemCount; i++) SpawnItem();        //아이템 스폰

        LockRoomsByType();                                      //타입에 따라 문을 닫음

        Room exitRoom = RoomDict[new Vector2Int(0, 0)];         //처음 방을 탈출방으로 만들기

        exitRoom.OpenEnterance(Direction.Down);                 //현관 아래 입구 열기
        exitRoom.LockEntrance(Direction.Down, _roomTypeKeyDict[exitRoom.RoomType], true);
        
        Manager.Instance.SoundManager.PlayBGM(BGMType.BGM_MiniGame);        //BGM 재생
    }
    #endregion

    //입구 관리 - 테두리에 있는 입구 제거
    void CheckEnterance(Room room, int height, int width)
    {
        if (height == _mapWidth - 1) room.CloseEnterance(Direction.Up);             //위쪽 입구 봉쇄
        else if (height == 0) room.CloseEnterance(Direction.Down);                  //밑쪽 입구 봉쇄
        
        if (width == 0) room.CloseEnterance(Direction.Left);                        //왼쪽 입구 봉쇄
        else if (width == _mapWidth - 1) room.CloseEnterance(Direction.Right);      //오른쪽 입구 봉쇄
    }

    //랜덤한 방의 활성화된 랜덤한 입구 닫기 
    void CloseRandomEnterance()
    {
        while (true)
        {
            //랜덤한 하나의 입구를 결정한다. (테두리를 제외한 부분에서)
            int widthRand = UnityEngine.Random.Range(1, _mapWidth - 1);
            int heightRand = UnityEngine.Random.Range(1, _mapWidth - 1);

            Vector2Int randInt = new Vector2Int(widthRand, heightRand);
            _closedRoomPos = randInt;

            Room randomRoom = RoomDict[randInt];

            Direction enteranceRand = (Direction)UnityEngine.Random.Range(0, randomRoom.EnteranceDict.Count);
            _closedDir = enteranceRand;

            //만약 해당 입구가 열려있다면, 이에 상응하는 입구도 같이 닫아준다.
            if (randomRoom.EnteranceDict[enteranceRand].gameObject.activeSelf)
            {
                randomRoom.CloseEnterance(enteranceRand);
                RoomDict[randInt + MapVector2Int(enteranceRand)].CloseEnterance(OppositeDirection(enteranceRand));
                break;
            }
        }
    }

    //깊이 우선 탐색을 통한 전체 순회 가능 여부
    bool CheckDFS(Vector2Int checkInt)
    {
        bool[,] visited = new bool[_mapWidth, _mapWidth];
        DFS(checkInt.x, checkInt.y, visited, Direction.Up);

        //LINQ로 2차원 배열 전체가 true인지 확인
        return visited.Cast<bool>().All(v => v);
    }

    //깊이 우선 탐색
    void DFS(int x, int y, bool[,] visited, Direction dir)
    {
        //길이 제한
        if ((x == _mapWidth) || (y == _mapWidth)) return;

        //중복 방문 제한
        if (visited[x, y]) return;

        //if : 지나온 길이 존재하지 않는 경우 => false
        //else if : 만약 피아노 방이거나 도사관일 경우 => true후 return  (이는 두개의 방으로 인한 닫힌 방의 존재를 없애기 위함)
        //else : 지나온 길이 존재하는 경우 => true
        switch (dir)
        {
            case Direction.Up:
                if (x > 0 && !RoomDict[new Vector2Int(x, y)].EnteranceDict[Direction.Down].gameObject.activeSelf) return;
                else if (RoomDict[new Vector2Int(x, y)].RoomType == RoomType.Piano || RoomDict[new Vector2Int(x, y)].RoomType == RoomType.Library)
                {
                    visited[x, y] = true;
                    return;
                }
                else visited[x, y] = true;
                break;
            case Direction.Right:
                if (y > 0 && !RoomDict[new Vector2Int(x, y)].EnteranceDict[Direction.Left].gameObject.activeSelf) return;
                else if (RoomDict[new Vector2Int(x, y)].RoomType == RoomType.Piano || RoomDict[new Vector2Int(x, y)].RoomType == RoomType.Library)
                {
                    visited[x, y] = true;
                    return;
                }
                else visited[x, y] = true;
                break;
            case Direction.Down:
                if (x > 0 && !RoomDict[new Vector2Int(x, y)].EnteranceDict[Direction.Up].gameObject.activeSelf) return;
                else if (RoomDict[new Vector2Int(x, y)].RoomType == RoomType.Piano || RoomDict[new Vector2Int(x, y)].RoomType == RoomType.Library)
                {
                    visited[x, y] = true;
                    return;
                }
                else visited[x, y] = true;
                break;
            case Direction.Left:
                if (y > 0 && !RoomDict[new Vector2Int(x, y)].EnteranceDict[Direction.Right].gameObject.activeSelf) return;
                else if (RoomDict[new Vector2Int(x, y)].RoomType == RoomType.Piano || RoomDict[new Vector2Int(x, y)].RoomType == RoomType.Library)
                {
                    visited[x, y] = true;
                    return;
                }
                else visited[x, y] = true;
                break;
        }

        //재귀
        DFS(x + 1, y, visited, Direction.Up);
        DFS(x, y + 1, visited, Direction.Right);

        if (x != 0) DFS(x - 1, y, visited, Direction.Down);
        if (y != 0) DFS(x, y - 1, visited, Direction.Left);
    }

    //다음 문 열기
    void OpenNextRoom(Vector2Int pos, Direction dir)
    {
        RoomDict[_closedRoomPos + MapVector2Int(dir)].OpenEnterance(OppositeDirection(dir));
    }

    //랜덤한 곳에 아이템 스폰하기
    void SpawnItem()
    {
        Vector2Int randomPos;
        do
        {
            int randX = UnityEngine.Random.Range(0, _mapWidth);
            int randY = UnityEngine.Random.Range(0, _mapWidth);
            randomPos = new Vector2Int(randX, randY);
        }
        while (_itemRoomDict.ContainsKey(randomPos) || RoomDict[randomPos].RoomType != RoomType.Corridor);

        Room randomRoom = RoomDict[randomPos];
        _itemRoomDict.Add(randomPos, randomRoom);

        // 아이템 스폰 위치 가져오기
        Vector3 spawnPos = randomRoom.GetRandomSpawnPosition();
        
        GameObject itemObject = Instantiate(_itemTable.ItemPrefab,
            spawnPos, Quaternion.identity, randomRoom.transform);

        AvoidItemData randomItemData = _itemTable.GetRandomItem();

        // 아이템 데이터 설정
        ItemForEscape item = itemObject.GetComponent<ItemForEscape>();
        if (item)
            item.SetItemData(randomItemData);
    }

    //방 타입에 따라서 문을 닫아주기
    private void LockRoomsByType()
    {
        foreach (var (pos, room) in RoomDict)
        {
            switch (room.RoomType)
            {
                case RoomType.Library or RoomType.Piano:
                {
                    foreach (Direction dir in System.Enum.GetValues(typeof(Direction)))
                    {
                        if (room.EnteranceDict.ContainsKey(dir) && room.EnteranceDict[dir].gameObject.activeSelf)
                        {
                            Vector2Int connectedPos = pos + MapVector2Int(dir);
                            if (RoomDict.ContainsKey(connectedPos))
                            {
                                Direction oppositeDir = OppositeDirection(dir);
                                RoomDict[connectedPos].LockEntrance(oppositeDir, _roomTypeKeyDict[room.RoomType]);
                            }
                        }
                    }

                    break;
                }
                case RoomType.Porch:
                {
                    if (room.EnteranceDict.ContainsKey(Direction.Down) && room.EnteranceDict[Direction.Down].gameObject.activeSelf)
                        room.LockEntrance(Direction.Down, _roomTypeKeyDict[room.RoomType]);
                    break;
                }
            }
        }
    }

    //전역으로 선언된 Dictionary 처리
    private void OnDestroy()
    {
        RoomDict = new Dictionary<Vector2Int, Room>();
    }

    #region static Methods

    //반대 방향 리턴
    public static Direction OppositeDirection(Direction dir)
    {
        switch (dir)
        {
            case Direction.Up: return Direction.Down;
            case Direction.Down: return Direction.Up;
            case Direction.Left: return Direction.Right;
            case Direction.Right: return Direction.Left;
            default: return dir;
        }
    }

    //방향에 상응하는 Vector2Int 리턴(방 좌표 기준)
    public static Vector2Int MapVector2Int(Direction dir)
    {
        switch (dir)
        {
            case Direction.Up: return new Vector2Int(1, 0);
            case Direction.Down: return new Vector2Int(-1, 0);
            case Direction.Left: return new Vector2Int(0, -1);
            case Direction.Right: return new Vector2Int(0, 1);
            default: return new Vector2Int(0, 0);
        }
    }

    //방향에 상응하는 Vector3 리턴(일반 position 기준) 보정 적용
    public static Vector3 ABSVector3(Direction dir)
    {
        switch (dir)
        {
            case Direction.Up: return new Vector3(0, 1, 0) + new Vector3(-0.5f, 0.5f, 0);
            case Direction.Down: return new Vector3(0, -1, 0) + new Vector3(-0.5f, 0.5f, 0);
            case Direction.Left: return new Vector3(-1, 0, 0) + new Vector3(-0.5f, 0.5f, 0);
            case Direction.Right: return new Vector3(1, 0, 0) + new Vector3(-0.5f, 0.5f, 0);
            default: return new Vector3(0, 0, 0);
        }
    }
    #endregion
}
