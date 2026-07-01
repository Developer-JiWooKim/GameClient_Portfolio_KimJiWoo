using UnityEngine;

/// <summary>
/// 몬스터의 컴포넌트를 조합하고 상태 머신(MonsterFSM)에 매 프레임 Tick을 전달하는 컨트롤러
/// </summary>
[RequireComponent(typeof(MonsterSight))]
[RequireComponent(typeof(MonsterMove))]
[RequireComponent(typeof(MonsterFSM))]
[RequireComponent(typeof(MonsterAnim))]
public class MonsterController : MonoBehaviour
{
    private MonsterSight         _monsterSight;
    private MonsterMove          _monsterMove;
    private MonsterFSM           _monsterFSM;
    private MonsterAttackTrigger _monsterAttackTrigger;
    private MonsterFieldOfView   _monsterFOV;
    private MonsterAnim          _monsterAnim;

    private bool _isSensed  = false;         // 타겟 감지 여부를 저장하는 bool
    private bool _isInRange = false;         // 타겟이 감지 반경 안에 들어와 있는지 여부를 저장하는 bool
    private bool _isDetectingPlayer = false; // Chase/Attack 상태(=발각 상태) 진입 여부, 플레이어 표정 알림용

    private Transform _target;
    public Transform Target
    {
        get => _target;
        set => _target = value;
    }

    public MonsterMove   Move   => _monsterMove;
    public MonsterAnim   Anim   => _monsterAnim;
    public MonsterAttackTrigger AttackTrigger => _monsterAttackTrigger;

    public bool    IsSensed       => _isSensed;
    public bool    IsInRange      => _isInRange;
    public Vector3 TargetPosition => _target.position;

    public IMonsterState IdleState   => _monsterFSM.IdleState;
    public IMonsterState ChaseState  => _monsterFSM.ChaseState;
    public IMonsterState AttackState => _monsterFSM.AttackState;

    public void ChangeState(IMonsterState next) => _monsterFSM.ChangeState(next);

    private void Awake() => Initialize();

    /// <summary>
    /// 초기화 메소드
    /// </summary>
    private void Initialize()
    {
        _monsterMove  = GetComponent<MonsterMove>();
        _monsterSight = GetComponent<MonsterSight>();
        _monsterFSM   = GetComponent<MonsterFSM>();
        _monsterAnim  = GetComponent<MonsterAnim>();

        _monsterAttackTrigger = GetComponentInChildren<MonsterAttackTrigger>();
        _monsterFOV           = GetComponentInChildren<MonsterFieldOfView>();

        _monsterFSM.OnStateChanged += OnStateChanged;
    }

    private void OnDestroy()
    {
        _monsterFSM.OnStateChanged -= OnStateChanged;
    }

    /// <summary>
    /// 오브젝트 풀에서 꺼내 재사용할 때 호출, 이전 플레이 상태를 지우고 새 위치/타겟으로 초기화하는 메소드
    /// </summary>
    public void ResetForReuse(Vector3 position, Transform target)
    {
        // 이전 타겟을 감지 중이던 상태로 반납/재사용될 경우, 그 타겟의 표정을 정상으로 되돌려줌
        SetDetectingPlayer(false);

        _target    = target;
        _isSensed  = false;
        _isInRange = false;

        _monsterFSM.ResetState();              // 상태 초기화
        _monsterMove.Warp(position);           // 미로의 랜덤한 위치에 재배치
        _monsterAttackTrigger?.ResetTrigger(); // 공격 트리거 초기화
        _monsterAnim?.PlayIdle();              // 애니메이션 Idle 실행
    }

    /// <summary>
    /// 오브젝트 풀에 반납되어 비활성화되기 전 타겟 참조를 끊는 메소드
    /// </summary>
    public void ClearTarget()
    {
        SetDetectingPlayer(false);

        _target = null;
    }

    /// <summary>
    /// 상태가 변경되면 실행할 이벤트에 등록된 메소드 - 발각(Alert) 상태 여부를 플레이어에게 알림
    /// </summary>
    private void OnStateChanged(IMonsterState next)
    {
        SetDetectingPlayer(next.IsAlertState);
    }

    /// <summary>
    /// 발각 상태가 실제로 바뀔 때만 타겟(플레이어)에게 알려 표정을 전환시키는 메소드
    /// </summary>
    private void SetDetectingPlayer(bool isDetecting)
    {
        if (_isDetectingPlayer == isDetecting) return;

        _isDetectingPlayer = isDetecting;

        if (_target != null && _target.TryGetComponent(out PlayerController player))
        {
            player.NotifyDetected(isDetecting);
        }
    }

    private void FixedUpdate()
    {
        if (_target == null) return;

        Vector3 targetPos = _target.position;

        // 타겟 위치가 탐지 거리 안에 있는지 체크
        _isInRange = _monsterSight.IsInRange(targetPos);

        if(_isInRange)
        {
            // 탐지 거리 안에 들어와 있으면 시야각 안에 들어와 있고 그 사이에 벽이 있는지 체크
            _isSensed = _monsterSight.TargetSense(targetPos);
        }
        else
        {
            _isSensed = false; // 범위 밖이면 감지 여부 초기화
        }
    }

    private void Update()
    {
        if (_target == null) return;

        // 상태 머신에 Tick 위임 - 전환 판단과 행동 실행 모두 현재 IMonsterState가 담당
        _monsterFSM.Tick();

        // 몬스터 시야 메시 그리기
        _monsterFOV?.DrawFieldOfView(transform);
    }
}
