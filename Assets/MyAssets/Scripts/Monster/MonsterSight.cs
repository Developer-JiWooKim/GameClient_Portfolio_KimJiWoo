using UnityEngine;

public class MonsterSight : MonoBehaviour
{
    [SerializeField] private float _detectionRange = 15f;   // 감지 반경
    [SerializeField] private float _fieldOfView    = 90f;   // 전체 시야각
    [SerializeField] private bool  _isSense        = false; // 감지 여부

    public float DetectionRange => _detectionRange;

    public float FieldOfView => _fieldOfView;

    /// <summary>
    /// 타겟이 시야각 안에 들어와 있고 타겟과 자신 사이에 벽이 있는지 검사하는 메소드 
    /// </summary>
    public bool TargetSense(Vector3 targetPos)
    {
        Vector3 myPos = transform.position;

        Vector3 dirToPlayer = targetPos - myPos; 
        dirToPlayer.y = 0;

        // 정규화 작업(normalized)
        float distance = dirToPlayer.magnitude; // 타겟과 자신 사이의 거리
        dirToPlayer /= distance;                // (타겟 방향 벡터 / 이 벡터의 길이(거리)) 로 정규화

        // 내적으로 현재 바라보는 앞 방향(forward)과 타겟 방향의 사잇각(cosθ)을 계산(cosθ 값에 해당하는 float 값)
        float dot = Vector3.Dot(transform.forward, dirToPlayer);

        dot = Mathf.Clamp(dot, -1, 1); // 내적값이 -1 ~ 1을 초과하지 못하게 방어

        // 위에서 구한 dot(float 값)을 라디안 변환(θ 각도) -> Mathf.Rad2Deg 곱해서 일반 각도로 변환
        float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;

        // _fieldOfView은 양측 전체 시야각이므로 절반과 비교
        if (angle >= _fieldOfView * 0.5f)
        {
            return _isSense = false;
        }

        // 내 위치 기준 바닥에서 0.5f 위 지점
        Vector3 origin = myPos + Vector3.up * 0.5f;

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
        Vector3 dir = targetPos - transform.position;
        dir.y = 0f;

        return dir.sqrMagnitude <= _detectionRange * _detectionRange;
    }

    /// <summary>
    /// 에디터 확인용 기즈모(감지 범위, 시야각)
    /// </summary>
    private void OnDrawGizmos()
    {
        Vector3 myPos = transform.position;

        // 감지 반경
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(myPos, _detectionRange);

        // 시야각 경계선 (회전 행렬로 좌/우 벡터 계산)
        Vector3 fwd = transform.forward;
        float halfRad = _fieldOfView * 0.5f * Mathf.Deg2Rad;

        // 시야각 기준 왼쪽 경계
        Vector3 leftDir = new Vector3(
             fwd.x * Mathf.Cos(-halfRad) - fwd.z * Mathf.Sin(-halfRad), 0,
             fwd.x * Mathf.Sin(-halfRad) + fwd.z * Mathf.Cos(-halfRad));

        // 시야각 기준 오른쪽 경계
        Vector3 rightDir = new Vector3(
             fwd.x * Mathf.Cos(halfRad) - fwd.z * Mathf.Sin(halfRad), 0,
             fwd.x * Mathf.Sin(halfRad) + fwd.z * Mathf.Cos(halfRad));

        Gizmos.color = _isSense ? Color.red : Color.yellow;
        Gizmos.DrawRay(myPos, leftDir * _detectionRange);
        Gizmos.DrawRay(myPos, rightDir * _detectionRange);
    }
}
