using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

public enum LineConnectionType
{
    Normal,
    SameFloor,
    Skip,
}

public class LineFactory : MapObjectFactory<UILineRenderer>
{
    [Header("Line Settings")]
    [SerializeField] private float _lineThickness = 5f; // 라인 두께
    [SerializeField] private Color _lineColor = Color.white; // 라인 색상
    
    [Header("Skip Line Settings")]
    [Range(50, 300)] [SerializeField] private float _skipLineYOffset = 60f; // 스킵 라인 Y 오프셋

    [Header("Line Points Count Settings")]
    [SerializeField] private int _normalLinePointsCount = 15;
    [SerializeField] private int _skipLinePointsCount = 25;
    [SerializeField] private int _sameLayerLinePointsCount = 7;

    public event Func<Node, Node, bool> CheckPathBlocked;

    private List<Vector2> _points = new List<Vector2>();
    
    /// <summary>
    /// 라인 생성 함수
    /// </summary>
    /// <param name="node">기준 노드</param>
    /// <param name="targetNode">기준 노드와 연결된 노드</param>
    /// <param name="nodeRect">기준 노드의 RectTransform</param>
    /// <param name="targetRect">기준 노드와 연결된 노드의 RectTransform</param>
    /// <returns>생성된 UI 라인 렌더러</returns>
    public UILineRenderer CreateLine(Node node, Node targetNode, RectTransform nodeRect, RectTransform targetRect)
    {
        _points.Clear();
        UILineRenderer lineRenderer = InitializeLineRenderer(node, targetNode);
        
        // 노드들의 위치값 가져오기
        Vector2 fromPoint = nodeRect.anchoredPosition; // 라인 시작 노드의 위치
        Vector2 toPoint = targetRect.anchoredPosition; // 라인 끝 노드의 위치
        
        // 시작 노드에서 목표 노드까지의 방향
        Vector2 direction = (toPoint - fromPoint).normalized;
        
        // 노드의 크기 절반 (노드 가장자리를 구하기 위한 값)
        Vector2 fromNodeHalfSize = nodeRect.rect.size / 2; // 라인 시작 노드의 크기 절반
        Vector2 toNodeHalfSize = targetRect.rect.size / 2; // 라인 끝 노드의 크기 절반
    
        // 방향에 따른 노드 가장자리 까지의 거리 계산 (노드 중앙에서 가장자리 까지의 거리)
        float fromDistance = GetEdgeDistance(fromNodeHalfSize, direction);
        float toDistance = GetEdgeDistance(toNodeHalfSize, -direction);
    
        // 라인 시작점과 끝점 구하기 (노드의 가장자리에서 시작)
        Vector2 startPoint = fromPoint + direction * fromDistance;
        Vector2 endPoint = toPoint - direction * toDistance;

        // 라인 렌더러의 위치 설정 및 상대 좌표 계산
        lineRenderer.transform.position = nodeRect.transform.position; // 라인 렌더러의 위치 설정
        Vector2 relativeStartPoint = startPoint - fromPoint; // 시작 노드 위치 기준으로 라인 시작점(시작 노드의 가장자리)의 거리/방향 벡터
        Vector2 relativeEndPoint = endPoint - fromPoint; // 시작 노드 위치 기준으로 라인 끝점(목표 노드의 가장자리)의 거리/방향 벡터

        LineConnectionType connectionType = GetConnectionType(node.Layer, targetNode.Layer);

        // 노드 타입에 따라 포인트 개수 설정
        int linePointsCount = GetLinePointsCount(connectionType);
        
        // 시작점 부터 끝점까지 정해둔 수만큼의 포인트 생성
        for (int i = 0; i < linePointsCount; i++)
        {
            float t = (float)i / (linePointsCount - 1);
            
            // 시작점과 끝점 사이의 비율에 따라 포인트 생성
            Vector2 point = Vector2.Lerp(relativeStartPoint, relativeEndPoint, t);
            _points.Add(point);
        }

        // 스킵 라인 && 시작 노드의 방 위치와 타겟 노드의 차이가 방 한개 차이(RoomPosition 차이)가 아닐 경우 곡선으로 변경 
        // (방 한개 차이 일 경우에는 직선으로(중간에 다른 노드와 교차할 일이 없음))
        if (connectionType == LineConnectionType.Skip && Mathf.Abs(node.RoomPosition - targetNode.RoomPosition) != 1)
        {
            Debug.Log($"노드({node.Layer}, {node.RoomPosition}) -> 타겟({targetNode.Layer}, {targetNode.RoomPosition}), 절대값 : {Mathf.Abs(node.RoomPosition - targetNode.RoomPosition)}");

            // 시작 노드와 타겟 노드의 방 위치가 같고, 사이 층의 같은 위치에 노드가 없다면 직선 라인 유지
            if ((node.RoomPosition == targetNode.RoomPosition || Mathf.Abs(node.RoomPosition - targetNode.RoomPosition) == 2) && 
                CheckPathBlocked?.Invoke(node, targetNode) == false)
                Debug.Log("Pass");
            else
            {
                // 기준 노드와 타겟 노드의 X 좌표가 모두 0이라면 반대 방향으로 곡선 생성
                bool isReverse = node.RoomPosition == 0 && targetNode.RoomPosition == 0;

                for (int i = 1; i < _points.Count - 1; i++)
                {
                    // i / (points.Count - 1)는 0부터 1까지의 값
                    // 여기에 pi를 곱하면 0부터 pi까지의 각도가 됨
                    float angle = (float)i / (_points.Count - 1) * Mathf.PI;

                    // sin(angle)은 다음과 같은 값을 가짐:
                    // 첫 점 : sin(0) = 0
                    // 중간 점 : sin(pi/2) = 1 (최댓값)
                    // 마지막 점 : sin(pi) = 0
                    float sinValue = Mathf.Sin(angle);

                    float curveOffset = sinValue * (isReverse ? -_skipLineYOffset : _skipLineYOffset);
                    _points[i] += new Vector2(0, curveOffset);
                }
            }
        }
        
        // 라인 렌더러 포인트 적용 및 굵기 설정
        lineRenderer.Points = _points.ToArray();
        lineRenderer.LineThickness = _lineThickness;
        lineRenderer.color = _lineColor;

        return lineRenderer;
    }
    
    // 라인렌더러 생성 및 이름 설정
    private UILineRenderer InitializeLineRenderer(Node node, Node targetNode)
    {
        UILineRenderer lineRenderer = CreateObject();
        lineRenderer.gameObject.name = $"Line_{node.Layer}_{node.RoomPosition} to {targetNode.Layer}_{targetNode.RoomPosition}";
        return lineRenderer;
    }
    
    // 방향 벡터와 노드 크기 절반을 이용해 노드의 가장자리까지의 거리 계산
    private float GetEdgeDistance(Vector2 halfSize, Vector2 direction)
    {
        float absX = Mathf.Abs(direction.x);
        float absY = Mathf.Abs(direction.y);
    
        // 수직 방향이 아닌 경우
        if (absX > 0.0001f)
        {
            float x = halfSize.x / absX; 
            float y = halfSize.y / absY;
            return Mathf.Min(x, y);
        }
        // 수직 방향인 경우
        else
            return halfSize.y / absY;
    }

    // 노드와 타겟노드의 Y값을 비교해 연결 타입 반환
    private LineConnectionType GetConnectionType(int nodeY, int targetNodeY)
    {
        // 노드와 타겟노드의 Y값을 비교해 타입 반환
        if (nodeY == targetNodeY)
            return LineConnectionType.SameFloor;
        else if (targetNodeY - nodeY >= 2)
            return LineConnectionType.Skip;
        else
            return LineConnectionType.Normal;
    }
    
    // 연결 타입에 따라 포인트 개수 반환
    private int GetLinePointsCount(LineConnectionType connectionType)
    {
        return connectionType switch
        {
            LineConnectionType.Normal => _normalLinePointsCount,
            LineConnectionType.Skip => _skipLinePointsCount,
            LineConnectionType.SameFloor => _sameLayerLinePointsCount,
            _ => _normalLinePointsCount
        };
    }
}
