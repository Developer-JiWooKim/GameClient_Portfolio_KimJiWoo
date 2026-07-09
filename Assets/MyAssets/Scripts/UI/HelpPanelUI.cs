using Assets.MyAssets.Scripts.UI.Controls;
using UnityEngine.UIElements;

namespace Assets.MyAssets.Scripts.UI
{
    /// <summary>
    /// 게임 규칙 안내 UI
    /// </summary>
    public class HelpPanelUI : BasePanelUI
    {
        protected override void Start()
        {
            base.Start();

            Root.Q<CutButton>("close-button").clicked += Hide;
        }
    }
}
