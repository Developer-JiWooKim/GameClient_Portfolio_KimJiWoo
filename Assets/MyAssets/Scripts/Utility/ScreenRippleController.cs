using UnityEngine;

/// <summary>
/// 풀스크린 일렁임(Ripple) 머티리얼의 _Intensity 값을 런타임에 조절하는 컨트롤러
/// </summary>
public class ScreenRippleController : MonoBehaviour
{
    [SerializeField] private Material _rippleMat;

    private static readonly int IntensityID = Shader.PropertyToID("_Intensity");

    // _rippleMat은 프로젝트에 저장된 공유 머티리얼 에셋이라, 전환이 중간에 끊겨 0이 아닌 값이 남으면
    // 에디터에서 그 값이 그대로 잔존/저장될 수 있음. 시작/종료 시점에 항상 0으로 되돌려 초기 상태를 보장함
    private void Awake() => SetIntensity(0f);
    private void OnDisable() => SetIntensity(0f);
    private void OnDestroy() => SetIntensity(0f);

    public void SetIntensity(float intensity)
    {
        if (_rippleMat == null) return;

        _rippleMat.SetFloat(IntensityID, intensity);
    }
}
