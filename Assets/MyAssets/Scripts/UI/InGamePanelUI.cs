using Assets.MyAssets.Scripts.UI.Controls;
using UnityEngine.UIElements;

namespace Assets.MyAssets.Scripts.UI
{
    public class InGamePanelUI : BasePanelUI
    {
        private Label _hpText;
        private Label _timerText;
        private Label _keyCountText;
        private VisualElement _sanityFill;
        private IVisualElementScheduledItem _sanityPulseSchedule;
        private bool _sanityPulseOn;

        private Label _layerBlockedWarningText;
        private IVisualElementScheduledItem _layerBlockedWarningHideSchedule;

        public event System.Action OnPauseClicked;

        protected override void Start()
        {
            base.Start();

            _hpText = Root.Q<Label>("hp-text");
            _timerText = Root.Q<Label>("timer-text");
            _keyCountText = Root.Q<Label>("key-count-text");
            _sanityFill = Root.Q<VisualElement>("sanity-fill");
            _layerBlockedWarningText = Root.Q<Label>("layer-blocked-warning-text");

            Root.Q<CutButton>("pause-button").clicked += () => OnPauseClicked?.Invoke();
        }

        public void UpdateHp(int current, int max)
        {
            _hpText.text = "HP : " + current.ToString() + " / " + max.ToString();
        }

        public void UpdateKeyCount(int current, int required)
        {
            _keyCountText.text = "Keys : " + current.ToString() + " / " + required.ToString();
        }

        public void UpdateTimer(string formattedTime)
        {
            _timerText.text = formattedTime;
        }

        /// <summary>
        /// 레이어 전환이 벽에 막혔을 때 "미로를 전환할 수 없습니다" 문구를 잠깐 띄웠다가 자동으로 숨김
        /// </summary>
        public void ShowLayerBlockedWarning()
        {
            _layerBlockedWarningHideSchedule?.Pause();

            _layerBlockedWarningText.EnableInClassList("visible", true);

            _layerBlockedWarningHideSchedule = _layerBlockedWarningText.schedule.Execute(() =>
            {
                _layerBlockedWarningText.EnableInClassList("visible", false);
            }).StartingIn(1200);
        }

        public void UpdateSanity(float current, float max)
        {
            float ratio = max > 0f ? current / max : 0f;
            _sanityFill.style.width = Length.Percent(ratio * 100f);

            bool isLow = ratio <= 0.25f;
            _sanityFill.EnableInClassList("low", isLow);

            if (isLow && _sanityPulseSchedule == null)
            {
                _sanityPulseSchedule = _sanityFill.schedule.Execute(() =>
                {
                    _sanityPulseOn = !_sanityPulseOn;
                    _sanityFill.EnableInClassList("pulse", _sanityPulseOn);
                }).Every(500);
            }
            else if (!isLow && _sanityPulseSchedule != null)
            {
                _sanityPulseSchedule.Pause();
                _sanityPulseSchedule = null;
                _sanityFill.EnableInClassList("pulse", false);
            }
        }
    }
}
