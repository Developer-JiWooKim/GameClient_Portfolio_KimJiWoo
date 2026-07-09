using System.Threading;
using UnityEngine;

namespace Assets.MyAssets.Scripts.Utility.Core
{
    /// <summary>
    /// Awaitable 기반 취소 가능한 대기 헬퍼 모음. 취소(OperationCanceledException)는 삼키지 않고 호출자에게 그대로 전파함
    /// </summary>
    public static class AwaitableUtil
    {
        /// <summary>
        /// Time.timeScale 기준(일시정지 시 함께 정지)으로 duration만큼 대기
        /// </summary>
        public static async Awaitable WaitScaled(float duration, CancellationToken token)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                await Awaitable.NextFrameAsync(token);
            }
        }

        /// <summary>
        /// Time.timeScale과 무관하게 실제 시간 기준으로 대기
        /// </summary>
        public static async Awaitable WaitUnscaled(float duration, CancellationToken token)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                await Awaitable.NextFrameAsync(token);
            }
        }
    }
}
