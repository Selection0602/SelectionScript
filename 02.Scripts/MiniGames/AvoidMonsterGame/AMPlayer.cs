using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class AMPlayer : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _speed = 4.5f;                   //이동 속도
    [SerializeField] private Vector2 _currentGridPosition;          //현재 그리드 위치
    [SerializeField] private bool _isMoving = false;                //이동 여부
    [SerializeField] private LayerMask _obstacleLayer;              //장애물 레이어
    [SerializeField] private float _obstacleCheckRadius = 0.15f;    //장애물 체크 반경
    private Vector2 _previousPos;           //이전 위치

    private Vector2 _inputVec;              //이동 벡터
    private Rigidbody2D _rigid;             //리지드 바디
    private PlayerInput _playerInput;       //플레이어 컨트롤러

    public Camera MainCam;                  //따라다닐 카메라

    public Vector2Int CurrentPos;           //현재 위치

    [SerializeField] private TalkBox _talkBox;         //UI TalkBox 창
    public TalkBox TalkBox => _talkBox;

    private Animator _animator;
    
    [SerializeField] private GameObject _curItem;      //현재 상호작용중인 아이템 
    private Inventory _inventory;
    public Inventory Inventory => _inventory;
    
    private InteractDetector _interactDetector;
    [SerializeField] private float _detectorDistance = 1.5f;
    
    private Vector2 _lastDirectionVector = Vector2.up;
    [SerializeField] private Direction _lastDirection = Direction.Up;
    
    [Header("몬스터 생성 이벤트 (아이템 수집 후)")]  
    public UnityEvent<Vector2Int> OnSpawnMonster;       
    
    [Header("게임 클리어 이벤트 (출구 접촉 후)")]  
    public UnityEvent OnGameClear;

    [Header("입구를 통해 텔레포트시 몬스터에게 입구 정보 넘김")]
    public UnityEvent<Enterance, Vector2Int> SubmitTeleportedEnterance;

    [Header("UI")]
    [SerializeField] private AvoidMenuUI _menuUI; // 메뉴 UI 오브젝트
    public AvoidMenuUI MenuUI => _menuUI;
    [SerializeField] private ImagePanel _imagePanel;

    #region Life Cycle 
    //초기 설정
    void Awake()
    {
        _rigid = GetComponent<Rigidbody2D>();
        _playerInput = GetComponent<PlayerInput>();
        _inventory = GetComponent<Inventory>();
        _interactDetector = GetComponentInChildren<InteractDetector>();
        CurrentPos = new Vector2Int(0, 0);
        _animator = GetComponent<Animator>();

        UpdateInteractionDetectorPosition();
    }

    private void Start()
    {
        //메뉴
        if (_menuUI)
        {
            _menuUI._onCloseMenu += () =>
            {
                Time.timeScale = 1f;
                ChangeInput("Player");
            };
        }
        //인벤토리
        if (_inventory)
        {
            _inventory.OnSpawnItemAdded += () => OnSpawnMonster?.Invoke(CurrentPos);
        }
        //이미지 패널
        if (_imagePanel)
        {
            _imagePanel.OnAnimationStarted += () => ChangeInput("Empty");
            _imagePanel.OnAnimationFinished += () => ChangeInput("Player");
        }
    }

    //이동
    void FixedUpdate()
    {
        //이동 중일 때
        if (_isMoving)
        {
            //현재 위치에서 목표 위치까지 일정 속도로 이동
            Vector2 newPos = Vector2.MoveTowards(_rigid.position, _currentGridPosition, _speed * Time.fixedDeltaTime);
            _rigid.MovePosition(newPos);

            //목표 지점에 거의 도달했는지 확인하기
            if (Vector2.Distance(_rigid.position, _currentGridPosition) < 0.01f)
            {
                _isMoving = false;
                _rigid.position = _currentGridPosition;
            }
        }
        //입력이 있을 때만 처리
        else if (FilterDirection(_inputVec) != Vector2.zero)
        {
            //이동 방향 정규화(단위 벡터)
            Vector2 moveDirection = FilterDirection(_inputVec).normalized;

            //이동할 위치 계산
            Vector2 newPosition = _currentGridPosition + moveDirection;
            //장애물이 없는 경우 이동
            if (!Physics2D.OverlapCircle(newPosition, _obstacleCheckRadius, _obstacleLayer))
            {
                _previousPos = _currentGridPosition;
                _menuUI.StepCount++;
                
                _currentGridPosition = newPosition;
                _isMoving = true;
            }
        }
        _animator.SetBool("isMoving", _isMoving);
    }

    //카메라 따라오기
    private void LateUpdate()
    {
        MainCam.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, MainCam.transform.position.z);
    }
    #endregion

    //상호작용 가능한 위치 업데이트
    private void UpdateInteractionDetectorPosition()
    {
        if (_interactDetector)
        {
            _interactDetector.transform.localPosition = _lastDirectionVector * _detectorDistance;
        }
    }

    
    #region Input System - Player
    //이동
    void OnMove(InputValue value)
    {
        _inputVec = value.Get<Vector2>();
        
        Vector2 moveDirection = FilterDirection(_inputVec);
        
        if (moveDirection != Vector2.zero)
        {
            _lastDirectionVector = moveDirection;
            _lastDirection = VectorToDirection(moveDirection);
            UpdateInteractionDetectorPosition();
        }
    }
    
    //C키, 아이템 수집 및 창 열기
    void OnCollect()
    {   
        if(_talkBox.gameObject.activeSelf)
        {
            // 대화 진행
            _talkBox.ProceedToNextDialogue();
        }
        else
        {
            if(AMMonster.IsExist)  
                return;
            
            IInteractable interactable = _interactDetector.CurrentInteractable;

            if (interactable == null)
                return;
            
            if (interactable.InteractableDirection != Direction.None && 
                interactable.InteractableDirection != _lastDirection)
                return;
            
            if (interactable is { IsInteractable: true })
            {
                Time.timeScale = 0f;
                
                if(interactable is not Piano)
                    interactable.ProcessInteraction(this);
                else
                    interactable.ProcessInteraction(this, () => OnSpawnMonster?.Invoke(CurrentPos));
            }
        }
    }
    
    //X키, 아이템 수집 후 창 닫기
    void OnClose()
    {
        if (Time.timeScale == 0f)
        {
             Time.timeScale = 1f;
             
             if (_talkBox.gameObject.activeSelf)
                 _talkBox.gameObject.SetActive(false);
            
             IInteractable interactable = _interactDetector.CurrentInteractable;

            interactable?.OnInteractionComplete();
        }
        else
        {
            if (AMMonster.IsExist) return;
            
            _menuUI.OpenMenu();
            ChangeInput("UI");
        }
    }
    #endregion

    #region Input System - UI

    private void OnMoveButton(InputValue value)
    {
        Vector2 direction = value.Get<Vector2>();
    
        // 방향키 입력에 따라 적절한 메서드 호출
        if (direction.y > 0.5f)
            _menuUI.OnMoveUp();
        else if (direction.y < -0.5f)
            _menuUI.OnMoveDown();

        if (direction.x > 0.5f)
            _menuUI.OnMoveRight();
        else if (direction.x < -0.5f)
            _menuUI.OnMoveLeft();
    }

    private void OnSubmit()
    {
        _menuUI.OnSubmit();
    }

    private void OnCancel()
    {
        _menuUI.OnCancel();
    }
    # endregion
    
    #region OnTrigger
    //충돌 처리 (Enter)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //입구
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enterance"))
        {
            Enterance enterance = collision.gameObject.GetComponent<Enterance>();
            Direction dir = enterance.Direction;

            // 문이 잠겨있는지 확인
            if (enterance.IsLocked)
            {
                if (_inventory.HasItem(enterance.RequiredKeyItemId))
                {
                    Time.timeScale = 0f;
                    
                    // 열쇠가 있으면 문 열기
                    AvoidItemData itemData = _inventory.GetItemData(enterance.RequiredKeyItemId);
                    _talkBox.StartDialogue
                    (
                        new string[] { $"{itemData.ItemName}을(를) 사용했다." },
                        () =>
                        {
                            Time.timeScale = 1f;
                            
                            Room currentRoom = MapGenerator.RoomDict[CurrentPos];
                            // 현재 방에서 잠긴 입구 잠금 해제
                            currentRoom.UnlockEntrance(dir);

                            bool isExit = enterance.IsExit;

                            if (isExit)
                            {
                                Manager.Instance.CreditManager.UnlockMiniGameAndSave(2);
                                OnGameClear?.Invoke();
                                return;
                            }
                            
                            Vector2Int nextPos = CurrentPos + MapGenerator.MapVector2Int(dir);
                            // 다음 방의 모든 입구 잠금 해제
                            if (MapGenerator.RoomDict.ContainsKey(nextPos))
                                MapGenerator.RoomDict[nextPos].UnlockAllEntrances();
                            
                            // 원래 사용하던 텔레포트 로직
                            CurrentPos += MapGenerator.MapVector2Int(dir);
                            SubmitTeleportedEnterance?.Invoke(enterance, CurrentPos);

                            Teleport(MapGenerator.RoomDict[CurrentPos].EnteranceDict[MapGenerator.OppositeDirection(dir)].transform
                                         .position + MapGenerator.ABSVector3(dir));
                        }
                    );
                }
                else
                {
                    Time.timeScale = 0f;
                    _talkBox.StartDialogue(new string[] { "잠겨있다." }, () =>
                    {
                        Time.timeScale = 1f;
                        
                        _currentGridPosition = _previousPos;
                        _rigid.position = _previousPos;
                        _isMoving = false;
                    });
                }

                return;
            }
            
            Vector2Int nextPos = CurrentPos + MapGenerator.MapVector2Int(dir);

            if (MapGenerator.RoomDict.ContainsKey(nextPos))
            {
                CurrentPos = nextPos;

                if (AMMonster.IsExist)
                    SubmitTeleportedEnterance?.Invoke(enterance, CurrentPos);

                Teleport(MapGenerator.RoomDict[CurrentPos].EnteranceDict[MapGenerator.OppositeDirection(dir)].transform
                    .position + MapGenerator.ABSVector3(dir));
            }
        }
    }
    #endregion

    //한 방향으로만 리턴
    private Vector2 FilterDirection(Vector2 input)
    {
        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            return new Vector2(Mathf.Sign(input.x), 0f);
        else if (Mathf.Abs(input.y) > 0)
            return new Vector2(0f, Mathf.Sign(input.y));
        else
            return Vector2.zero;
    }

    //텔레포트
    private void Teleport(Vector2 position)
    {
        _rigid.position = position;
        _currentGridPosition = position;
    }

    //PlayerInput 활성화
    public void EnablePlayerInput()
    {
        _playerInput.enabled = true;
    }

    //PlayerInput 비활성화  
    public void DisablePlayerInput()
    {
        _playerInput.enabled = false;
    }
    
    //Action맵 교체
    public void ChangeInput(string actionMap)
    {
        _playerInput.SwitchCurrentActionMap(actionMap);
    }
    
    //Vector2가 어느 방향인지 Direction 리턴
    private Direction VectorToDirection(Vector2 vector)
    {
        if (vector.normalized == Vector2.up)
            return Direction.Up;
        if (vector.normalized == Vector2.down)
            return Direction.Down;
        if (vector.normalized == Vector2.left)
            return Direction.Left;
        if (vector.normalized == Vector2.right)
            return Direction.Right;
            
        return Direction.None;
    }
}
