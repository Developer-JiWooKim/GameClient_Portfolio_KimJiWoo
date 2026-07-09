using Assets.MyAssets.Scripts.Player;
using Assets.MyAssets.Scripts.Utility.Core;
using Assets.MyAssets.Scripts.Utility.Maze;
using Assets.MyAssets.Scripts.Utility.SingleTon;
using UnityEngine;

namespace Assets.MyAssets.Scripts.Utility.Spawners
{
    /// <summary>
    /// PlayerSpawner/MonsterSpawner/KeySpawner/GoalPointSpawner를 정해진 순서로 호출하는 코디네이터.
    /// 몬스터 스폰은 플레이어의 Transform을 타겟으로 필요로 하므로, 반드시 플레이어 스폰 이후에 실행돼야 함
    /// </summary>
    public class UnitsSpawner : MonoBehaviour
    {
        [Header("Maze Layer Manager")]
        [SerializeField] private MazeLayerManager _mazeLayerManager;

        [Header("Spawners")]
        [SerializeField] private PlayerSpawner _playerSpawner;
        [SerializeField] private MonsterSpawner _monsterSpawner;
        [SerializeField] private KeySpawner _keySpawner;
        [SerializeField] private GoalPointSpawner _goalPointSpawner;

        private float _spawnY = 1f; // 유닛 스폰 y 좌표

        private Vector2Int _playerStartCell; // 플레이어 시작 셀
        private Vector2Int _goalCell;        // 목표 지점 셀

        private MazeGenerator _activeMaze;

        public PlayerController Player => _playerSpawner.Player;

        /// <summary>
        /// 인트로 카메라에서 팔로우 카메라로 전환하는 메소드 (플레이어 인트로 애니메이션이 끝난 뒤 호출됨)
        /// </summary>
        public Awaitable SwitchToFollowCameraAsync() => _playerSpawner.SwitchToFollowCameraAsync();

        private void Initialize()
        {
            _activeMaze = _mazeLayerManager.GetActiveMaze();
            if (_activeMaze == null)
            {
                Debug.LogError("UnitsSpawner Initialize(): _activeMaze is null");
            }

            _playerStartCell = new Vector2Int(0, 0);

            _goalCell = new Vector2Int(_activeMaze.Cols - 1, _activeMaze.Rows - 1);
        }

        /// <summary>
        /// 몬스터 수 설정 메소드
        /// </summary>
        public void SetMonsterCount(int count)
        {
            _monsterSpawner.SetMonsterCount(count);
        }

        private bool CheckSerializeFieldIsNull()
        {
            bool hasNull = false;

            // 현재 내 스크립트의 필드들 전수 조사 (널이 있으면 로그를 찍고 true로 잠금)
            if (_mazeLayerManager == null) { Debug.LogError("UnitsSpawner: _mazeLayerManager is null"); hasNull = true; }
            if (_playerSpawner == null) { Debug.LogError("UnitsSpawner: _playerSpawner is null"); hasNull = true; }
            if (_monsterSpawner == null) { Debug.LogError("UnitsSpawner: _monsterSpawner is null"); hasNull = true; }
            if (_keySpawner == null) { Debug.LogError("UnitsSpawner: _keySpawner is null"); hasNull = true; }
            if (_goalPointSpawner == null) { Debug.LogError("UnitsSpawner: _goalPointSpawner is null"); hasNull = true; }

            if (hasNull) return true;

            hasNull |= _playerSpawner.CheckSerializeFieldIsNull();
            hasNull |= _monsterSpawner.CheckSerializeFieldIsNull();
            hasNull |= _keySpawner.CheckSerializeFieldIsNull();
            hasNull |= _goalPointSpawner.CheckSerializeFieldIsNull();

            return hasNull;
        }

        /// <summary>
        /// 유닛들을 미로의 랜덤한 위치에 스폰하는 메소드 (플레이어 -> 몬스터/키 순서 고정)
        /// </summary>
        public void SpawnAll()
        {
            // SerializeField에서 null이 하나라도 있으면 Spawn X
            if (CheckSerializeFieldIsNull()) return;

            ClearAll();
            Initialize();
            SpawnPlayer();

            // 몬스터는 플레이어의 Transform을 타겟으로 필요로 하므로 반드시 플레이어 스폰 이후에 실행
            _monsterSpawner.SpawnMonsters(_activeMaze, _spawnY, Player.transform, _playerStartCell, _goalCell);
            _keySpawner.SpawnKeys(_activeMaze, _spawnY, _playerStartCell, _goalCell);
            _goalPointSpawner.SetSpawnContext(_activeMaze, _goalCell, _spawnY);

            if (GameManager.Instance == null)
            {
                Debug.LogError("UnitsSpawner SpawnAll(): GameManager.Instance is Null");
                return;
            }

            GameRule gameRule = GameManager.Instance.GameRule;
            gameRule.OnAllKeysCollected += _goalPointSpawner.SpawnGoalPoint; // 열쇠를 전부 모으면 골 포인트를 생성하도록 구독

            // 클리어/게임오버 시 몬스터가 플레이어를 계속 쫓지 않도록, MonsterSpawner가 직접 타겟을 끊음
            gameRule.OnClear += _monsterSpawner.StopAllMonsters;
            gameRule.OnGameOver += _monsterSpawner.StopAllMonsters;

            if (_mazeLayerManager.FogWarSystem == null)
            {
                { Debug.LogError("UnitsSpawner: _mazeLayerManager.FogWarSystem is Null"); return; }
            }

            FischlWorks_FogWar.csFogWar fogWar = _mazeLayerManager.FogWarSystem;

            // 실시간 생성된 초기 미로 벽을 감지하도록 첫 스캔 실행
            fogWar.ScanLevel();

            // 스폰된 플레이어에게 안개 시스템을 넘겨주며 시야를 킴
            Player.RegisterToFogSystem(fogWar);

            // 재시작(Replay) 시 MazeLayerManager.ResetFogMemory()로 fogField 자체는 이미 Hidden으로 초기화되지만,
            // 실제로 화면에 그려지는 안개 텍스처(fogPlaneTextureLerpBuffer)는 Update()의 갱신 주기/보간(lerp)을 통해
            // 서서히 따라가므로, 그동안 이전 판에서 밝혔던 부분이 잠시(혹은 갱신 조건에 따라 계속) 남아 보일 수 있음
            // ForceUpdateFog()로 새 플레이어 기준 안개를 즉시 재계산하고 버퍼에 그대로 복사해, 재시작 시에도
            // 처음 시작할 때와 동일하게 안개가 즉시 리셋된 상태로 보이게 함
            fogWar.ForceUpdateFog();
        }

        private void SpawnPlayer()
        {
            Vector3 spawnPos = _activeMaze.GetCell(_playerStartCell.x, _playerStartCell.y).worldCenter;
            spawnPos.y = _spawnY;

            _playerSpawner.Spawn(spawnPos, _mazeLayerManager);
        }

        /// <summary>
        /// 초기화 메소드(재시작 전 기존 오브젝트/이벤트 구독 정리)
        /// </summary>
        private void ClearAll()
        {
            GameRule gameRule = GameManager.Instance.GameRule;
            gameRule.OnAllKeysCollected -= _goalPointSpawner.SpawnGoalPoint;
            gameRule.OnClear -= _monsterSpawner.StopAllMonsters;
            gameRule.OnGameOver -= _monsterSpawner.StopAllMonsters;

            _playerSpawner.Despawn();
            _monsterSpawner.ReleaseAll();
            _keySpawner.ReleaseAll();
            _goalPointSpawner.ReleaseAll();
        }
    }
}
