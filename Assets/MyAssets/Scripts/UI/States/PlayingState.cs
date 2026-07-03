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
            controller.SetPlayerInputEnabled(true);
            controller.InGamePanel.Show();
        }

        public void Exit(GameUIController controller)
        {
            // InGamePanel은 Paused 중에도 뒤에 계속 보여야 하므로 여기서 숨기지 않음.
            // 진짜로 숨겨야 하는 시점(Result/Title 진입)은 각 상태의 Enter가 책임짐.
        }
    }
}
