using Assets.MyAssets.Scripts.Player;
using Assets.MyAssets.Scripts.Utility.SingleTon;
using Assets.MyAssets.Scripts.Utility.Visuals;
using UnityEngine;

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

        public PlayerController Player { get; private set; } // => _playerInstance?.GetComponent<PlayerController>();

        public bool CheckSerializeFieldIsNull()
        {
            bool hasNull = false;

            if (_playerPrefab == null) { Debug.LogError("PlayerSpawner: _playerPrefab is null"); hasNull = true; }
            if (_introCameraSequencer == null) { Debug.LogError("PlayerSpawner: _introCameraSequencer is null"); hasNull = true; }

            return hasNull;
        }

        /// <summary>
        /// 플레이어를 스폰하고 인트로 카메라, 레이어 전환 입력을 연결하는 메소드
        /// </summary>
        public void Spawn(Vector3 spawnPos, MazeLayerManager mazeLayerManager)
        {
            _playerInstance = Instantiate(_playerPrefab, spawnPos, Quaternion.identity);

            // 인트로 카메라로 즉시 컷 - 팔로우 카메라로의 전환은 플레이어 인트로 애니메이션이 끝난 뒤
            // SwitchToFollowCameraAsync()를 통해 별도로 트리거됨
            _introCameraSequencer.CutToIntroCamera(_playerInstance.transform);

            // 플레이어의 입력(Tab) 시 미로를 전환하는 이벤트를 연결
            if (_playerInstance.TryGetComponent(out PlayerInputHandler playerInput))
            {
                mazeLayerManager?.RegisterPlayerInput(playerInput);
            }

            // 생성한 플레이어 인스턴스의 PlayerController 컴포넌트를 Player 프로퍼티에 연결
            if (_playerInstance.TryGetComponent<PlayerController>(out PlayerController playerController))
            {
                Player = playerController;
            }
        }

        /// <summary>
        /// 인트로 카메라에서 팔로우 카메라로 전환하는 메소드 (플레이어 인트로 애니메이션이 끝난 뒤 호출됨)
        /// </summary>
        public Awaitable SwitchToFollowCameraAsync() => _introCameraSequencer.SwitchToFollowCamera();

        /// <summary>
        /// 재시작 시 이전 플레이어를 파괴하는 메소드
        /// </summary>
        public void Despawn()
        {
            if (_playerInstance == null) return;

            // 안개 시스템에 리빌러가 계속 쌓이지 않도록 파괴 전에 해제
            Player.UnregisterFromFogSystem();

            Destroy(_playerInstance);
            _playerInstance = null;
            Player = null;
        }
    }
}
