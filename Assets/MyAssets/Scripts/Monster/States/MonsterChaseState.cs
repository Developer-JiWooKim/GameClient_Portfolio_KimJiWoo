namespace Assets.MyAssets.Scripts.Monster.States
{
    /// <summary>
    /// Chase 상태 - 타겟을 향해 이동, 감지 범위를 벗어나면 Idle로, 공격 범위에 들어오면 Attack으로 전환
    /// </summary>
    public class MonsterChaseState : IMonsterState
    {
        public bool IsAlertState => true;

        public void Enter(MonsterController controller) { }

        public void Exit(MonsterController controller) { }

        public void Tick(MonsterController controller)
        {
            if (!controller.IsInRange)
            {
                controller.ChangeState(controller.IdleState);
                return;
            }

            controller.Move.MoveToTarget(controller.TargetPosition);
            controller.Anim.PlayRun();

            if (controller.AttackTrigger.PlayerInAttackRange)
            {
                controller.ChangeState(controller.AttackState);
            }
        }
    }
}
