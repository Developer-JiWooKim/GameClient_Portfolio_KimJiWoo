using System.Threading;
using Assets.MyAssets.Scripts.Utility.Core;
using Assets.MyAssets.Scripts.Utility.SingleTon;
using UnityEngine;

namespace Assets.MyAssets.Scripts.Utility.Visuals
{
    /// <summary>
    /// Physical 벽에 30초 주기로 그림(오염) 무늬가 누적되는 연출을 담당하는 컴포넌트
    /// Physical 벽은 모두 동일한 _physicalGrimeMaterial(공유 머테리얼)을 참조하므로, 이 머테리얼 하나만 값을 바꿔도
    /// Physical 레이어의 모든 벽에 한 번에 반영됨
    /// </summary>
    public class PhysicalWallGrimeCycler : MonoBehaviour
    {
        [Header("Physical Wall Grime")]
        [SerializeField] private Material _physicalGrimeMaterial; // Wall_Physical의 유일한 머테리얼 슬롯과 동일한 에셋(공유 머테리얼)을 참조해야 함
        [SerializeField] private float _grimeCycleInterval = 30f; // 다음 단계로 넘어가기까지 대기하는 시간(초)
        [SerializeField] private float _grimeStep = 0.25f; // 한 사이클마다 늘어나는 블렌드 양(0~1)
        [SerializeField] private float _grimeFadeDuration = 3f; // 한 사이클 내에서 블렌드가 부드럽게 올라가는 시간(초)

        private static readonly int GrimeBlendPropertyId = Shader.PropertyToID("_GrimeBlend");

        private CancellationTokenSource _grimeCts;

        private void OnDestroy()
        {
            _grimeCts?.Cancel();
            _grimeCts?.Dispose();
        }

        /// <summary>
        /// 새 판이 시작될 때 그림(오염) 블렌드를 0으로 되돌리고 30초 주기 누적 사이클을 새로 시작
        /// </summary>
        public void RestartCycle()
        {
            _grimeCts?.Cancel();
            _grimeCts?.Dispose();
            _grimeCts = null;

            if (_physicalGrimeMaterial == null) return;

            _physicalGrimeMaterial.SetFloat(GrimeBlendPropertyId, 0f);

            _grimeCts = new CancellationTokenSource();
            _ = RunCycle(_grimeCts.Token);

            GameManager.Instance.GameRule.OnClear += StopCycle;
            GameManager.Instance.GameRule.OnGameOver += StopCycle;
        }

        private void StopCycle()
        {
            _grimeCts?.Cancel();
            _grimeCts?.Dispose();
            _grimeCts = null;
        }

        /// <summary>
        /// _grimeCycleInterval마다 블렌드를 _grimeStep만큼 늘려가며 완전히 뒤덮일 때(1)까지 반복
        /// </summary>
        private async Awaitable RunCycle(CancellationToken token)
        {
            try
            {
                float blend = 0f;

                while (blend < 1f)
                {
                    await AwaitableUtil.WaitScaled(_grimeCycleInterval, token);

                    float from = blend;
                    blend = Mathf.Min(1f, blend + _grimeStep);

                    await FadeGrimeBlend(from, blend, _grimeFadeDuration, token);
                }
            }
            catch (System.OperationCanceledException oce)
            {
                Debug.Log(oce);
            }
        }

        /// <summary>
        /// 그림 블렌드 값을 duration 동안 from -> to로 보간 (Time.timeScale 영향을 받음 - 일시정지 중엔 진행 안 됨)
        /// </summary>
        private async Awaitable FadeGrimeBlend(float from, float to, float duration, CancellationToken token)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                _physicalGrimeMaterial.SetFloat(GrimeBlendPropertyId, Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration)));

                await Awaitable.NextFrameAsync(token);
            }

            _physicalGrimeMaterial.SetFloat(GrimeBlendPropertyId, to);
        }
    }
}
