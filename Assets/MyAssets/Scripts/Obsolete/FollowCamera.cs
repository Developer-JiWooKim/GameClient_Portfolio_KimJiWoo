using UnityEngine;

namespace Assets.MyAssets.Scripts.Obsolete
{
    /// <summary>
    /// [미사용 - 이전 Algorithm_Portfolio_KimJiWoo에서 직접 구현한 플레이어를 따라다니는 카메라 제어 컴포넌트(SmoothDamp 기반)]
    /// 현재 프로젝트에서는 Cinemachine으로 대체
    /// </summary>
    [System.Obsolete("Cinemachine(CinemachineCamera + CinemachineBrain)으로 대체됨. 더 이상 사용하지 않음")]
    public class FollowCamera : MonoBehaviour
    {
        [SerializeField] private float _smoothTime = 0.3f; // 높을수록 더 느리게 따라감

        [Header("Quarter View Settings")]
        [SerializeField] private float _height = 20f;  // 높이 (Y)
        [SerializeField] private float _distanceBack = 18f;  // 뒤로 빠지는 거리 (Z)
        [SerializeField] private float _pitchAngle = 45f;  // X축 회전 각도

        private Vector3 _velocity = Vector3.zero;
        private Vector3 _cameraOffset;
        private Transform _target;

        public Transform Target
        {
            get => _target;
            set => _target = value;
        }

        private void Awake() => Initiailize();
        private void Initiailize()
        {
            _cameraOffset = new Vector3(0, _height, -_distanceBack);
            transform.rotation = Quaternion.Euler(_pitchAngle, 0f, 0f);
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            SmoothFollowing();
        }

        private void SmoothFollowing()
        {
            Vector3 targetPosition = _target.position + _cameraOffset;

            transform.position = Vector3.SmoothDamp(
                transform.position,
                targetPosition,
                ref _velocity,
                _smoothTime
            );
        }
    }
}
