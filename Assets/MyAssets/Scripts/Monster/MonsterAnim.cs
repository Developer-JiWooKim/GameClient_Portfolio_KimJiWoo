using UnityEngine;

namespace Assets.MyAssets.Scripts.Monster
{
    public class MonsterAnim : MonoBehaviour
    {
        private Animator _animator;

        // Animator의 파라미터 ID 값을 미리 캐싱
        private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
        private static readonly int IsRunningHash = Animator.StringToHash("IsRunning");
        private static readonly int IsAttackingHash = Animator.StringToHash("IsAttacking");

        // 매 Tick마다 같은 상태가 반복 호출돼도 SetBool을 다시 쏘지 않도록 마지막 값 캐싱
        private bool _lastMoving;
        private bool _lastRunning;
        private bool _lastAttacking;
        private bool _hasSetStateOnce;
        private bool _isSetAnimator = false; // 애니메이터 컴포넌트 Null 체크용 bool

        private void Awake()
        {
            _isSetAnimator = _animator = GetComponentInChildren<Animator>();
        }

        // State Helper
        public void PlayWalk() => SetState(moving: true, running: false, attacking: false);
        public void PlayRun() => SetState(moving: true, running: true, attacking: false);
        public void PlayIdle() => SetState(moving: false, running: false, attacking: false);
        public void PlayAttack() => SetState(moving: false, running: false, attacking: true);

        private void SetState(bool moving, bool running, bool attacking)
        {
            // Animator 컴포넌트 Null 체크
            if (!_isSetAnimator)
            {
                Debug.LogError("MonsterAnim - SetState(): Animator is null");
                return;
            }

            // 상태 클래스가 Tick()마다 같은 Play*()를 계속 호출하므로, 값이 그대로면 SetBool 3회를 그냥 건너뜀
            if (_hasSetStateOnce && moving == _lastMoving && running == _lastRunning && attacking == _lastAttacking)
            {
                return;
            }

            // 상황에 맞는 애니메이션 실행
            _animator.SetBool(IsMovingHash, moving);
            _animator.SetBool(IsRunningHash, running);
            _animator.SetBool(IsAttackingHash, attacking);

            _lastMoving = moving;
            _lastRunning = running;
            _lastAttacking = attacking;
            _hasSetStateOnce = true;
        }

        void OnValidate()
        {
            _isSetAnimator = _animator = GetComponentInChildren<Animator>();
        }
    }
}
