using UnityEngine.UIElements;
using Assets.MyAssets.Scripts.UI.Controls;
using Assets.MyAssets.Scripts.Utility.SingleTon;

namespace Assets.MyAssets.Scripts.UI
{
    /// <summary>
    /// 옵션 UI - SFX/BGM 볼륨 슬라이더와 음소거 토글 (세션 내에서만 적용, 저장 안 함)
    /// </summary>
    public class OptionsPanelUI : BasePanelUI
    {
        private Slider _sfxVolumeSlider;
        private Toggle _sfxMuteToggle;
        private Slider _bgmVolumeSlider;
        private Toggle _bgmMuteToggle;

        protected override void Start()
        {
            base.Start();

            Root.Q<CutButton>("close-button").clicked += Hide;

            _sfxVolumeSlider = Root.Q<Slider>("sfx-volume-slider");
            _sfxMuteToggle   = Root.Q<Toggle>("sfx-mute-toggle");
            _bgmVolumeSlider = Root.Q<Slider>("bgm-volume-slider");
            _bgmMuteToggle   = Root.Q<Toggle>("bgm-mute-toggle");

            if (SoundManager.Instance == null) return;

            // 슬라이더/토글 리스너가 곧바로 되쏘지 않도록 SetValueWithoutNotify로 현재 값 초기화
            _sfxVolumeSlider.SetValueWithoutNotify(SoundManager.Instance.SfxVolume);
            _bgmVolumeSlider.SetValueWithoutNotify(SoundManager.Instance.BgmVolume);
            _sfxMuteToggle.SetValueWithoutNotify(SoundManager.Instance.IsSfxMuted);
            _bgmMuteToggle.SetValueWithoutNotify(SoundManager.Instance.IsBgmMuted);

            _sfxVolumeSlider.RegisterValueChangedCallback(evt => SoundManager.Instance.SetSfxVolume(evt.newValue));
            _bgmVolumeSlider.RegisterValueChangedCallback(evt => SoundManager.Instance.SetBgmVolume(evt.newValue));
            _sfxMuteToggle.RegisterValueChangedCallback(evt => SoundManager.Instance.SetSfxMuted(evt.newValue));
            _bgmMuteToggle.RegisterValueChangedCallback(evt => SoundManager.Instance.SetBgmMuted(evt.newValue));
        }
    }
}
