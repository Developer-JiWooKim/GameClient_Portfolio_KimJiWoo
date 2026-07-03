using UnityEngine;

namespace Assets.MyAssets.Scripts.ScriptableObject
{
    /// <summary>
    /// 게임에서 쓰는 모든 사운드 클립을 보관하는 순수 데이터 에셋
    /// </summary>
    [CreateAssetMenu(fileName = "SoundLibrary", menuName = "Audio/Sound Library")]
    public class SoundLibrary : UnityEngine.ScriptableObject
    {
        [Header("Gameplay SFX")]
        public AudioClip keyCollected;
        public AudioClip goalSpawned;
        public AudioClip goalReached;
        public AudioClip playerDamaged;
        public AudioClip gameClear;
        public AudioClip gameOver;
        public AudioClip layerSwitch;
        public AudioClip layerSwitchBlocked;

        [Header("BGM")]
        public AudioClip physicalBGM;
        public AudioClip arcaneBGM;
    }
}
