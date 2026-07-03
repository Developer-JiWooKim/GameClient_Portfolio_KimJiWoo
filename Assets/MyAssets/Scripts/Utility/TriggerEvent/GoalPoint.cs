using UnityEngine;
using Assets.MyAssets.Scripts.Utility.SingleTon;

namespace Assets.MyAssets.Scripts.Utility.TriggerEvent
{
    public class GoalPoint : MonoBehaviour
    {
        /// <summary>
        /// 플레이어가 목표 지점에 들어왔을 때 게임 클리어 Trigger 이벤트
        /// </summary>
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            SoundManager.Instance?.PlayGoalReached();

            GameManager.Instance.GameRule.Clear();
        }
    }
}
