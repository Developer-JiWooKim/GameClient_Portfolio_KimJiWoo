using Assets.MyAssets.Scripts.UI.Controls;
using Assets.MyAssets.Scripts.Utility.Core;
using Assets.MyAssets.Scripts.Utility.SingleTon;
using UnityEngine.UIElements;

namespace Assets.MyAssets.Scripts.UI
{
    public class SelectPanelUI : BasePanelUI
    {
        public event System.Action OnGameModeConfirmed;
        public event System.Action OnBackClicked;

        protected override void Start()
        {
            base.Start();

            Root.Q<CutCard>("normal-card").clicked += () => Confirm(GameMode.Normal);
            Root.Q<CutCard>("hard-card").clicked += () => Confirm(GameMode.Hard);
            Root.Q<CutButton>("back-button").clicked += () => OnBackClicked?.Invoke();
        }

        private void Confirm(GameMode gameMode)
        {
            GameManager.CurrentGameMode = gameMode;

            OnGameModeConfirmed?.Invoke();
        }
    }
}
