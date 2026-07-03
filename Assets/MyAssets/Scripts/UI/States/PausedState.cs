using Assets.MyAssets.Scripts.Utility.Core;
using Assets.MyAssets.Scripts.Utility.SingleTon;

namespace Assets.MyAssets.Scripts.UI.States
{
    /// <summary>
    /// 일시정지 상태 - InGamePanel은 그대로 둔 채 PausePanel을 오버레이로 표시
    /// </summary>
    public class PausedState : IGameFlowState
    {
        public void Enter(GameUIController controller)
        {
            GameManager.Instance.PauseGame();
            controller.SetPlayerInputEnabled(false);

            GameRule gameRule = GameManager.Instance.GameRule;

            controller.PausePanel.Show(
                controller.Player.CurrentHp, controller.Player.MaxHp,
                gameRule.CurrentCollectedKeyCount, gameRule.RequiredKeyCount,
                GameManager.Instance.GameTimer.GetFormattedTime());
        }

        public void Exit(GameUIController controller)
        {
            // PausePanel.Hide()가 자기 자식인 OptionsPanel까지 함께 닫음
            controller.PausePanel.Hide();
        }
    }
}
