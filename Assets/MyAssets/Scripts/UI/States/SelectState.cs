namespace Assets.MyAssets.Scripts.UI.States
{
    /// <summary>
    /// 난이도 선택 UI에 들어와 있는 상태
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
