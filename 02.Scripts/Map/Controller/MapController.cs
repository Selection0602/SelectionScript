using System;
using UnityEngine;

public class MapController : MonoBehaviour
{
    [Header("Map Settings")] 
    [SerializeField] private int _totalX; // 층에 생성할 방 수
    [SerializeField] private int _totalY; // 총 층 수
    [SerializeField] private int _seed; // 랜덤 시드 값

    [Header("Components")] 
    [SerializeField] private MapInitializer _mapInitializer;
    [SerializeField] private PlayerPathController _playerPathController;
    [SerializeField] private MapDataHandler _mapDataHandler;
    [SerializeField] private NodeActionHandler _nodeActionHandler;
    [SerializeField] private MapUnfoldAnimation _mapAnimation;

    [Header("Node Table")]
    [SerializeField] private NodeTable _nodeTable; // 노드 관련 데이터가 저장된 테이블
    
    private MapManager _mapManager;
    private Map _currentMap; // 현재 맵
    private MapView _mapView; // 맵 뷰


    private void Start()
    {
        _nodeTable.Initialize();
        
        try
        {
            Manager.Instance.SaveManager.SaveGame();
        }
        catch (NullReferenceException ex)
        {
            Debug.LogWarning($"저장 중 NullReference 예외 발생: {ex.Message}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"저장 중 예외 발생: {ex.Message}");
        }

        // 맵 애니메이션 완료 후 컨트롤러 초기화
        _mapAnimation.OnAnimationCompleted += InitializeController;
        _mapManager = Manager.Instance.MapManager;
        
        ShowMap();
    }

    // 맵 생성 + 맵 애니메이션 재생
    private void ShowMap()
    {
        if (_mapManager.canSkip)
        {
            _mapAnimation.SkipAnimation();
            CheckTutorial();
        }
        else
        {
            _mapAnimation.PlayAnimation();
            _mapManager.canSkip = true;
        }
    }

    // 컨트롤러 초기화
    private void InitializeController()
    {
        _mapView = GetComponentInChildren<MapView>();
        _mapView.CheckNodeMoveable += _playerPathController.HasVisitedNode;

        InitializeMap(_totalX, _totalY, _seed);
    }

    // 맵 생성 및 맵 뷰 초기화
    private void InitializeMap(int totalX, int totalY, int seed = 0)
    {
        MapData savedData = _mapDataHandler.GetSavedMapData();
        // 저장된 맵 데이터가 존재한다면
        if (savedData != null)
        {
            // 맵 데이터 불러오기
            LoadSavedMapData(savedData);
            return;
        }

        // 맵 생성
        _currentMap = _mapInitializer.CreateMap(totalX, totalY, _nodeTable, seed);
        Node startNode = _mapInitializer.GetStartNode();

        // 생성한 맵 화면에 띄워주기
        _mapView.InitializeMapView(_currentMap, _nodeTable);

        // 플레이어 노드 설정
        _playerPathController.SetPosition(startNode);
        
        // 생성된 맵 데이터 저장
        _mapDataHandler.SaveMapData
        (_currentMap, _playerPathController.CurrentNode, _playerPathController.Path,
            _mapView.GetScrollViewContentPosition());
    }

    /// <summary>
    /// 플레이어 위치 이동 시도
    /// </summary>
    /// <param name="clickedNode">클릭한 노드</param>
    public void TryUpdatePlayerPosition(Node clickedNode)
    {
        // TryMoveToNode에서 플레이어의 현재 위치가 업데이트 되기 때문에 
        // ChangeNodeAndLineColorToVisited에 전달해줄 노드를 미리 저장
        Node currentNode = _playerPathController.CurrentNode;

        // 클릭한 노드가 현재 플레이어 노드와 연결되어 있다면(이동 가능하다면)
        if (_playerPathController.TryMoveToNode(clickedNode))
        {
            _mapView.ChangeNodeColorToNonMoveable(_playerPathController.CurrentNode, _playerPathController.Path);
            _mapView.ChangeNodeAndLineColorToVisited(currentNode, clickedNode);

            // 맵 매니저에 저장해둘 맵 데이터 생성
            _mapDataHandler.SaveMapData
            (_currentMap, _playerPathController.CurrentNode, _playerPathController.Path,
                _mapView.GetScrollViewContentPosition());

            // 노드 액션 핸들러 생성 및 노드 액션 처리
            _nodeActionHandler.HandleNodeAction(clickedNode.Type);
        }
        else
            Debug.Log("이동할 수 없는 노드입니다.");
    }

    // 저장된 데이터 불러오기
    private void LoadSavedMapData(MapData mapData)
    {
        _currentMap = mapData.CurrentMap;

        // 저장된 맵 데이터 기반으로 맵 뷰 초기화
        _mapView.InitializeMapView(_currentMap, _nodeTable, mapData.ContentPosition);
        // 플레이어 위치 설정
        _playerPathController.SetPosition(mapData.PlayerNode, mapData.PlayerPath);

        DisplayPlayerPath();
    }

    // 저장된 맵 데이터를 불러온 이후 플레이어 경로에 포함된 노드 및 라인들 색상 변경
    private void DisplayPlayerPath()
    {
        for (int i = 0; i < _playerPathController.Path.Count; i++)
        {
            // 현재 노드가 마지막 노드라면 nextNode를 현재 플레이어 노드로 설정
            Node nextNode = i < _playerPathController.Path.Count - 1
                ? _playerPathController.Path[i + 1]
                : _playerPathController.CurrentNode;

            // 플레이어가 방문한 노드 및 라인 색상 변경
            _mapView.ChangeNodeAndLineColorToVisited(_playerPathController.Path[i], nextNode);
        }
        // 플레이어가 이미 지나왔지만 방문하지 않은 노드 및 라인 색상 변경
        _mapView.ChangeNodeColorToNonMoveable(_playerPathController.CurrentNode, _playerPathController.Path, true);
    }

    private void CheckTutorial()
    {

    }
}