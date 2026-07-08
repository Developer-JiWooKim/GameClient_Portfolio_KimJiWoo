using UnityEngine;

namespace Assets.MyAssets.Scripts.Player
{
    public class PlayerAnim : MonoBehaviour
    {
        private Animator _animator;

        private readonly int IsMovingHash = Animator.StringToHash("IsMoving");      // Parameter
        private readonly int PlayIntroHash = Animator.StringToHash("PlayIntro");    // Parameter
        private readonly int IntroStateHash = Animator.StringToHash("LookAround");  // State

        private bool _isSetAnimator = false; // 애니메이터 컴포넌트 Null 체크용 bool

        private void Awake()
        {
            _isSetAnimator = _animator = GetComponentInChildren<Animator>();
            if (!_isSetAnimator)
            {
                Debug.LogError("_animator is null");
            }
            else
            {
                _animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            }
        }

        public void SetMoving(bool isMoving)
        {
            _animator.SetBool(IsMovingHash, isMoving);
        }

        /// <summary>
        /// Intro 상태 애니메이션 재생을 트리거하고, Animator가 실제로 Intro 상태에 진입했다가 벗어날 때까지(재생 완료) 대기하는 메소드 
        /// </summary>
        public async Awaitable PlayIntroAsync()
        {
            _animator.SetTrigger(PlayIntroHash);

            // AnyState -> Intro 전환이 끝나 실제로 Intro 상태에 진입할 때까지 대기  
            while (_animator.IsInTransition(0) || _animator.GetCurrentAnimatorStateInfo(0).shortNameHash != IntroStateHash)
            {
                await Awaitable.NextFrameAsync(destroyCancellationToken);
            }

            // Intro 상태를 벗어나기 시작할 때까지(=재생이 끝나 Idle로 전환 시작) 대기 
            while (!_animator.IsInTransition(0) && _animator.GetCurrentAnimatorStateInfo(0).shortNameHash == IntroStateHash)
            {
                await Awaitable.NextFrameAsync(destroyCancellationToken);
            }
        }
    }
}
