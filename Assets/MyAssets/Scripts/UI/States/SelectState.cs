namespace Assets.MyAssets.Scripts.UI.States
{
    /// <summary>
    /// 난이도 선택 화면
    /// </summary>
    public class SelectState : IGameFlowState
    {
        public void Enter(GameUIController controller)
        {
            controller.SelectPanel.Show();
        }

        public void Exit(GameUIController controller)
        {
            controller.SelectPanel.Hide();
        }
    }
}
