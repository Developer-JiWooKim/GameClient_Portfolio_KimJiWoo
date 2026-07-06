using System;
using UnityEngine;
using Assets.MyAssets.Scripts.Utility.Core;
using Assets.MyAssets.Scripts.Utility.SingleTon;

namespace Assets.MyAssets.Scripts.Player
{
    /// <summary>
    /// Arcane 레이어에 머무는 동안 정신력이 깎이고 Physical로 돌아오면 회복되는 스탯.
    /// 0이 되면 즉시 게임오버(GameClient에 적용할 내용.txt 기획 노트 반영) - 하드 난이도가 더 가혹함.
    /// </summary>
    public class PlayerSanity : MonoBehaviour
    {
        [SerializeField] private float _maxSanity = 100f;

        [Header("Arcane 체류 시 드레인/초")]
        [SerializeField] private float _normalDrainPerSecond = 4f;
        [SerializeField] private float _hardDrainPerSecond = 8f;

        [Header("Physical 복귀 시 회복/초")]
        [SerializeField] private float _normalRecoverPerSecond = 10f;
        [SerializeField] private float _hardRecoverPerSecond = 4f;

        private float _currentSanity;
        private bool _isDepleted;

        public float CurrentSanity => _currentSanity;
        public float MaxSanity => _maxSanity;

        public event Action<float, float> OnSanityChanged;
        public event Action OnSanityDepleted;

        private void Awake() => _currentSanity = _maxSanity;

        private void Update()
        {
            if (_isDepleted || MazeLayerManager.Instance == null) return;

            bool isHard = GameManager.CurrentGameMode == GameMode.Hard;
            bool inArcane = MazeLayerManager.Instance.CurrentLayer == MazeLayerManager.LayerType.Arcane;

            float rate = inArcane
                ? (isHard ? -_hardDrainPerSecond : -_normalDrainPerSecond)
                : (isHard ? _hardRecoverPerSecond : _normalRecoverPerSecond);

            float next = Mathf.Clamp(_currentSanity + rate * Time.deltaTime, 0f, _maxSanity);
            if (Mathf.Approximately(next, _currentSanity)) return;

            _currentSanity = next;
            OnSanityChanged?.Invoke(_currentSanity, _maxSanity);

            if (_currentSanity <= 0f)
            {
                _isDepleted = true;
                OnSanityDepleted?.Invoke();
            }
        }
    }
}
