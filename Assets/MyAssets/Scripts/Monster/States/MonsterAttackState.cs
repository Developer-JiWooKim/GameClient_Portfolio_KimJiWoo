using Assets.MyAssets.Scripts.Player;

namespace Assets.MyAssets.Scripts.Monster.States
{
    /// <summary>
    /// Attack 상태 - 타겟을 바라보며 공격, 공격 범위를 벗어나면 Chase로 복귀
    /// </summary>
    public class MonsterAttackState : IMonsterState
    {
        public bool IsAlertState => true;

        public void Enter(MonsterController controller) { }

        public void Exit(MonsterController controller) { }

        public void Tick(MonsterController controller)
        {
            // 공격 사정거리에 타겟(플레이어)이 들어오면 움직임을 멈추고(가속도 때문에 몬스터가 플레이어를 뚫고 지나가는 현상 방지)
            // 타겟(플레이어) 방향으로 회전
            controller.Move.StopMovement();
            controller.Move.LookAtTarget(controller.TargetPosition);

            MonsterAttackTrigger attackTrigger = controller.AttackTrigger;

            // 타겟(플레이어)이 공격 범위 안에 들어와 공격할 수 있는지 Trigger 체크 결과가 false면
            if (!attackTrigger.PlayerInAttackRange)
            {
                // 추격 상태로 전환
                controller.ChangeState(controller.ChaseState);
                return;
            }

            PlayerController player = attackTrigger.Player;

            bool didAttack = player != null && player.TakeDamage(); // 현재 공격 가능한 상태인지 체크

            // 공격 가능하면 Attack 애니메이션, 불가능 하면 Idle애니메이션
            if (didAttack)
            {
                controller.Anim.PlayAttack();
            }
            else
            {
                controller.Anim.PlayIdle();
            }
        }
    }
}
