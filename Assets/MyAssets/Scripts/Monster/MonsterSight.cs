using UnityEngine;

public class MonsterSight : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private float _detectionRange = 15f;   // 감지 반경
    [SerializeField] private float _fieldOfView    = 90f;   // 전체 시야각

    [Header("Sense Check")]
    [SerializeField] private bool _isSense = false; // 감지 여부, 에디터에서 확인용

    public float DetectionRange => _detectionRange;

    public float FieldOfView => _fieldOfView;

    /// <summary>
    /// 타겟이 시야각 안에 들어와 있고 타겟과 자신 사이에 벽이 있는지 검사하는 메소드 
    /// </summary>
    public bool TargetSense(Vector3 targetPos)
    {
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
        dot = Mathf.Clamp(dot, -1, 1); // 내적값이 -1 ~ 1을 초과하지 못하게 방어        
        float angle = Mathf.Acos(dot) * Mathf.Rad2Deg; // 위에서 구한 dot(float 값)을 라디안 변환(θ 각도)

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
