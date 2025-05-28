using System.Collections.Generic;
using UnityEngine;

public class AMMonster : MonoBehaviour
{
    public int TeleportCount = 0;                           //텔레포트 횟수
    public static int SpawnCount = 0;                       //스폰 횟수(전역)
    public static bool IsExist = false;                     //현재 존재 여부(전역)

    [Header("적의 추적 방식 Setting")]
    [SerializeField] Transform _player;                     //플레이어 transform
    [SerializeField] Transform _target;                     //타겟
    public Vector2Int CurrentPos;                           //현재 위치
    [SerializeField] private float _moveSpeed = 3f;         //이동 속도
    [SerializeField] private Vector3 _nextPos;              //다음 위치
    [SerializeField] private bool _isMoving = false;        //이동 여부
    [SerializeField] private LayerMask _obstacleLayer;      //장애물 레이어

    [SerializeField] private Animator _animator;            //애니메이터
    private RectTransform _spriteRect;

    private Rigidbody2D _rigid;                             //리지드 바디

    public JumpScareController JSController;               //점프스케어

    public LinkedList<Enterance> TeleportPosList = new LinkedList<Enterance>(); //텔레포트할 입구 관리

    void Start()
    {
        //방어코드 : 한번 더 타겟 설정
        _target = _player;
        _nextPos = transform.position;
        _rigid = GetComponent<Rigidbody2D>();
        _spriteRect = _animator.gameObject.GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        IsExist = true;
        TeleportPosList.Clear();
        _target = _player;
    }

    //추적
    void Update()
    {
        //TeleportPosList 검사
        if (TeleportPosList.Count != 0)
        {
            _target = TeleportPosList.First.Value.transform;
        }
        else _target = _player;

        //움직일때
        if (_isMoving)
        {
            //이동 중이면 계속 이동함
            transform.position = Vector3.MoveTowards(transform.position, _nextPos, _moveSpeed * Time.deltaTime);

            //벡터간의 거리 계산 => sqr이 더 빠름 (도착했을때)
            if ((_nextPos - transform.position).sqrMagnitude < 0.01f)
            {
                transform.position = _nextPos;
                _isMoving = false;
            }
        }
        //타겟 있을때 
        else if (_target != null)
        {
            //이동이 끝났으면 다음 칸 계산
            Vector2 tmpDir = _target.position - transform.position;
            Vector2 tmpMoveDir = Vector2.zero;

            //우선순위 방향 계산
            List<Vector2> directions = new List<Vector2>();

            //절대값 우선순위 (더 큰 쪽) 
            //좌우로 더 심하게 차이가 날 경우 => 우 상 하 좌
            if (CheckPriority(tmpDir.x, tmpDir.y))      
            {
                directions.Add(new Vector2(Mathf.Sign(tmpDir.x), 0));
                directions.Add(new Vector2(0, Mathf.Sign(tmpDir.y)));
                directions.Add(new Vector2(0, -Mathf.Sign(tmpDir.y)));
                directions.Add(new Vector2(-Mathf.Sign(tmpDir.x), 0));
            }
            //상하로 더 심하게 차이가 날 경우 => 상 우 좌 하 
            else
            {
                directions.Add(new Vector2(0, Mathf.Sign(tmpDir.y)));
                directions.Add(new Vector2(Mathf.Sign(tmpDir.x), 0));
                directions.Add(new Vector2(-Mathf.Sign(tmpDir.x), 0));
                directions.Add(new Vector2(0, -Mathf.Sign(tmpDir.y)));
            }

            //다음 방향 결정
            foreach (Vector2 tryDir in directions)
            {
                Vector3 tryPos = transform.position + (Vector3)tryDir;
                if (!Physics2D.OverlapCircle(tryPos, 0.48f, _obstacleLayer))
                {
                    _nextPos = tryPos;
                    _isMoving = true;
                    if (tryDir.x != 0)
                    {
                        Vector3 scale = _spriteRect.localScale;
                        scale.x = tryDir.x > 0 ? 1 : -1;
                        _spriteRect.localScale = scale;

                        //위치 오프셋 처리 (로컬 기준)
                        Vector3 offset = _spriteRect.localPosition;
                        offset.x = tryDir.x > 0 ? 1.2f : -1.2f;
                        _spriteRect.localPosition = offset;
                    }
                    break;
                }

            }
        }
        _animator.SetBool("isMoving", _isMoving);
    }

    //우선순위 평가 (어떤 물체를 추적하느냐에 따라, 이동의 우선순위를 체크)
    private bool CheckPriority(float x, float y)
    {
        //방의 입구를 목표로 삼았을 경우
        if (_target.TryGetComponent<Enterance>(out var enterance))
        {
            switch (enterance.Direction)
            {
                //위, 아래
                case Direction.Up: case Direction.Down:
                        if(Mathf.Abs(x) > Mathf.Abs(y)) return true;
                        else return false;

                //좌, 우
                case Direction.Left: case Direction.Right:
                    if (Mathf.Abs(x) >= Mathf.Abs(y)) return true;
                    else return false;
                default : return false;
            }
        }
        //플레이어를 목표로 삼았을 경우
        else
        {
            if (Mathf.Abs(x) > Mathf.Abs(y))   return true;
            else    return false;   
        }
    }

    //(플레이어가 방을 통과할 때,) 몬스터가 목표로 삼을 방LinkedList에 방의 입구를 추가시켜줌
    public void AddTeleportPos(Enterance enterance, Vector2Int roomPos)
    {
        //텔포 리스트에 아무것도 없으면 일단 추가
        if(TeleportPosList.Count == 0)
        {
            TeleportPosList.AddLast(enterance);
        }
        else
        {
            //플레이어가 이전 방으로 되돌아가는 경우 
            if (enterance.Direction == MapGenerator.OppositeDirection(TeleportPosList.Last.Value.Direction))
            {
                TeleportPosList.RemoveLast();
            }
            //몬스터가 무조건적으로 방의 입구로 형성된 길을 따라가지 않도록 방지
            else if (roomPos == CurrentPos) TeleportPosList.Clear();
            //이 외에는 그냥 추가해주기
            else { TeleportPosList.AddLast(enterance); }

        }
    }

    //몬스터 텔레포트
    public void Teleport(Vector3 position)
    {
        _rigid.position = position;
        transform.position = position;
        _nextPos = position;
        //텔레포트 방 리스트가 비어있지 않을 경우 첫번째 원소를 삭제
        if (TeleportPosList.Count != 0) TeleportPosList.RemoveFirst();
        TeleportCount++;
    }

    #region Trigger
    //충돌 처리
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //입구
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enterance"))
        {
            Direction dir = collision.gameObject.GetComponent<Enterance>().Direction;
            CurrentPos += MapGenerator.MapVector2Int(dir);
            Teleport(MapGenerator.RoomDict[CurrentPos].EnteranceDict[MapGenerator.OppositeDirection(dir)].transform.position
               + MapGenerator.ABSVector3(dir));
        }

        //플레이어
        if (collision.gameObject.layer == LayerMask.NameToLayer("AMPlayer"))
        {
            Debug.Log("당신은 탈출에 실패하였습니다! 정신력 -20");
            collision.gameObject.SetActive(false);
            JSController.StartJumpScare(0.1f, 0.5f, 2f);
        }
    }
    #endregion

    #region Unity Clean-Up
    //초기화 해줄 것들 초기화하기
    private void OnDisable()
    {
        Manager.Instance.SoundManager.PlayBGM(BGMType.BGM_MiniGame);     
        TeleportPosList.Clear();
        IsExist = false;
    }

    private void OnDestroy()
    {
        SpawnCount = 0;
    }
    #endregion
}