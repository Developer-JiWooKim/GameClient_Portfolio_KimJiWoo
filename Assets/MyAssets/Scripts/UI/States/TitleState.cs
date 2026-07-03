namespace Assets.MyAssets.Scripts.UI.States
{
    /// <summary>
    /// 타이틀 화면
    /// </summary>
    public class TitleState : IGameFlowState
    {
        public void Enter(GameUIController controller)
        {
            controller.TitlePanel.Show();
        }

        public void Exit(GameUIController controller)
        {
            controller.TitlePanel.Hide();
        }
    }
}
