using UnityEngine.UIElements;
using Assets.MyAssets.Scripts.UI.Controls;

namespace Assets.MyAssets.Scripts.UI
{
    /// <summary>
    /// 게임 규칙 안내 UI - 텍스트만 표시하는 읽기 전용 패널 (닫기 버튼 외 상호작용 없음)
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
