using UnityEngine;
using Assets.MyAssets.Scripts.Utility.SingleTon;

namespace Assets.MyAssets.Scripts.Monster
{
    public class MonsterSight : MonoBehaviour
    {
        [Header("Setting")]
        [SerializeField] private float _detectionRange = 15f;   // 감지 반경
        [SerializeField] private float _fieldOfView = 90f;   // 전체 시야각

        [Header("Sense Check")]
        [SerializeField] private bool _isSense = false; // 감지 여부, 현재 몬스터가 플레이어를 감지했는지 에디터에서 확인용

        public float DetectionRange => _detectionRange;

        public float FieldOfView => _fieldOfView;

        // FOV 절반 각도의 cos값을 미리 계산해둠 - 매 프레임 Acos로 각도를 구하는 대신
        // dot(cosθ)을 이 값과 직접 비교해서 삼각함수 호출을 없앰
        private float _cosHalfFieldOfView;

        private void Awake()
        {
            _cosHalfFieldOfView = Mathf.Cos(_fieldOfView * 0.5f * Mathf.Deg2Rad);
        }

        /// <summary>
        /// 타겟이 시야각 안에 들어와 있고 타겟과 자신 사이에 벽이 있는지 검사하는 메소드
        /// </summary>
        public bool TargetSense(Vector3 targetPos)
        {
            // 하드 모드일 때는 항상 감지 성공
            if (GameManager.IsHardArcaneMode())
            {
                return _isSense = true;
            }

            Vector3 myPos = transform.position;

            Vector3 dirToPlayer = targetPos - myPos;
            dirToPlayer.y = 0;

            // 정규화 작업(normalized)
            float distance = dirToPlayer.magnitude; // 타겟과 자신 사이의 거리
            dirToPlayer /= distance;                // (타겟 방향 벡터 / 이 벡터의 길이(거리)) 로 정규화

            float dot = Vector3.Dot(transform.forward, dirToPlayer); // 내적으로 현재 forward와 타겟 방향의 cosθ 계산

            // dot(cosθ)이 cos(FOV/2)보다 작다는 건 θ가 FOV/2보다 크다는 뜻, 즉 타겟이 시야각 안에 들어오지 않음
            if (dot <= _cosHalfFieldOfView)
            {
                return _isSense = false;
            }

            // 내 위치 기준 바닥에서 0.5f 위 지점
            Vector3 origin = myPos + Vector3.up * 0.5f;

            if (MazeLayerManager.Instance == null)
            {
                return _isSense = false;
            }

            // 시야각 안에 있어도 Ray를 쐈을 때 현재 활성화된 레이어의 벽이 타겟과 자신 사이에 있으면 감지 실패
            if (Physics.Raycast(origin, dirToPlayer, distance, MazeLayerManager.Instance.CurrentWallLayerMask))
            {
                return _isSense = false;
            }

            return _isSense = true;
        }

        /// <summary>
        /// 감지 반경 안에 들어왔는지 검사하는 메소드
        /// </summary>
        public bool IsInRange(Vector3 targetPos)
        {
            if (GameManager.IsHardArcaneMode())
            {
                return true;
            }

            Vector3 dir = targetPos - transform.position;
            dir.y = 0f;

            return dir.sqrMagnitude <= _detectionRange * _detectionRange;
        }

        /// <summary>
        /// 에디터 확인용 기즈모(감지 범위)
        /// </summary>
        private void OnDrawGizmos()
        {
            Vector3 myPos = transform.position;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(myPos, _detectionRange);
        }
    }
}
