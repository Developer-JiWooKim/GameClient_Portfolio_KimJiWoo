using Assets.MyAssets.Scripts.Utility.SingleTon;

namespace Assets.MyAssets.Scripts.UI.States
{
    /// <summary>
    /// 게임 클리어/오버 후 결과를 나타내는 상태
    /// </summary>
    public class ResultState : IGameFlowState
    {
        public void Enter(GameUIController controller)
        {
            // Paused 상태(End 버튼)에서 곧장 넘어올 수 있어 timeScale이 0으로 남아있을 수 있으므로 복구
            GameManager.Instance.ResumeGame();

            controller.InGamePanel.Hide();
            controller.SetPlayerControlEnabled(false);
            controller.ResultPanel.Show(controller.PendingResultMessage, GameManager.Instance.GameTimer.GetFormattedTime());
        }

        public void Exit(GameUIController controller)
        {
            controller.ResultPanel.Hide();
        }
    }
}
