using Assets.MyAssets.Scripts.Utility.SingleTon;

namespace Assets.MyAssets.Scripts.UI.States
{
    /// <summary>
    /// 실제 플레이 중 상태 - Select에서 새로 시작하거나 Paused에서 재개/재시작할 때 진입
    /// </summary>
    public class PlayingState : IGameFlowState
    {
        public void Enter(GameUIController controller)
        {
            GameManager.Instance.ResumeGame();
            controller.SetPlayerControlEnabled(true);
            controller.InGamePanel.Show();
        }

        public void Exit(GameUIController controller) { }
    }
}
