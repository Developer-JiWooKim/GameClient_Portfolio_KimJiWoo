using Assets.MyAssets.Scripts.Utility.Core;
using Unity.Cinemachine;
using UnityEngine;

namespace Assets.MyAssets.Scripts.Utility.Visuals
{
    /// <summary>
    /// 플레이어 스폰 시 인트로 카메라를 보여준 뒤 팔로우 카메라로 부드럽게 전환하는 컴포넌트
    /// 스폰 직후 CutToIntroCamera()로 즉시 컷하고, 플레이어 인트로 애니메이션이 끝난 뒤 SwitchToFollowCamera()를 호출해 전환함
    /// </summary>
    public class IntroCameraSequencer : MonoBehaviour
    {
        [Header("Cinemachine Cameras")]
        [SerializeField] private CinemachineCamera _followPlayerCamera;
        [SerializeField] private CinemachineCamera _introCamera;

        [Header("Timing")]
        [SerializeField] private float _introHoldDuration = 0.5f; // 재시작 시 안정적인 카메라 전환을 위해 기다릴 시간
        [SerializeField] private float _followBlendDuration = 2f; // 팔로우 카메라로 블렌드되는 데 걸리는 시간 - CinemachineBrain의 Default Blend Time과 맞춰야 함

        private CinemachineBrain _cinemachineBrain;

        private void Awake()
        {
            if (Camera.main != null)
            {
                _cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
            }

            if (_cinemachineBrain != null)
            {
                // 게임 시작 연출 대기 중 Time.timeScale이 0으로 멈춰있어도 카메라 블렌드가 실제로 진행되게
                _cinemachineBrain.IgnoreTimeScale = true;
            }
        }

        /// <summary>
        /// 인트로 카메라로 즉시 컷하는 메소드 - 팔로우 카메라로의 전환은 SwitchToFollowCamera()가 별도로 처리
        /// 플레이어 인트로 애니메이션이 끝난 뒤 호출되도록
        /// </summary>
        public void CutToIntroCamera(Transform target)
        {
            // 재시작(Replay) 시 이전 판에서 올려둔 팔로우 카메라 우선순위가 남아있으면 인트로 카메라가 안 보이므로,
            // 매번 인트로보다 낮은 값으로 되돌려둔 뒤 SwitchToFollowCamera()에서 다시 올림
            _followPlayerCamera.Priority = _introCamera.Priority - 1;

            // Replay 시 브레인에 이전 판의 카메라 상태가 남아있으면, 인트로 카메라로 우선순위가 바뀌는 순간
            // 이전 위치 -> 인트로 로 블렌드가 걸려 인트로로 움직이려다 바로 팔로우로 되돌아가는 것처럼 보이는 현상 방지
            _cinemachineBrain?.ResetState(); // ResetState()로 즉시 인트로 카메라를 선택

            _followPlayerCamera.Target.TrackingTarget = target;
        }

        /// <summary>
        /// 추적 카메라의 우선순위를 올려 팔로우 카메라로 전환시키고, 블렌드가 끝날 때까지 기다리는 메소드
        /// 플레이어 인트로 애니메이션이 끝난 뒤 호출됨
        /// </summary>
        public async Awaitable SwitchToFollowCamera()
        {
            // 게임 시작 연출 대기 중에는 Time.timeScale이 0으로 멈춰있을 수 있으므로, unscaled time으로 대기
            await AwaitableUtil.WaitUnscaled(_introHoldDuration, destroyCancellationToken);

            _followPlayerCamera.Priority = _introCamera.Priority + 1;

            // CinemachineBrain.IsBlending 폴링은 프레임 타이밍에 따라 실제 블렌드보다 먼저 끝난 것처럼
            // 보일 수 있어 신뢰할 수 없으므로, _followBlendDuration만큼 고정된 시간을 그대로 기다림
            // (CinemachineBrain의 Default Blend Time 설정과 값을 맞춰야 함)
            await AwaitableUtil.WaitUnscaled(_followBlendDuration, destroyCancellationToken);
        }
    }
}
