using UnityEngine;
using UnityEngine.UIElements;
using Assets.MyAssets.Scripts.UI.Controls;

namespace Assets.MyAssets.Scripts.UI
{
    public class PausePanelUI : BasePanelUI
    {
        [SerializeField] private OptionsPanelUI _optionsPanel;

        private Label _hpText;
        private Label _timerText;
        private Label _keyCountText;

        public event System.Action OnResumeClicked;
        public event System.Action OnReplayClicked;
        public event System.Action OnEndClicked;

        protected override void Start()
        {
            base.Start();

            _hpText = Root.Q<Label>("hp-text");
            _timerText = Root.Q<Label>("timer-text");
            _keyCountText = Root.Q<Label>("key-count-text");

            Root.Q<CutButton>("resume-button").clicked += () => OnResumeClicked?.Invoke();
            Root.Q<CutButton>("options-button").clicked += () => _optionsPanel.Show();
            Root.Q<CutButton>("replay-button").clicked += () => OnReplayClicked?.Invoke();
            Root.Q<CutButton>("end-button").clicked += () => OnEndClicked?.Invoke();
        }

        /// <summary>
        /// 일시정지 시점의 체력/열쇠/타이머 스냅샷을 채운 뒤 패널을 표시
        /// </summary>
        public void Show(int currentHp, int maxHp, int currentKeys, int requiredKeys, string formattedTime)
        {
            _hpText.text       = $"HP : {currentHp} / {maxHp}";
            _keyCountText.text = $"Keys : {currentKeys} / {requiredKeys}";
            _timerText.text    = formattedTime;

            Show();
        }

        public override void Hide()
        {
            _optionsPanel.Hide();
            base.Hide();
        }
    }
}
