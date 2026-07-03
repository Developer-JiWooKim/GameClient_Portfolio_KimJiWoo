using UnityEngine;

namespace Assets.MyAssets.Scripts.Utility
{
    /// <summary>
    /// Light(Point)의 Intensity를 사인파 기반으로 강하게 펄스시키는 컴포넌트
    /// </summary>
    [RequireComponent(typeof(Light))]
    public class PulsingLight : MonoBehaviour
    {
        [Header("Pulse Speed")]
        [SerializeField] private float _pulseSpeed = 2f;

        [Header("Brightness Range")]
        [SerializeField] private float _minIntensity = 3f;
        [SerializeField] private float _maxIntensity = 10f;

        private Light _light;
        private float _pulseTimer;

        private void Awake()
        {
            _light = GetComponent<Light>();
        }

        private void OnEnable()
        {
            // 여러 개가 동시에 시작해도 똑같이 펄스하지 않도록 시작 타이밍을 랜덤하게 어긋나게
            _pulseTimer = Random.Range(0f, Mathf.PI * 2f);
        }

        private void Update()
        {
            _pulseTimer += Time.deltaTime * _pulseSpeed;

            // 0~1 사이를 왔다갔다하는 펄스 값 (사인파 기반)
            float pulse = (Mathf.Sin(_pulseTimer) + 1f) * 0.5f;

            _light.intensity = Mathf.Lerp(_minIntensity, _maxIntensity, pulse);
        }
    }
}
