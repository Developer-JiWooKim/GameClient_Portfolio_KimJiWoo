using System;
using Assets.MyAssets.Scripts.Player;
using Assets.MyAssets.Scripts.Utility.Core;
using Assets.MyAssets.Scripts.Utility.SingleTon;
using UnityEngine;

namespace Assets.MyAssets.Scripts.Utility.Visuals
{
    /// <summary>
    /// 레이어 전환 시 화면 일렁임 연출(일시정지 + 입력차단 + 리플 페이드 + 레이어 교체 + 복구)을 담당하는 컴포넌트
    /// </summary>
    public class LayerTransitionSequencer : MonoBehaviour
    {
        [Header("Layer Transition FX")]
        [SerializeField] private ScreenRippleController _rippleController;
        [SerializeField] private float _rippleInDuration = 0.1f;
        [SerializeField] private float _rippleHoldDuration = 0.05f; // 일렁임이 최고조일 때 실제로 미로를 바꿔치기하는 구간
        [SerializeField] private float _rippleOutDuration = 0.1f;

        private bool _isTransitioning = false;
        public bool IsTransitioning => _isTransitioning; // 전환 연출 중 일시정지 등 다른 흐름 진입을 막기 위한 상태 노출

        /// <summary>
        /// 타이머/입력/유닛 움직임을 멈추고 화면 일렁임 효과 안에서 실제 미로를 교체한 뒤 다시 재생시키는 시퀀스
        /// onSwapLayer는 일렁임이 화면을 가리는 시점(페이드 인 완료 후)에 호출됨
        /// </summary>
        public async Awaitable PlayTransition(PlayerInputHandler playerInput, Action onSwapLayer)
        {
            if (playerInput == null)
            {
                Debug.LogError("LayerTransitionSequencer PlayTransition(): playerInput is Null");
                return;
            }

            _isTransitioning = true;

            try
            {
                SoundManager.Instance?.PlayLayerSwitch();

                GameManager.Instance.PauseGame();

                // PlayerInputHandler.OnDisable()에서 입력값도 같이 초기화됨
                playerInput.enabled = false;

                await FadeRipple(0f, 1f, _rippleInDuration);
                await AwaitableUtil.WaitUnscaled(_rippleHoldDuration, destroyCancellationToken);

                onSwapLayer(); // 일렁임이 화면을 가리는 동안 레이어 교체

                await FadeRipple(1f, 0f, _rippleOutDuration);
            }
            catch (System.OperationCanceledException oce)
            {
                Debug.Log(oce);
            }
            finally
            {
                // 예외가 나도(또는 정상 취소돼도) 입력/시간이 영원히 멈춰있지 않도록 무조건 복구
                playerInput.enabled = true;

                GameManager.Instance.ResumeGame();

                _isTransitioning = false;
            }
        }

        /// <summary>
        /// 일렁임 강도를 duration 동안 from -> to로 보간 (unscaled time 기준)
        /// </summary>
        private async Awaitable FadeRipple(float from, float to, float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;

                _rippleController.SetIntensity(Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration)));

                await Awaitable.NextFrameAsync(destroyCancellationToken);
            }

            _rippleController.SetIntensity(to);
        }
    }
}
