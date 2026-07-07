using Assets.MyAssets.Scripts.Player;
using UnityEngine;

namespace Assets.MyAssets.Scripts.Monster
{
    /// <summary>
    /// 몬스터 공격 범위 안에 플레이어가 들어왔는지 체크, Trigger 이벤트
    /// </summary>
    public class MonsterAttackTrigger : MonoBehaviour
    {
        public PlayerController Player { get; private set; }
        public bool PlayerInAttackRange { get; private set; } = false;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            PlayerInAttackRange = true;

            Player = other.GetComponent<PlayerController>();
            if (Player == null)
            {
                Debug.LogError($"MonsterAttackTrigger: PlayerController가 없습니다! 대상: {other.name}", other.gameObject);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            PlayerInAttackRange = false;
            Player = null;
        }

        /// <summary>
        /// 오브젝트 풀에서 재사용될 때 이전 타겟팅 상태를 초기화하는 메소드
        /// </summary>
        public void ResetTrigger()
        {
            PlayerInAttackRange = false;
            Player = null;
        }
    }
}
