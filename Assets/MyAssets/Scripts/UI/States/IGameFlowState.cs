/// <summary>
/// 게임 UI 흐름 상태의 진입/이탈 시 동작을 정의하는 인터페이스
/// </summary>
public interface IGameFlowState
{
    void Enter(GameUIController controller);
    void Exit(GameUIController controller);
}
