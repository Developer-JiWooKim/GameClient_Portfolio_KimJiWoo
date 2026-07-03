using UnityEngine;
using Assets.MyAssets.Scripts.ScriptableObject;

namespace Assets.MyAssets.Scripts.Utility.SingleTon
{
    /// <summary>
    /// 사운드 재생을 책임지는 매니저
    /// </summary>
    public class SoundManager : Singleton<SoundManager>
    {
        [SerializeField] private SoundLibrary _library;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource _sfxSource;
        [SerializeField] private AudioSource _bgmSource;
        [SerializeField] private float _sfxVolume = 1f;
        [SerializeField] private float _bgmVolume = 0.5f;

        public float SfxVolume => _sfxVolume;
        public float BgmVolume => _bgmVolume;
        public bool  IsSfxMuted => _sfxSource != null && _sfxSource.mute;
        public bool  IsBgmMuted => _bgmSource != null && _bgmSource.mute;

        private void Start()
        {
            if (_sfxSource != null) _sfxSource.volume = _sfxVolume;
            if (_bgmSource != null) _bgmSource.volume = _bgmVolume;

            if (MazeLayerManager.Instance != null)
            {
                MazeLayerManager.Instance.OnLayerChanged       += PlayBGMForLayer;
                MazeLayerManager.Instance.OnLayerSwitchBlocked += PlayLayerSwitchBlocked;
            }
        }

        private void OnDisable()
        {
            if (MazeLayerManager.Instance != null)
            {
                MazeLayerManager.Instance.OnLayerChanged       -= PlayBGMForLayer;
                MazeLayerManager.Instance.OnLayerSwitchBlocked -= PlayLayerSwitchBlocked;
            }
        }

        /// <summary>
        /// 효과음 하나 재생
        /// </summary>
        private void PlaySFX(AudioClip clip)
        {
            if (clip == null || _sfxSource == null) return;

            _sfxSource.PlayOneShot(clip);
        }

        public void PlayKeyCollected() => PlaySFX(_library?.keyCollected);
        public void PlayGoalSpawned() => PlaySFX(_library?.goalSpawned);
        public void PlayGoalReached() => PlaySFX(_library?.goalReached);
        public void PlayPlayerDamaged() => PlaySFX(_library?.playerDamaged);
        public void PlayGameClear() => PlaySFX(_library?.gameClear);
        public void PlayGameOver() => PlaySFX(_library?.gameOver);
        public void PlayLayerSwitch() => PlaySFX(_library?.layerSwitch);
        public void PlayLayerSwitchBlocked() => PlaySFX(_library?.layerSwitchBlocked);

        /// <summary>
        /// 현재 레이어에 맞는 배경음으로 교체 (이미 같은 클립이 재생 중이면 무시)
        /// </summary>
        public void PlayBGMForLayer(MazeLayerManager.LayerType layer)
        {
            if (_library == null || _bgmSource == null) return;

            AudioClip clip = layer == MazeLayerManager.LayerType.Physical ? _library.physicalBGM : _library.arcaneBGM;

            if (clip == null) return;
            if (_bgmSource.clip == clip && _bgmSource.isPlaying) return;

            _bgmSource.clip = clip;
            _bgmSource.volume = _bgmVolume;
            _bgmSource.loop = true;
            _bgmSource.Play();
        }

        public void StopBGM()
        {
            if (_bgmSource == null) return;

            _bgmSource.Stop();
        }

        /// <summary>
        /// SFX 볼륨 설정 (0~1). 이후 재생되는 효과음부터 즉시 반영됨
        /// </summary>
        public void SetSfxVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);

            if (_sfxSource != null) _sfxSource.volume = _sfxVolume;
        }

        /// <summary>
        /// BGM 볼륨 설정 (0~1). 재생 중인 BGM에도 즉시 반영됨
        /// </summary>
        public void SetBgmVolume(float volume)
        {
            _bgmVolume = Mathf.Clamp01(volume);

            if (_bgmSource != null) _bgmSource.volume = _bgmVolume;
        }

        public void SetSfxMuted(bool isMuted)
        {
            if (_sfxSource != null) _sfxSource.mute = isMuted;
        }

        public void SetBgmMuted(bool isMuted)
        {
            if (_bgmSource != null) _bgmSource.mute = isMuted;
        }
    }
}
