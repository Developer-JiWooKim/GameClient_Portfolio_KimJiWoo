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

    public void PlayWalk()
    {
        if(_animator == null)
        {
            Debug.LogError("Animator is null");
            return;
        }

        _animator.SetBool(IsMovingHash, true);
        _animator.SetBool(IsRunningHash, false);
        _animator.SetBool(IsAttackingHash, false);
    }

    public void PlayRun()
    {
        if (_animator == null)
        {
            Debug.LogError("Animator is null");
            return;
        }

        _animator.SetBool(IsMovingHash, true);
        _animator.SetBool(IsRunningHash, true);
        _animator.SetBool(IsAttackingHash, false);
    }

    public void PlayIdle()
    {
        if (_animator == null)
        {
            Debug.LogError("Animator is null");
            return;
        }

        _animator.SetBool(IsMovingHash, false);
        _animator.SetBool(IsRunningHash, false);
        _animator.SetBool(IsAttackingHash, false);
    }

    public void PlayAttack()
    {
        if (_animator == null)
        {
            Debug.LogError("Animator is null");
            return;
        }

        _animator.SetBool(IsMovingHash, false);
        _animator.SetBool(IsRunningHash, false);
        _animator.SetBool(IsAttackingHash, true);
    }
}
