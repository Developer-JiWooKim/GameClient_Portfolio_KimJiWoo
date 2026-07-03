using UnityEngine;
using UnityEngine.UI;
using Assets.MyAssets.Scripts.Utility.SingleTon;

namespace Assets.MyAssets.Scripts.UI
{
    public class TitlePanelUI : BasePanelUI
    {
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _optionButton;
        [SerializeField] private Button _exitButton;
        [SerializeField] private OptionsPanelUI _optionsPanel;

        public event System.Action OnPlayClicked;

        private void Awake()
        {
            _playButton.onClick.AddListener(() => OnPlayClicked?.Invoke());

            _optionButton.onClick.AddListener(() => _optionsPanel.Show());

            _exitButton.onClick.AddListener(() => GameManager.Instance.GameExit());
        }

        /// <summary>
        /// 타이틀에서 다른 화면으로 넘어갈 때 옵션 패널이 열려있는 채로 남지 않도록 함께 닫음
        /// </summary>
        public override void Hide()
        {
            _optionsPanel.Hide();
            base.Hide();
        }
    }
}
