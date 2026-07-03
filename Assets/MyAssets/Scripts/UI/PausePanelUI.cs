using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.MyAssets.Scripts.UI
{
    public class PausePanelUI : BasePanelUI
    {
        [SerializeField] private TextMeshProUGUI _hpText;
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private TextMeshProUGUI _keyCountText;
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _optionsButton;
        [SerializeField] private Button _replayButton;
        [SerializeField] private Button _endButton;
        [SerializeField] private OptionsPanelUI _optionsPanel;

        public event System.Action OnResumeClicked;
        public event System.Action OnReplayClicked;
        public event System.Action OnEndClicked;

        private void Awake()
        {
            _resumeButton.onClick.AddListener(() => OnResumeClicked?.Invoke());
            _optionsButton.onClick.AddListener(() => _optionsPanel.Show());
            _replayButton.onClick.AddListener(() => OnReplayClicked?.Invoke());
            _endButton.onClick.AddListener(() => OnEndClicked?.Invoke());
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
