using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// 플레이어 스폰 시 인트로 카메라를 잠깐 보여준 뒤 팔로우 카메라로 부드럽게 전환하는 컴포넌트.
/// UnitSpawner가 플레이어를 스폰한 직후 PlayIntro() 한 번만 호출하면 이후 시퀀스는 알아서 진행됨
/// </summary>
public class IntroCameraSequencer : MonoBehaviour
{
    [Header("Cinemachine Cameras")]
    [SerializeField] private CinemachineCamera _followPlayerCamera;
    [SerializeField] private CinemachineCamera _introCamera;

    [Header("Timing")]
    [SerializeField] private float _introHoldDuration = 0.5f; // 재시작 시 안정적인 카메라 전환을 위해 기다릴 시간

    private CinemachineBrain _cinemachineBrain;

    private void Awake()
    {
        if (Camera.main != null)
        {
            _cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
        }
    }

    /// <summary>
    /// 인트로 카메라로 즉시 컷한 뒤, 일정 시간 후 팔로우 카메라로 블렌드 전환하는 시퀀스를 시작하는 메소드
    /// </summary>
    public void PlayIntro(Transform target)
    {
        // 재시작(Replay) 시 이전 판에서 올려둔 팔로우 카메라 우선순위가 남아있으면 인트로 카메라가 안 보이므로,
        // 매번 인트로보다 낮은 값으로 되돌려둔 뒤 SwitchToFollowCameraAfterDelay()에서 다시 올림
        _followPlayerCamera.Priority = _introCamera.Priority - 1;

        // Replay 시 브레인에 이전 판의 카메라 상태가 남아있으면, 인트로 카메라로 우선순위가 바뀌는 순간
        // "이전 위치 -> 인트로"로 블렌드가 걸려 인트로로 움직이려다 바로 팔로우로 되돌아가는 것처럼 보임.
        // ResetState()로 블렌딩 없이 즉시 인트로 카메라를 선택시켜, 첫 시작 때처럼 인트로 컷 -> 팔로우 블렌드만 보이게 함
        _cinemachineBrain?.ResetState();

        _followPlayerCamera.Target.TrackingTarget = target;

        // 즉시 Priority를 높이면 블렌딩이 생략되는 현상 생김
        // -> Awaitable.WaitForSecondsAsync()로 _introHoldDuration 만큼 기다린 후 _followPlayerCamera의 Priority를 높임
        _ = SwitchToFollowCameraAfterDelay();
    }

    /// <summary>
    /// 인트로 카메라를 일정 시간 동안 무조건 보여준 뒤, 추적 카메라의 우선순위를 올려 카메라 전환시키는 메소드
    /// </summary>
    private async Awaitable SwitchToFollowCameraAfterDelay()
    {
        try
        {
            await Awaitable.WaitForSecondsAsync(_introHoldDuration, destroyCancellationToken);

            _followPlayerCamera.Priority = _introCamera.Priority + 1;
        }
        catch (System.OperationCanceledException oce)
        {
            Debug.LogException(oce);
        }
    }
}
