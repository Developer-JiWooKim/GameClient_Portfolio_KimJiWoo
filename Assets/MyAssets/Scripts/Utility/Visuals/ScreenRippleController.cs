using UnityEngine;

namespace Assets.MyAssets.Scripts.Utility.Visuals
{
    /// <summary>
    /// 풀스크린 일렁임(Ripple) 머티리얼의 _Intensity 값을 런타임에 조절하는 컨트롤러
    /// </summary>
    public class ScreenRippleController : MonoBehaviour
    {
        [SerializeField] private Material _rippleMat;

        private static readonly int IntensityID = Shader.PropertyToID("_Intensity");

        private void Awake() => SetIntensity(0f);
        private void OnDisable() => SetIntensity(0f);
        private void OnDestroy() => SetIntensity(0f);

        public void SetIntensity(float intensity)
        {
            if (_rippleMat == null) return;

            _rippleMat.SetFloat(IntensityID, intensity);
        }
    }
}
