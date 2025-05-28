using System.Collections;
using UnityEngine;

public class AMMonsterSpawner : MonoBehaviour
{
    public Vector3 SpawnPos;                                        //스폰 위치
    public GameObject MonsterPrefab;                                //스폰하는 몬스터
    [SerializeField] private float _timeLimit = 10f;                //스폰 후 존재 시간

    //몬스터 소환
    public void SpawnMonster(Vector2Int pos)
    {
        //현재 플레이어가 있는 방과 방에서 열린 입구 중 하나를 랜덤으로 가져옴
        Room room;
        Direction enteranceRand;
        do
        {
            room = MapGenerator.RoomDict[pos];
            enteranceRand = (Direction)Random.Range(0, room.EnteranceDict.Count);
        }
        while (!room.EnteranceDict[enteranceRand].gameObject.activeSelf);

        //이후 몬스터 스폰위치를 결정지을 때 사용할 보정값
        Vector3 extraVector;
        switch(enteranceRand)
        {
            case Direction.Up: extraVector = new Vector3(-0.5f, - 0.5f, 0); break;
            case Direction.Down: extraVector = new Vector3(-0.5f, 1.5f, 0); break;
            case Direction.Left: extraVector = new Vector3(0.5f ,0.5f , 0); break;
            case Direction.Right: extraVector = new Vector3(-1.5f, 0.5f, 0); break;
            default: extraVector = new Vector3(0, 0, 0); break;
        }

        MonsterPrefab.transform.GetComponent<AMMonster>().CurrentPos = pos;
        MonsterPrefab.SetActive(false);

        //처음에 소환되는 위치를 입구와 닿지 않게 만들기
        SpawnPos = room.EnteranceDict[enteranceRand].transform.position + extraVector;

        //몬스터를 소환
        MonsterPrefab.transform.position = SpawnPos;
        MonsterPrefab.SetActive(true);
        Manager.Instance.SoundManager.PlayBGM(BGMType.BGM_AoOni);
        StartCoroutine(RemoveMonster());
        
        AMMonster.SpawnCount++;
    }

    //스폰 후 n초 이상 지난 후에, 텔레포트를 1회 이상 했을 시 사라짐
    IEnumerator RemoveMonster()
    {
        //timer 만들기
        float curTime = 0f;
        while (_timeLimit >= curTime)
        {
            curTime += Time.deltaTime;
            yield return null;
        }
        MonsterPrefab.GetComponent<AMMonster>().TeleportCount = 0;

        //timer 다 지난 후 텔레포트 검사
        while (MonsterPrefab.GetComponent<AMMonster>().TeleportCount <= 0)
        {
            yield return null;
        }

        //텔레포트 횟수 초기화, 만약 아이템을 다 먹었을 경우 몬스터는 사라지지 않음
        MonsterPrefab.GetComponent<AMMonster>().TeleportCount = 0;
        if (AMMonster.SpawnCount != MapGenerator.ItemCount)
        {
            MonsterPrefab.gameObject.SetActive(false);
        }
    }
}
