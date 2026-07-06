using UnityEngine.UIElements;
using Assets.MyAssets.Scripts.UI.Controls;

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

        public event System.Action OnPauseClicked;

        protected override void Start()
        {
            base.Start();

            _hpText = Root.Q<Label>("hp-text");
            _timerText = Root.Q<Label>("timer-text");
            _keyCountText = Root.Q<Label>("key-count-text");
            _sanityFill = Root.Q<VisualElement>("sanity-fill");

            Root.Q<CutButton>("pause-button").clicked += () => OnPauseClicked?.Invoke();
        }

        public void UpdateHp(int current, int max)
        {
            _hpText.text = $"HP : {current} / {max}";
        }

        public void UpdateKeyCount(int current, int required)
        {
            _keyCountText.text = $"Keys : {current} / {required}";
        }

        public void UpdateTimer(string formattedTime)
        {
            _timerText.text = formattedTime;
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
