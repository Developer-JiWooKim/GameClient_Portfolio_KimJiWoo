using UnityEngine;

namespace Assets.MyAssets.Scripts.Monster
{
    public class MonsterAnim : MonoBehaviour
    {
        private Animator _animator;

        private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
        private static readonly int IsRunningHash = Animator.StringToHash("IsRunning");
        private static readonly int IsAttackingHash = Animator.StringToHash("IsAttacking");

        // 매 Tick마다 같은 상태가 반복 호출돼도 SetBool을 다시 쏘지 않도록 마지막 값 캐싱
        private bool _lastMoving;
        private bool _lastRunning;
        private bool _lastAttacking;
        private bool _hasSetStateOnce;

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
        }

        public void PlayWalk() => SetState(moving: true, running: false, attacking: false);
        public void PlayRun() => SetState(moving: true, running: true, attacking: false);
        public void PlayIdle() => SetState(moving: false, running: false, attacking: false);
        public void PlayAttack() => SetState(moving: false, running: false, attacking: true);

        private void SetState(bool moving, bool running, bool attacking)
        {
            // TODO#: 이 부분 질문, 유니티에서 null 체크는 가짜 널(Fake Null)을 검사하기에 연산을 많이 잡아먹는데, 
            // 이를 Tick안에서 매번 검사하면 꽤 무거울거로 예상됨 -> Awake에서 한번만 검사하고 bool에 저장 후 매 프레임에서 불값만 비교하는게 안전한가?
            if (_animator == null)
            {
                Debug.LogError("Animator is null");
                return;
            }

            // 상태 클래스가 Tick()마다 같은 Play*()를 계속 호출하므로, 값이 그대로면 SetBool 3회를 그냥 건너뜀
            if (_hasSetStateOnce && moving == _lastMoving && running == _lastRunning && attacking == _lastAttacking)
            {
                return;
            }

            _animator.SetBool(IsMovingHash, moving);
            _animator.SetBool(IsRunningHash, running);
            _animator.SetBool(IsAttackingHash, attacking);

            _lastMoving = moving;
            _lastRunning = running;
            _lastAttacking = attacking;
            _hasSetStateOnce = true;
        }
    }
}
