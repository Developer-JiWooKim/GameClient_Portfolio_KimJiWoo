using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Assets.MyAssets.Scripts.Utility.SingleTon;

namespace Assets.MyAssets.Scripts.UI
{
    /// <summary>
    /// 옵션 UI - SFX/BGM 볼륨 슬라이더와 음소거 토글 (세션 내에서만 적용, 저장 안 함)
    /// </summary>
    public class OptionsPanelUI : BasePanelUI
    {
        [SerializeField] private TextMeshProUGUI _optionText;
        // 화면 해상도 설정(할지, 안할지 모름)
        // 화면 흔들림 On/Off 버튼으로 할듯?
        [SerializeField] private Button _closeButton;

        [Header("SFX")]
        [SerializeField] private Slider _sfxVolumeSlider;
        [SerializeField] private Toggle _sfxMuteToggle;

        [Header("BGM")]
        [SerializeField] private Slider _bgmVolumeSlider;
        [SerializeField] private Toggle _bgmMuteToggle;

        private void Awake()
        {
            _closeButton.onClick.AddListener(Hide);
        }

        private void Start()
        {
            if (SoundManager.Instance == null) return;

            // 슬라이더/토글 리스너가 곧바로 되쏘지 않도록 Notify 없이 현재 값으로 초기화
            _sfxVolumeSlider.SetValueWithoutNotify(SoundManager.Instance.SfxVolume);
            _bgmVolumeSlider.SetValueWithoutNotify(SoundManager.Instance.BgmVolume);
            _sfxMuteToggle.SetIsOnWithoutNotify(SoundManager.Instance.IsSfxMuted);
            _bgmMuteToggle.SetIsOnWithoutNotify(SoundManager.Instance.IsBgmMuted);

            _sfxVolumeSlider.onValueChanged.AddListener(SoundManager.Instance.SetSfxVolume);
            _bgmVolumeSlider.onValueChanged.AddListener(SoundManager.Instance.SetBgmVolume);
            _sfxMuteToggle.onValueChanged.AddListener(SoundManager.Instance.SetSfxMuted);
            _bgmMuteToggle.onValueChanged.AddListener(SoundManager.Instance.SetBgmMuted);
        }
    }
}
