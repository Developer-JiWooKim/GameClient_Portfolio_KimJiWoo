using UnityEngine;

public class MonsterFSM : MonoBehaviour
{
    public enum State
    {
        Idle,       
        Chase,      
        Attack,
    }

    [SerializeField] private State _current;

    public State Current => _current;

    public event System.Action<State> OnStateChanged;

    /// <summary>
    /// isSensed(시야각, 장애물 있는지 여부), isInRange(감지 범위 안에 들어와있는지 여부) 에 따라 현재 상태 결정하는 메소드
    /// </summary>
    public void Evaluate(bool isSensed, bool isInRange)
    {
        switch (Current)
        {     
            // 현재 상태가 Idle인데 타겟을 감지했으면 Chase로 상태 변경
            case State.Idle:
                if (isSensed) TransitionTo(State.Chase);
                break;

            // 현재 상태가 Chase인데 타겟이 감지 반경을 벗어나면 Idle로 상태 변경
            case State.Chase:
                if (!isInRange) TransitionTo(State.Idle);
                break;
        }
    }

    /// <summary>
    /// 공격이 가능하면 상태를 Attack, 그렇지 않으면 Chase로 변경하는 메소드
    /// </summary>
    public void SetAttack(bool isAttacking)
    {
        if (isAttacking)
        {
            TransitionTo(State.Attack);
        }
        else
        {
            TransitionTo(State.Chase);
        }
    }

    /// <summary>
    /// 상태 변경 메소드
    /// </summary>
    private void TransitionTo(State next)
    {
        if (_current == next) return;

        OnStateChanged?.Invoke(next);

        _current = next;
    }
}
