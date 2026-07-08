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
            // 감지 범위를 벗어나면 Idle상태로 전환
            if (!controller.IsInRange)
            {
                controller.ChangeState(controller.IdleState);
                return;
            }

            // 타겟(플레이어)을 향해 이동 및 추격 애니메이션 재생
            controller.Move.MoveToTarget(controller.TargetPosition);
            controller.Anim.PlayRun();

            // 추격 중 플레이어가 공격 범위 안에 들어오면(TriggerEnter로 체크) Attack상태로 전환
            if (controller.AttackTrigger.PlayerInAttackRange)
            {
                controller.ChangeState(controller.AttackState);
            }
        }
    }
}
