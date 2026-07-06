using UnityEngine;
using UnityEngine.UIElements;
using Assets.MyAssets.Scripts.UI.Controls;
using Assets.MyAssets.Scripts.Utility.SingleTon;

namespace Assets.MyAssets.Scripts.UI
{
    public class TitlePanelUI : BasePanelUI
    {
        [SerializeField] private OptionsPanelUI _optionsPanel;

        public event System.Action OnPlayClicked;

        protected override void Start()
        {
            base.Start();

            Root.Q<CutButton>("start-button").clicked += () => OnPlayClicked?.Invoke();
            Root.Q<CutButton>("options-button").clicked += () => _optionsPanel.Show();
            Root.Q<CutButton>("exit-button").clicked += () => GameManager.Instance.GameExit();
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
