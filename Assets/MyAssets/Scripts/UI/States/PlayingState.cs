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
            // 게임 시작 연출(카메라 전환 + 인트로 애니메이션) 중에는 GameUIController가 연출 완료 시점에
            // 직접 재개/입력허용/HUD표시를 처리하므로 여기서는 건너뜀
            if (controller.IsGameStarting) return;

            GameManager.Instance.ResumeGame();
            controller.SetPlayerControlEnabled(true);
            controller.InGamePanel.Show();
        }

        public void Exit(GameUIController controller) { }
    }
}
