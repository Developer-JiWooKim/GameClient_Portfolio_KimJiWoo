using UnityEngine.UIElements;
using Assets.MyAssets.Scripts.UI.Controls;
using Assets.MyAssets.Scripts.Utility.SingleTon;

namespace Assets.MyAssets.Scripts.UI
{
    public class ResultPanelUI : BasePanelUI
    {
        private Label _resultText;
        private Label _resultTimeText;

        public event System.Action OnReplayClicked;
        public event System.Action OnSelectClicked;

        protected override void Start()
        {
            base.Start();

            _resultText = Root.Q<Label>("result-text");
            _resultTimeText = Root.Q<Label>("result-time-text");

            Root.Q<CutButton>("replay-button").clicked += () => OnReplayClicked?.Invoke();
            Root.Q<CutButton>("select-button").clicked += () => OnSelectClicked?.Invoke();
            Root.Q<CutButton>("game-end-button").clicked += () => GameManager.Instance.GameExit();
        }

        /// <summary>
        /// 결과 메시지/시간을 채운 뒤 패널을 표시하는 메소드 - CLEAR/GAME OVER에 따라 gold/red 포인트 색 분기(UI_Design_Reference.md 참고)
        /// </summary>
        public void Show(string message, string formattedTime)
        {
            _resultText.text = message;
            _resultTimeText.text = $"End Time : {formattedTime}";

            bool isClear = message.Contains("CLEAR");
            _resultText.EnableInClassList("clear", isClear);
            _resultText.EnableInClassList("gameover", !isClear);

            Show();
        }
    }
}
