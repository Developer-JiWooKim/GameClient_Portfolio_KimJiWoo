namespace Assets.MyAssets.Scripts.Monster.States
{
    /// <summary>
    /// 몬스터 AI 상태의 행동과 전환 조건을 정의하는 interface
    /// </summary>
    public interface IMonsterState
    {
        bool IsAlertState { get; } // 현재 상태를 알리는 프로퍼티, 플레이어를 감지했는지 여부

        void Enter(MonsterController controller);   // 현재 상태를 시작할 때 1번 실행
        void Exit(MonsterController controller);    // 현재 상태를 끝내고 나갈 때 1번 실행
        void Tick(MonsterController controller);    // 현재 상태일 때 매 프레임 실행
    }
}
