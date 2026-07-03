using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.MyAssets.Scripts.UI
{
    /// <summary>
    /// 옵션 UI 스켈레톤 - 화면 크기/SFX·BGM 볼륨/화면 흔들림 등 실제 항목은 추후 채움
    /// </summary>
    public class OptionsPanelUI : BasePanelUI
    {
        [SerializeField] private TextMeshProUGUI _optionText;
        // 화면 해상도 설정(할지, 안할지 모름)
        // 볼륨 조절
        // 화면 흔들림 On/Off 버튼으로 할듯?
        [SerializeField] private Button _closeButton;


        private void Awake()
        {
            _closeButton.onClick.AddListener(Hide);
        }
    }
}
