using UnityEngine;

/// <summary>
/// 플레이어 얼굴 표정(블렌드셰이프)을 제어하는 컴포넌트
/// 몸 애니메이션(Idle/Move)과는 독립적으로 작동함 - 평상시엔 Fsad 고정 + Fhide(눈 깜빡임) 주기적 반복
/// 몬스터에게 발각되면 즉시 Fdam으로 전환
/// </summary>
public class PlayerFaceController : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer _faceRenderer;

    [Header("Face")]
    [SerializeField] private string _fsadName  = "Fsad";
    [SerializeField] private string _fhideName = "Fhide";
    [SerializeField] private string _fdamName  = "Fdam";

    [Header("Blink Cycle")]
    [SerializeField] private float _blinkIntervalMin = 2f;
    [SerializeField] private float _blinkIntervalMax = 3f;
    [SerializeField] private float _blinkDuration    = 0.15f;

    private int _fsadIndex;
    private int _fhideIndex;
    private int _fdamIndex;

    private bool _isCaught; // 몬스터에게 발각 여부

    private float _blinkTimer;
    private float _nextBlinkInterval;

    private void Awake()
    {
        _fsadIndex  = _faceRenderer.sharedMesh.GetBlendShapeIndex(_fsadName);
        _fhideIndex = _faceRenderer.sharedMesh.GetBlendShapeIndex(_fhideName);
        _fdamIndex  = _faceRenderer.sharedMesh.GetBlendShapeIndex(_fdamName);
    }

    private void OnEnable()
    {
        SetNormalExpression();
    }

    private void Update()
    {
        if (_isCaught) return; // 발각 상태면 깜빡임 로직 자체를 멈춤 (Fdam 고정 유지)

        startBlinking();        
    }

    /// <summary>
    /// 눈 깜빡임(Fhide 0 - 100) 메소드
    /// </summary>
    private void startBlinking()
    {
        _blinkTimer += Time.deltaTime;

        if (_blinkTimer < _nextBlinkInterval) return; // 아직 깜빡일 타이밍 아님

        float blinkProgress = (_blinkTimer - _nextBlinkInterval) / _blinkDuration;

        if (blinkProgress >= 1f)
        {
            // 깜빡임 한 번 끝남 - 다음 깜빡임까지 다시 타이머/간격 리셋
            _faceRenderer.SetBlendShapeWeight(_fhideIndex, 0f);
            _blinkTimer = 0f;
            _nextBlinkInterval = Random.Range(_blinkIntervalMin, _blinkIntervalMax);
            return;
        }

        float weight = Mathf.Sin(blinkProgress * Mathf.PI) * 100f;

        _faceRenderer.SetBlendShapeWeight(_fhideIndex, weight);
    }


    /// <summary>
    /// 몬스터에게 발각됐을 때 호출
    /// </summary>
    public void SetCaughtExpression()
    {
        _isCaught = true;

        _faceRenderer.SetBlendShapeWeight(_fsadIndex, 0f);
        _faceRenderer.SetBlendShapeWeight(_fhideIndex, 0f);
        _faceRenderer.SetBlendShapeWeight(_fdamIndex, 100f);
    }

    /// <summary>
    /// 발각 상태에서 벗어났을 때(또는 처음 시작할 때) 호출 - 평상시 표정으로 복귀
    /// </summary>
    public void SetNormalExpression()
    {
        _isCaught = false;

        _faceRenderer.SetBlendShapeWeight(_fsadIndex, 100f);
        _faceRenderer.SetBlendShapeWeight(_fdamIndex, 0f);

        _blinkTimer = 0f;
        _nextBlinkInterval = Random.Range(_blinkIntervalMin, _blinkIntervalMax);
    }
}
