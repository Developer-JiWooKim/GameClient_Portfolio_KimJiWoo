using System;
using UnityEngine;

/// <summary>
/// 몬스터의 상태 전환을 관리하는 상태 머신 (State Pattern Context)
/// 각 상태의 행동/전환 조건 판단은 IMonsterState 구현체(Idle/Chase/Attack)가 직접 담당하며,
/// 이 클래스는 상태 인스턴스 보관과 Enter/Exit 전환 처리, Tick 위임만 담당
/// </summary>
public class MonsterFSM : MonoBehaviour
{
    private MonsterController _controller;

    private IMonsterState _current;
    public IMonsterState Current => _current;

    private readonly MonsterIdleState   _idleState   = new MonsterIdleState();
    private readonly MonsterChaseState  _chaseState  = new MonsterChaseState();
    private readonly MonsterAttackState _attackState = new MonsterAttackState();

    public IMonsterState IdleState   => _idleState;
    public IMonsterState ChaseState  => _chaseState;
    public IMonsterState AttackState => _attackState;

    public event Action<IMonsterState> OnStateChanged;

    private void Awake()
    {
        _controller = GetComponent<MonsterController>();
        _current    = _idleState;
    }

    /// <summary>
    /// 현재 상태를 Tick, 그 안에서 상태 전환이 일어나면 같은 프레임에 이어서 새 상태까지 Tick
    /// </summary>
    public void Tick()
    {
        IMonsterState before;
        int safety = 0; // 무한 루프 방지용

        do
        {
            before = _current; // 최신 상태 저장
            _current?.Tick(_controller); // 현재 상태의 행동 실행
            safety++; // 실행횟수 +1
        }
        while (_current != before && safety < 3); // 방금 실행하는 동안 상태가 바뀌었고, 실행 횟수가 3미만이면 반복
    }

    /// <summary>
    /// 상태 전환 메소드, 이전 상태의 Exit와 새 상태의 Enter를 호출하고 변경 이벤트를 발행
    /// </summary>
    public void ChangeState(IMonsterState next)
    {
        if (_current == next) return;

        _current?.Exit(_controller);
        _current = next;
        _current?.Enter(_controller);

        OnStateChanged?.Invoke(_current);
    }

    /// <summary>
    /// 오브젝트 풀에서 재사용될 때 상태를 Idle로 초기화하는 메소드 (Enter/이벤트 발행 없이 즉시 초기화)
    /// </summary>
    public void ResetState()
    {
        _current = _idleState;
    }
}
