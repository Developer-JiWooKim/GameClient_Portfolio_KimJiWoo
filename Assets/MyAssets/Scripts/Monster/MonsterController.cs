using UnityEngine;

/// <summary>
/// 몬스터의 행동을 결정하고 조종하는 컨트롤러
/// </summary>
public class MonsterController : MonoBehaviour
{
    private MonsterSight       _monsterSight;
    private MonsterMove        _monsterMove;
    private MonsterFSM         _monsterFSM;
    private MonsterAttack      _monsterAttack;
    private MonsterFieldOfView _monsterFOV;

    private bool _isSensed  = false; // 타겟 감지 여부를 저장하는 bool
    private bool _isInRange = false; // 타겟이 감지 반경 안에 들어와 있는지 여부를 저장하는 bool

    private float _attackInterval = 1.5f;
    private float _attackTimer    = 0f;

    private Transform _target;
    public Transform Target
    {
        get => _target;
        set => _target = value;
    }

    private void Start() => Initialize();

    /// <summary>
    /// 초기화 메소드
    /// </summary>
    private void Initialize()
    {
        _monsterMove  = GetComponent<MonsterMove>();
        _monsterSight = GetComponent<MonsterSight>();
        _monsterFSM   = GetComponent<MonsterFSM>();

        _monsterAttack = GetComponentInChildren<MonsterAttack>();
        _monsterFOV    = GetComponentInChildren<MonsterFieldOfView>();

        _monsterFSM.OnStateChanged += OnStateChanged;
    }

    private void OnDestroy()
    {
        _monsterFSM.OnStateChanged -= OnStateChanged;
    }

    /// <summary>
    /// 상태가 변경되면 실행할 이벤트에 등록된 메소드
    /// </summary>
    private void OnStateChanged(MonsterFSM.State next)
    {
        switch (next)
        {
            // Idle 상태가 되면 Path를 초기화
            case MonsterFSM.State.Idle:
                _monsterMove.ClearPath();
                break;
        }
    }

    private void FixedUpdate()
    {
        if (_target == null) return;

        // 타겟 위치가 탐지 거리 안에 있는지 체크
        _isInRange = _monsterSight.IsInRange(_target.position); 
        
        if(_isInRange)
        {
            // 탐지 거리 안에 들어와 있으면 시야각 안에 들어와 있고 그 사이에 벽이 있는지 체크
            _isSensed = _monsterSight.TargetSense(_target.position);
        }
        else
        {
            _isSensed = false; // 범위 밖이면 감지 여부 초기화
        }
    }

    private void Update()
    {
        if (_target == null) return;        

        // 감지 상태에 따라 몬스터의 상태를 변경
        _monsterFSM.Evaluate(_isSensed, _isInRange);

        // 정해진 상태에 따라 몬스터 행동 실행
        MonsterAI();

        // 몬스터 시야 메시 그리기
        _monsterFOV?.DrawFieldOfView(transform);
    }

    /// <summary>
    /// 몬스터 행동 메소드
    /// </summary>
    private void MonsterAI()
    {
        switch (_monsterFSM.Current)
        {
            case MonsterFSM.State.Idle:
                _monsterMove.Patrol();
                break;
            case MonsterFSM.State.Chase:
                _monsterMove.MoveToTarget(_target.position);
                if(_monsterAttack.PlayerInAttackRange)
                {
                    _monsterFSM.SetAttack(true);
                }
                break;
            case MonsterFSM.State.Attack:
                _monsterMove.LookAtTarget(_target.position);
                if(!_monsterAttack.PlayerInAttackRange)
                {
                    _monsterFSM.SetAttack(false);
                }
                else
                {
                    TryAttack();
                }
                break;
        }
    }

    /// <summary>
    /// 몬스터 공격에 Interval을 주어 플레이어 체력이 한번에 0이 되지 않게하는 메소드
    /// </summary>
    private void TryAttack()
    {
        _attackTimer -= Time.deltaTime;
        if (_attackTimer > 0f) return;

        _attackTimer = _attackInterval;
        _monsterAttack.Player?.TakeDamage();
    }
}
