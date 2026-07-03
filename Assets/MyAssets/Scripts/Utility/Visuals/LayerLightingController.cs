using UnityEngine;
using Assets.MyAssets.Scripts.Utility.SingleTon;

namespace Assets.MyAssets.Scripts.Utility.Visuals
{
    /// <summary>
    /// 레이어 전환에 맞춰 Directional Light 색/밝기와 대기 Fog 색을 함께 바꿔주는 컴포넌트
    /// </summary>
    public class LayerLightingController : MonoBehaviour
    {
        [Header("Directional Light")]
        [SerializeField] private Light _directionalLight;

        [Header("Physical")]
        [SerializeField] private Color _physicalLightColor = Color.white;
        [SerializeField] private float _physicalLightIntensity = 1f;
        [SerializeField] private Color _physicalFogColor = new Color(0.08f, 0.08f, 0.1f);

        [Header("Arcane")]
        [SerializeField] private Color _arcaneLightColor = new Color(0.65f, 0.35f, 0.95f);
        [SerializeField] private float _arcaneLightIntensity = 0.6f;
        [SerializeField] private Color _arcaneFogColor = new Color(0.22f, 0.04f, 0.3f);

        private void Start()
        {
            if (MazeLayerManager.Instance != null)
            {
                MazeLayerManager.Instance.OnLayerChanged += ApplyLayer;

                // 시작은 항상 Physical이므로 그 기준으로 한 번 맞춰둠
                ApplyLayer(MazeLayerManager.Instance.CurrentLayer);
            }
        }

        private void OnDisable()
        {
            if (MazeLayerManager.Instance != null)
            {
                MazeLayerManager.Instance.OnLayerChanged -= ApplyLayer;
            }
        }

        private void ApplyLayer(MazeLayerManager.LayerType layer)
        {
            bool isPhysical = layer == MazeLayerManager.LayerType.Physical;

            if(_directionalLight != null)
            {
                _directionalLight.color = isPhysical ? _physicalLightColor : _arcaneLightColor;
                _directionalLight.intensity = isPhysical ? _physicalLightIntensity : _arcaneLightIntensity;
            }

            RenderSettings.fogColor = isPhysical ? _physicalFogColor : _arcaneFogColor;
        }
    }
}
