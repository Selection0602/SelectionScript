using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GBPlayer : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _speed = 4.5f;                   //이동 속도
    [SerializeField] private Vector2 currentGridPosition;           //현재 그리드 위치
    [SerializeField] private float _gridSize = 1f;                  //그리드 크기
    [SerializeField] private bool _isMoving = false;                //이동 여부
    [SerializeField] private LayerMask _obstacleLayer;              //장애물 레이어

    [SerializeField] private AvoidMenuUI _menuUI;                   //메뉴 UI 오브젝트
    [SerializeField] private Inventory _inventory;                  //인벤토리
    [SerializeField] private TalkBox _talkBox;                      //대화 

    private Vector2 _inputVec;                  //이동 벡터
    private Rigidbody2D _rigid;                 //리지드 바디
    private int _stepCount = 0;                 //스텝 카운트

    [Header("Current Interact Settings")] 
    public Camera MainCam;                      //따라다닐 카메라
    public BaseInteractObject CurrentInteract => _detector.CurrentInteractable;        //현재 상호작용하는 뭁체
    public Button CurrentButton;                //현재 상호작용하는 뭁체

    [Header("Interaction Detector Settings")]
    [SerializeField] private float _detectorDistance = 1.5f;                //감지 범위
    [SerializeField] private Direction _lastDirection = Direction.Up;       //마지막 감지 방향(Direction)
    private GBInteractDetector _detector;                                   //감지
    private Vector2 _lastDirectionVector = Vector2.up;                      //마지막 감지 방향(Vector2)

    [SerializeField] private PlayerInput _playerController;     //플레이어 컨트롤러
    [SerializeField] private Animator _animator;                //애니메이터

    #region Unity Cycle
    //초기 세팅
    void Awake()
    {
        _rigid = GetComponent<Rigidbody2D>();
        _playerController = GetComponent<PlayerInput>();
        currentGridPosition = _rigid.position;
        _animator = GetComponent<Animator>();

        _detector = GetComponentInChildren<GBInteractDetector>();
        UpdateInteractionDetectorPosition();
    }

    private void Start()
    {
        if (!_menuUI) return;
        
        _menuUI._onCloseMenu += () =>
        {
            Time.timeScale = 1f;
            ChangeInput("Player");
        };
    }

    //이동 방법 구현
    void FixedUpdate()
    {
        //이동 중일 때
        if (_isMoving)
        { 
            //현재 위치에서 목표 위치까지 일정 속도로 이동
            Vector2 newPos = Vector2.MoveTowards(_rigid.position, currentGridPosition, _speed * Time.fixedDeltaTime);
            _rigid.MovePosition(newPos);

            //목표 지점에 거의 도달했는지 확인하기
            if (Vector2.Distance(_rigid.position, currentGridPosition) < 0.01f)
            {
                _isMoving = false;
                _animator.SetBool("isMoving", _isMoving);
                _rigid.position = currentGridPosition;
            }
        }
        //입력이 있을 때만 처리
        else if (FilterDirection(_inputVec) != Vector2.zero)
        {
            //이동 방향 정규화(단위 벡터)
            Vector2 moveDirection = FilterDirection(_inputVec).normalized;
            
            //이동할 위치 계산
            Vector2 newPosition = currentGridPosition + moveDirection * _gridSize;
            //장애물이 없는 경우 이동
            if (!Physics2D.OverlapCircle(newPosition, 0.48f, _obstacleLayer))
            {
                _stepCount++;
                _menuUI.StepCount = _stepCount;
                
                currentGridPosition = newPosition;
                _isMoving = true;
                _animator.SetBool("isMoving", _isMoving);
            }
        }
    }

    //카메라 따라다니기
    private void LateUpdate()
    {
        MainCam.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, MainCam.transform.position.z);
    }
    #endregion

    private void UpdateInteractionDetectorPosition()
    {
        if (_detector)
        {
            _detector.transform.localPosition = _lastDirectionVector * _detectorDistance;
        }
    }
    
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

    //그리드에 맞게 텔레포트
    public void Teleport(Vector2 position)
    {
        _rigid.position = position;
        currentGridPosition = position;
    }

    #region Player Input - Player
    //이동 
    void OnMove(InputValue value)
    {
        if (_talkBox.gameObject.activeSelf) return;
        _inputVec = value.Get<Vector2>();
        
        Vector2 moveDirection = FilterDirection(_inputVec);
        if (moveDirection != Vector2.zero)
        {
            _lastDirectionVector = moveDirection;
            _lastDirection = VectorToDirection(moveDirection);
            UpdateInteractionDetectorPosition();
        }
    }

    //z키 - Player일 경우 해당 오브젝트와 상호작용하기
    void OnSelect(InputValue value)
    {
        //대전제 : currentInteractObj는 null이 아님
        if(CurrentInteract != null && CurrentInteract.CanInteract)
        {
            CurrentInteract.Panel.OpenPanel();
            CurrentInteract.Panel.SetSentance(CurrentInteract.Talk);
            if (CurrentInteract.Panel is NormalPanel NP)
            {
                if(CurrentInteract is not KeyPickUpObject) CurrentInteract.FirstCheck();
            }
            ChangeInputToUI();
        }
        //talkBox가 있을 경우
        if (_talkBox.gameObject.activeSelf)
        {
            _talkBox.ProceedToNextDialogue();
        }
    }

    //x키 - 취소
    private void OnClose()
    {
        if (_talkBox.gameObject.activeSelf)
        {
            _talkBox.gameObject.SetActive(false);
            return;
        }

        if(Time.timeScale != 0f)
        {
            _menuUI.OpenMenu(() => ChangeInput("Player"));
            ChangeInput("UI");
        }
        
    }
    #endregion

    #region Player Input - UI

    //방향키 - 버튼 움직이기
    void OnMoveButton(InputValue value)
    {
        Vector2 direction = value.Get<Vector2>();

        if (direction != Vector2.zero)
        {
            if (_menuUI.IsMenuOpen)
            {
                if (direction.y > 0.5f)
                    _menuUI.OnMoveUp();
                else if (direction.y < -0.5f)
                    _menuUI.OnMoveDown();

                if (direction.x > 0.5f)
                    _menuUI.OnMoveRight();
                else if (direction.x < -0.5f)
                    _menuUI.OnMoveLeft();
            }
            else if (CurrentInteract.Panel is IPanelButton IPB)
            {
                IPB.ChangeButton();
                CurrentButton = IPB.DefaultButton;
            }
        }
    }

    //z키 - UI에서 버튼 선택
    void OnSubmit()
    {
        if (_talkBox.gameObject.activeSelf)
        {
            _talkBox.ProceedToNextDialogue();
            return;
        }

        if (_menuUI.IsMenuOpen) _menuUI.OnSubmit();
        else if (CurrentInteract.Panel is IPanelButton)
        {
            Debug.Log("Button Clicked");
            CurrentButton.onClick.Invoke();
        }
        else
        {
            if(CurrentInteract.CanInteract)
            {
                CurrentInteract.DisplayNextTalk();
            }
            else
            {
                CurrentInteract.Panel.ClosePanel();
                _playerController.SwitchCurrentActionMap("Player");
                if(CurrentInteract is KeyUnlockObject GK) GK.TeleportToGround?.Invoke();
                if (CurrentInteract is not KeyPickUpObject Obj) CurrentInteract.CanInteract = true;
            }
        }
    }

    //x키 - UI에서 버튼 선택
    void OnCancel()
    {
        if (_talkBox.gameObject.activeSelf)
        {
            _talkBox.ProceedToNextDialogue();
            return;
        }

        if (_menuUI.IsMenuOpen)
        {
            _menuUI.OnCancel();
        }
        //대전제 : currentInteractObj는 null이 아님
        else if (CurrentInteract != null)
        {
            if(CurrentInteract.Panel is not FinalCheckPanel)
            {
                CurrentInteract.Panel.ClosePanel();
                _playerController.SwitchCurrentActionMap("Player");
            }
            CurrentInteract.CanInteract = true;
        }
    }
    #endregion

    #region Player 키 조작 바꾸기
    //UI로 바꾸기
    public void ChangeInputToUI()
    {
        if (CurrentInteract.Panel is IPanelButton IPB) CurrentButton = IPB.DefaultButton;
        _playerController.SwitchCurrentActionMap("UI");
    }
    //Empty로 바꾸기 (잠시 할당 안된 상태)
    public void ChangeInputToEmpty()
    {
        _playerController.SwitchCurrentActionMap("Empty");
    }
    //Player로 바꾸기
    public void ChangeInputToPlayer()
    {
        _playerController.SwitchCurrentActionMap("Player");
    }
    //actionMap 교체
    private void ChangeInput(string actionMap)
    {
        _playerController.SwitchCurrentActionMap(actionMap);
    }
    #endregion
    
    //Vector2 => Direction으로 바꿔줌
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
