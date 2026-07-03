using UnityEngine;
using UnityEngine.UI;


namespace Assets.MyAssets.Scripts.UI
{
    /// <summary>
    /// 화면 전체를 덮는 빨간 Image를 피격 시 깜빡이듯 보여주는 컴포넌트
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class DamageflashUI : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _maxAlpha = 0.35f;
        [SerializeField] private float _fadeDuration = 0.4f;

        private Image _flashImage;

        private bool  _isFlashing;
        private float _flashTimer;

        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            _flashImage = GetComponent<Image>();

            Color c = _flashImage.color;
            c.a = 0f;
            _flashImage.color = c;

            _isFlashing = false;
        }

        /// <summary>
        /// 화면을 빨갛게 깜빡이게 하는 메소드
        /// </summary>
        public void Flash()
        {
            _flashTimer = 0f;

            // 이미 Flash가 진행 중이면 타이머만 리셋, 중복 방지
            if (!_isFlashing)
            {
                _ = PlayFlash();
            }
        }

        private async Awaitable PlayFlash()
        {
            _isFlashing = true;

            try
            {
                Color c = _flashImage.color;

                while (_flashTimer < _fadeDuration)
                {
                    c.a = Mathf.Lerp(_maxAlpha, 0f, _flashTimer / _fadeDuration);

                    _flashImage.color = c;

                    _flashTimer += Time.deltaTime;

                    await Awaitable.NextFrameAsync(destroyCancellationToken);
                }

                c.a = 0f;

                _flashImage.color = c;
            }
            catch (System.OperationCanceledException oce)
            {
                Debug.LogException(oce);
            }
            finally
            {
                // 예외(또는 정상 취소)가 나도 _isFlashing이 영원히 true로 고정되지 않도록 무조건 복구
                _isFlashing = false;
            }
        }
    }
}
