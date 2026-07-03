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
        [SerializeField] private float _bgmVolume = 0.5f;

        private void Start()
        {
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
    }
}
