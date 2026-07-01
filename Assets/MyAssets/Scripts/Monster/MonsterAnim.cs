using UnityEngine;

public class MonsterAnim : MonoBehaviour
{
    private Animator _animator;

    private static readonly int IsMovingHash    = Animator.StringToHash("IsMoving");
    private static readonly int IsRunningHash   = Animator.StringToHash("IsRunning");
    private static readonly int IsAttackingHash = Animator.StringToHash("IsAttacking");

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
    }

    public void PlayWalk()   => SetState(moving: true,  running: false, attacking: false);
    public void PlayRun()    => SetState(moving: true,  running: true,  attacking: false);
    public void PlayIdle()   => SetState(moving: false, running: false, attacking: false);
    public void PlayAttack() => SetState(moving: false, running: false, attacking: true);

    private void SetState(bool moving, bool running, bool attacking)
    {
        if (_animator == null)
        {
            Debug.LogError("Animator is null");
            return;
        }

        _animator.SetBool(IsMovingHash, moving);
        _animator.SetBool(IsRunningHash, running);
        _animator.SetBool(IsAttackingHash, attacking);
    }
}
