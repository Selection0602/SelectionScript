using System.Collections.Generic;
using UnityEngine;

public class EnemyPositions : MonoBehaviour
{
    public Transform _centerPosition;  // 중앙 기준 포인트
    public Transform[] _enemyPositions;  // 일반 몬스터 배치
    public Transform[] _summonPositions;  // 소환수 몬스터 배치

    public List<Vector3> GetPositions(int count)
    {
        List<Vector3> positions = new List<Vector3>();
        for (int i = 0; i < count; i++)
        {
            positions.Add(_enemyPositions[i].position);
        }
        return positions;
    }

    public Vector3 GetCenterPos()
    {
        return _centerPosition.position;
    }

    public Vector3 GetSummonPosition(int num)
    {
        return _summonPositions[num].position;
    }
}
