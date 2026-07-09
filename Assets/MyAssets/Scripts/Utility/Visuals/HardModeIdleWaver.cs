using System.Threading;
using Assets.MyAssets.Scripts.Utility.Core;
using Assets.MyAssets.Scripts.Utility.SingleTon;
using UnityEngine;

namespace Assets.MyAssets.Scripts.Utility.Visuals
{
    /// <summary>
    /// Hard 모드에서 일정 시간 경과 후 화면이 은은하게 상시 일렁이는 연출을 담당하는 컴포넌트
    /// </summary>
    public class HardModeIdleWaver : MonoBehaviour
    {
        [Header("Layer Transition FX 참조")]
        [SerializeField] private ScreenRippleController _rippleController;

        [Header("Idle Waver FX (Hard 모드)")]
        [SerializeField] private float _idleWaverStartDelay = 30f; // 일렁임 시작까지 대기 시간(초)
        [SerializeField] private float _idleWaverAmplitude = 0.1f; // 일렁임 최대 강도 (레이어 전환의 1.0 대비 은은하게)
        [SerializeField] private float _idleWaverFrequency = 0.6f; // 사인파 진동 속도

        private CancellationTokenSource _idleWaverCts;

        private void OnDestroy()
        {
            _idleWaverCts?.Cancel();
            _idleWaverCts?.Dispose();
        }

        /// <summary>
        /// Hard 모드에서 새 판이 시작될 때 일렁임을 0으로 되돌리고 60초 뒤부터 시작되는 은은한 상시 일렁임을 새로 시작.
        /// Normal 모드에서는 시작하지 않음
        /// </summary>
        public void RestartCycle()
        {
            _idleWaverCts?.Cancel();
            _idleWaverCts?.Dispose();
            _idleWaverCts = null;

            _rippleController.SetIntensity(0f);

            if (GameManager.CurrentGameMode != GameMode.Hard) return;

            _idleWaverCts = new CancellationTokenSource();
            _ = RunCycle(_idleWaverCts.Token);

            GameManager.Instance.GameRule.OnClear += StopCycle;
            GameManager.Instance.GameRule.OnGameOver += StopCycle;
        }

        private void StopCycle()
        {
            _idleWaverCts?.Cancel();
            _idleWaverCts?.Dispose();
            _idleWaverCts = null;

            _rippleController.SetIntensity(0f);
        }

        /// <summary>
        /// _idleWaverStartDelay(초)만큼 대기한 뒤, 취소될 때까지 사인파 기반의 은은한 일렁임을 계속 유지
        /// </summary>
        private async Awaitable RunCycle(CancellationToken token)
        {
            try
            {
                await AwaitableUtil.WaitScaled(_idleWaverStartDelay, token);

                while (!token.IsCancellationRequested)
                {
                    if (!MazeLayerManager.Instance.IsTransitioning) // 레이어 전환의 FadeRipple과 값 충돌 방지
                    {
                        float intensity = _idleWaverAmplitude * (0.5f + 0.5f * Mathf.Sin(Time.time * _idleWaverFrequency));
                        _rippleController.SetIntensity(intensity);
                    }

                    await Awaitable.NextFrameAsync(token);
                }
            }
            catch (System.OperationCanceledException oce)
            {
                Debug.Log(oce);
            }
        }
    }
}
