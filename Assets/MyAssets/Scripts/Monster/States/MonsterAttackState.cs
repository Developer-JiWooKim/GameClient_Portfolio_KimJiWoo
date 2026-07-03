namespace Assets.MyAssets.Scripts.Monster.States
{
    /// <summary>
    /// 공격 상태 - 타겟을 바라보며 공격, 공격 범위를 벗어나면 Chase로 복귀
    /// </summary>
    public class MonsterAttackState : IMonsterState
    {
        public bool IsAlertState => true;

        public void Enter(MonsterController controller) { }

        public void Exit(MonsterController controller) { }

        public void Tick(MonsterController controller)
        {
            controller.Move.StopMovement();
            controller.Move.LookAtTarget(controller.TargetPosition);

            if (!controller.AttackTrigger.PlayerInAttackRange)
            {
                controller.ChangeState(controller.ChaseState);
                return;
            }

            bool didAttack = controller.AttackTrigger.Player != null && controller.AttackTrigger.Player.TakeDamage();

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
