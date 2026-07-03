using Assets.MyAssets.Scripts.UI.States;

namespace Assets.MyAssets.Scripts.UI
{
    /// <summary>
    /// 게임 UI 흐름(Title/Select/Playing/Paused/Result) 전환을 관리하는 상태 머신
    /// </summary>
    public class GameFlowFSM
    {
        private readonly GameUIController _controller;

        private IGameFlowState _current;
        public IGameFlowState Current => _current;

        public readonly TitleState   TitleState   = new TitleState();
        public readonly SelectState  SelectState  = new SelectState();
        public readonly PlayingState PlayingState = new PlayingState();
        public readonly PausedState  PausedState  = new PausedState();
        public readonly ResultState  ResultState  = new ResultState();

        public GameFlowFSM(GameUIController controller)
        {
            _controller = controller;
        }

        /// <summary>
        /// 상태 전환 메소드, 이전 상태의 Exit와 새 상태의 Enter를 호출
        /// </summary>
        public void ChangeState(IGameFlowState next)
        {
            if (_current == next) return;

            _current?.Exit(_controller);
            _current = next;
            _current?.Enter(_controller);
        }
    }
}
