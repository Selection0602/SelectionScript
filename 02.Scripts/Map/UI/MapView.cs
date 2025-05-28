using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class LineData
{
    public LineData(UILineRenderer lineRenderer, Color lineColor)
    {
        LineRenderer = lineRenderer;
        LineColor = lineColor;
    }

    public UILineRenderer LineRenderer { get; }
    public Color LineColor { get; }
}

public class MapView : MonoBehaviour
{
    private Map _currentMap; // 생성된 맵 데이터

    [Header("Save Data")]
    [SerializeField] private RectTransform _scrollViewContentTransform; // 스크롤뷰 위치 저장용

    [Header("Map Object Factory")]
    [SerializeField] private NodeViewFactory _nodeViewFactory;
    [SerializeField] private LineFactory _lineFactory;
    [SerializeField] private PlayerIconView _playerIconView;

    [Header("Moveable Node Animation")]
    [SerializeField] private Color _moveableLineColor;
    [SerializeField] private float _moveableAnimationDuration = 1f;

    [Header("Color Offset")]
    [SerializeField] private float _nonMoveableColorOffSet = 0.8f;
    [SerializeField] private float _visitColorOffSet = 0.5f;

    private Dictionary<Node, NodeView> _spawnNodes = new(); // 생성된 노드와 노드뷰 저장
    private Dictionary<(Node, Node), UILineRenderer> _connectionLines = new(); // 연결된 노드끼리의 선 이미지 저장
    private Dictionary<NodeView, LineData> _moveableNodeLines = new(); // 현재 애니메이션 재생 중인 노드와 라인 저장

    public event Func<Node, bool> CheckNodeMoveable;

    #region 아틀라스 초기화
    
    [Header("Sprite Atlas")]
    [SerializeField] private SpriteAtlas _spriteAtlas;
    [SerializeField] private Image _mapBackground;
    [SerializeField] private Image[] _mapBorders;
    private const string ATLAS_LABEL = "MapBG";
    private const string MAP_BACKGROUND_SPRITE_NAME = "MapBackground";
    private const string MAP_BORDER_SPRITE_NAME = "MapBorder";
    
    private async void Awake() 
    {
        _spriteAtlas = await Manager.Instance.AddressableManager.Load<SpriteAtlas>(ATLAS_LABEL);
        SetSpriteAtlas();
    }

    private void SetSpriteAtlas()
    {
        _mapBackground.sprite = _spriteAtlas.GetSprite(MAP_BACKGROUND_SPRITE_NAME);

        Sprite border = _spriteAtlas.GetSprite(MAP_BORDER_SPRITE_NAME);
        for (int i = 0; i < _mapBorders.Length; i++)
            _mapBorders[i].sprite = border;
    }
    #endregion
    
    private void Start()
    {
        _lineFactory.CheckPathBlocked += IsPathBetweenBlocked;
    }

    #region 맵 UI 초기화 및 설정
    /// <summary>
    /// 화면에 맵을 띄워주는 함수
    /// </summary>
    /// <param name="map">띄워줄 맵 데이터</param>
    /// <param name="nodeTable">노드 정보가 담긴 테이블</param>
    /// <param name="contentPosition">스크롤 뷰 컨텐츠 위치(맵 이동 후 복귀 시 또는 불러오기 시 사용)</param>
    public void InitializeMapView(Map map, NodeTable nodeTable, Vector2 contentPosition = default)
    {
        // 스크롤뷰 위치가 기본값이라면 현재 위치로 설정
        if (contentPosition != default)
        {
            _scrollViewContentTransform.offsetMin = new Vector2(contentPosition.x, _scrollViewContentTransform.offsetMin.y);
            _scrollViewContentTransform.offsetMax = new Vector2(contentPosition.y, _scrollViewContentTransform.offsetMax.y);
        }

        // 매개변수로 받은 맵 데이터 저장
        _currentMap = map;

        // 노드뷰 팩토리의 nodeTable 초기화
        _nodeViewFactory.SetNodeTable(nodeTable);

        ClearMap();
        SpawnNodes();
        ConnectNodes();
    }

    private void ClearMap()
    {
        _nodeViewFactory.ClearObjects();
        _lineFactory.ClearObjects();
        _playerIconView.ClearObject();
        
        _spawnNodes.Clear();
        _connectionLines.Clear();
        _moveableNodeLines.Clear();
    }
    #endregion

    #region 노드 생성 및 연결
    private void SpawnNodes()
    {
        if (_currentMap == null) return;

        // 맵에 노드 생성
        foreach (var (y, nodes) in _currentMap.MapInfo)
        {
            // 현재 층의 노드 배열 길이만큼 반복
            foreach (var nodeData in nodes)
            {
                // 노드 데이터가 null이거나 노드 타입이 None이라면 패스
                if (nodeData == null || nodeData.Type == NodeType.None) continue;

                // 노드뷰 오브젝트 생성
                NodeView nodeView = _nodeViewFactory.CreateNodeView(nodeData, _currentMap.RoomCount);

                // nodeData, nodeView 딕셔너리에 저장
                _spawnNodes.Add(nodeData, nodeView);
            }
        }
    }

    private void ConnectNodes()
    {
        foreach (var (node, nodeView) in _spawnNodes)
        {
            if (node.ConnectedNodes == null) continue;

            foreach (var connectedNode in node.ConnectedNodes)
            {
                // connectedNode에 해당하는 노드뷰가 존재하지 않으면 패스
                if (!_spawnNodes.TryGetValue(connectedNode, out NodeView connectedNodeView)) continue;

                var nodeRect = nodeView.RectTransform;
                var connectedNodeRect = connectedNodeView.RectTransform;

                // 라인 렌더러 생성
                var lineRenderer = _lineFactory.CreateLine
                    (node, connectedNode, nodeRect, connectedNodeRect);

                // 연결된 노드(노드, 연결된 노드)와 라인 렌더러 저장
                _connectionLines.Add((node, connectedNode), lineRenderer);
            }
        }
    }
    #endregion

    #region 노드 및 라인 색상 변경
    /// <summary>
    /// 방문한 노드 및 선 색상 변경 함수
    /// </summary>
    /// <param name="currentNode">현재 노드</param>
    /// <param name="targetNode">이동 할 노드(라인 색 변경 시 사용)</param>
    public void ChangeNodeAndLineColorToVisited(Node currentNode, Node targetNode)
    {
        // 지나온 노드 색상 변경
        if (_spawnNodes.TryGetValue(currentNode, out NodeView nodeView))
            nodeView.ChangeColor(_visitColorOffSet);

        // 지나온 라인 색상 변경
        if (!_connectionLines.TryGetValue((currentNode, targetNode), out UILineRenderer lineRenderer)) return;
        lineRenderer.color = GetLineColorToVisited(lineRenderer.color);

        // 같은 층 반복 노드의 경우 양쪽 두 선을 모두 변경
        if (_connectionLines.TryGetValue((targetNode, currentNode), out lineRenderer))
            lineRenderer.color = GetLineColorToVisited(lineRenderer.color);
    }

    /// <summary>
    /// 플레이어가 방문하지 않은 노드 및 선 색상 변경 함수
    /// </summary>
    /// <param name="currentNode">현재 플레이어 노드</param>
    /// <param name="playerPath">플레이어 경로</param>
    /// <param name="isLoad">저장된 데이터를 불러오는 중이라면 true</param>
    public void ChangeNodeColorToNonMoveable(Node currentNode, List<Node> playerPath, bool isLoad = false)
    {
        // 플레이어 층의 한 층 아래의 노드들 혹은 isLoad가 true라면 아래 전체 층의 노드들 가져오기
        var nodes = _spawnNodes.Keys.Where(x => !isLoad ? x.Layer == currentNode.Layer - 1 : x.Layer < currentNode.Layer);

        foreach (var node in nodes)
        {
            // 플레이어 경로에 포함 되어 있다면 색상 변경
            if (!playerPath.Contains(node) && _spawnNodes.TryGetValue(node, out NodeView nodeView))
            {
                nodeView.ChangeColor(_nonMoveableColorOffSet);
            }
        }
    }
    
    // 지나온 라인 색상 변경
    private Color GetLineColorToVisited(Color originalColor)
    {
        Color visitedColor = originalColor * 0.5f;
        visitedColor.a = originalColor.a;
        return visitedColor;
    }
    #endregion

    #region 노드 및 라인 애니메이션 관리
    private void MoveableNodeAnimation(Node node)
    {
        // 이전 애니메이션 정지
        StopAllMoveableAnimation();

        // 이동 가능한 노드 및 라인 애니메이션 시작
        foreach (var connectedNode in node.ConnectedNodes)
        {
            StartMoveableAnimation(node, connectedNode);
        }
    }

    // 이동가능 노드/라인 애니메이션 재생
    private void StartMoveableAnimation(Node node, Node targetNode)
    {
        // 현재 노드에서 타겟 노드로 이동 가능한지 확인
        if (CheckNodeMoveable?.Invoke(targetNode) ?? false) return;

        // 타겟노드 노드뷰 가져오기
        if (!_spawnNodes.TryGetValue(targetNode, out NodeView connectedNodeView)) return;
        // 현재 노드에서 타겟 노드로 이어지는 라인 가져오기
        if (!_connectionLines.TryGetValue((node, targetNode), out UILineRenderer lineRenderer)) return;

        _moveableNodeLines[connectedNodeView] = new LineData(lineRenderer, lineRenderer.color);

        connectedNodeView.StartMoveableAnimation();

        StartLineAnimation(lineRenderer);

        // 양쪽으로 연결되어 있는 경우 반대쪽 라인도 애니메이션 설정
        if (_connectionLines.TryGetValue((targetNode, node), out UILineRenderer backwardLine))
        {
            if (_spawnNodes.TryGetValue(node, out NodeView sourceNodeView))
            {
                _moveableNodeLines[sourceNodeView] = new LineData(backwardLine, backwardLine.color);
                StartLineAnimation(backwardLine);
            }
        }
    }

    // 라인 애니메이션 재생
    private void StartLineAnimation(UILineRenderer lineRenderer)
    {
        lineRenderer.DOColor(_moveableLineColor, _moveableAnimationDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.Linear);
    }

    // 이동 가능 노드/라인 애니메이션 정지
    private void StopAllMoveableAnimation()
    {
        foreach (var nodeAndLine in _moveableNodeLines)
        {
            nodeAndLine.Key.StopMoveableAnimation();

            nodeAndLine.Value.LineRenderer.DOKill();
            nodeAndLine.Value.LineRenderer.color = nodeAndLine.Value.LineColor;
        }
        _moveableNodeLines.Clear();
    }
    #endregion

    #region 플레이어 위치 표시
    /// <summary>
    /// 플레이어 위치를 화면에 띄워주는 함수
    /// </summary>
    /// <param name="playerNode">플레이어가 위치한 노드</param>
    public void ShowPlayerPosition(Node playerNode)
    {
        if (!_spawnNodes.TryGetValue(playerNode, out NodeView targetNodeView))
        {
            Debug.LogError("플레이어 노드가 존재하지 않습니다.");
            return;
        }

        // 아웃라인 비활성화
        targetNodeView.Outline.enabled = false;

        // 플레이어 아이콘 위치 설정
        _playerIconView.ShowPlayerPosition(targetNodeView.transform.position);
        // 플레이어 노드 기준 이동 가능 노드 및 라인 애니메이션 시작
        MoveableNodeAnimation(playerNode);
    }
    #endregion

    #region 노드 사이 경로 차단 확인(라인 생성 시 사용)
    private bool IsPathBetweenBlocked(Node node, Node targetNode)
    {
        // 노드와 타겟노드 사이의 노드들 가져오기
        var betweenNodes = GetNodesBetweenLayers(node.Layer, targetNode.Layer);

        foreach (var betweenNode in betweenNodes)
        {
            // 노드와 타겟노드가 같은 위치에 있으며 두 노드 사이에 같은 위치에 노드가 있다면 true
            if (node.RoomPosition == betweenNode.RoomPosition && targetNode.RoomPosition == betweenNode.RoomPosition)
                return true;

            if (node.RoomPosition - targetNode.RoomPosition == 2 && node.RoomPosition - betweenNode.RoomPosition == 1)
                return true;

            if (node.RoomPosition - targetNode.RoomPosition == -2 && node.RoomPosition - betweenNode.RoomPosition == -1)
                return true;
        }

        return false;
    }

    private List<Node> GetNodesBetweenLayers(int startLayer, int endLayer)
    {
        List<Node> betweenNodes = new List<Node>();

        // startLayer와 endLayer 중 작은 값과 큰 값을 구함
        int minLayer = Math.Min(startLayer, endLayer);
        int maxLayer = Math.Max(startLayer, endLayer);

        foreach (var pair in _spawnNodes)
        {
            Node node = pair.Key;
            // 노드가 minLayer와 maxLayer 사이에 있는지 확인
            if (node.Layer > minLayer && node.Layer < maxLayer)
            {
                betweenNodes.Add(node);
            }
        }

        return betweenNodes;
    }
    #endregion
    
    /// <summary>
    /// 스크롤 뷰 컨텐츠 위치 저장
    /// </summary>
    /// <returns>스크롤 뷰 컨텐츠 위치 반환</returns>
    public Vector2 GetScrollViewContentPosition() =>
        new(_scrollViewContentTransform.offsetMin.x, _scrollViewContentTransform.offsetMax.x);
    // rect.offSetMin = Vector2(Left, Bottom)
    // rect.offSetMax = Vector2(Right, Top)
}