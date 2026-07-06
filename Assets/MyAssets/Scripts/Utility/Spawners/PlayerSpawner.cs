using UnityEngine;
using Assets.MyAssets.Scripts.Player;
using Assets.MyAssets.Scripts.Utility.SingleTon;
using Assets.MyAssets.Scripts.Utility.Visuals;

namespace Assets.MyAssets.Scripts.Utility.Spawners
{
    /// <summary>
    /// 플레이어 스폰/디스폰, 인트로 카메라, 레이어 전환 입력 연결을 전담하는 스포너
    /// </summary>
    public class PlayerSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private IntroCameraSequencer _introCameraSequencer;

        private GameObject _playerInstance;

        public PlayerController Player => _playerInstance?.GetComponent<PlayerController>();

        public bool CheckSerializeFieldIsNull()
        {
            bool hasNull = false;

            if (_playerPrefab == null) { Debug.LogError("PlayerSpawner: _playerPrefab이 null임"); hasNull = true; }
            if (_introCameraSequencer == null) { Debug.LogError("PlayerSpawner: _introCameraSequencer가 null임"); hasNull = true; }

            return hasNull;
        }

        /// <summary>
        /// 플레이어를 스폰하고 인트로 카메라, 레이어 전환 입력을 연결하는 메소드
        /// </summary>
        public void Spawn(Vector3 spawnPos, MazeLayerManager mazeLayerManager)
        {
            _playerInstance = Instantiate(_playerPrefab, spawnPos, Quaternion.identity);

            // 인트로->팔로우 카메라 전환 연출은 IntroCameraSequencer가 전담
            _introCameraSequencer.PlayIntro(_playerInstance.transform);

            // 플레이어의 입력(Tab) 시 미로를 전환하는 이벤트를 연결
            if (_playerInstance.TryGetComponent(out PlayerInputHandler playerInput))
            {
                mazeLayerManager?.RegisterPlayerInput(playerInput);
            }
        }

        /// <summary>
        /// 재시작 시 이전 플레이어를 파괴하는 메소드
        /// </summary>
        public void Despawn()
        {
            if (_playerInstance == null) return;

            // 안개 시스템에 리빌러가 계속 쌓이지 않도록 파괴 전에 해제
            if (_playerInstance.TryGetComponent(out PlayerController pc))
            {
                pc.UnregisterFromFogSystem();
            }

            Destroy(_playerInstance);
            _playerInstance = null;
        }
    }
}
