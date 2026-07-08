namespace Assets.MyAssets.Scripts.Monster.States
{
    /// <summary>
    /// Idle 상태 - 타겟을 감지하기 전까지 미로를 돌아다님, 감지되면 Chase로 전환
    /// </summary>
    public class MonsterIdleState : IMonsterState
    {
        public bool IsAlertState => false;

        public void Enter(MonsterController controller)
        {
            // Patrol하기 전 Target(이동할 목표 지점) 초기화, isStopped = false
            controller.Move.ClearPath();
        }

        public void Exit(MonsterController controller) { }

        public void Tick(MonsterController controller)
        {
            // Idle 상태에서(Patrol 중에) 타겟(플레이어)이 감지되면 Chase상태로 전환
            if (controller.IsSensed)
            {
                controller.ChangeState(controller.ChaseState);
                return;
            }

            controller.Move.Patrol();
            controller.Anim.PlayWalk();
        }
    }
}
