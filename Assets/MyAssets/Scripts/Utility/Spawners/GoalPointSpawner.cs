using Assets.MyAssets.Scripts.Utility.Maze;
using Assets.MyAssets.Scripts.Utility.SingleTon;
using UnityEngine;

namespace Assets.MyAssets.Scripts.Utility.Spawners
{
    /// <summary>
    /// 골 포인트 스폰(열쇠를 전부 모으면 1회 생성)을 전담하는 스포너
    /// </summary>
    public class GoalPointSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject _goalPointPrefab;

        private GameObject _goalPointInstance;

        private MazeGenerator _activeMaze;
        private Vector2Int _goalCell;
        private float _spawnY;

        public bool CheckSerializeFieldIsNull()
        {
            if (_goalPointPrefab == null)
            {
                Debug.LogError("GoalPointSpawner: _goalPointPrefab is null");
                return true;
            }

            return false;
        }

        /// <summary>
        /// 열쇠를 전부 모았을 때(OnAllKeysCollected) SpawnGoalPoint()가 사용할 위치 정보를 미리 저장하는 메소드
        /// </summary>
        public void SetSpawnContext(MazeGenerator activeMaze, Vector2Int goalCell, float spawnY)
        {
            _activeMaze = activeMaze;
            _goalCell = goalCell;
            _spawnY = spawnY;
        }

        /// <summary>
        /// 골 포인트를 1개만 생성하는 메소드 (열쇠를 전부 모으면 GameRule.OnAllKeysCollected로 호출)
        /// </summary>
        public void SpawnGoalPoint()
        {
            if (_goalPointInstance != null)
            {
                Debug.LogError("GoalPointSpawner SpawnGoalPoint(): _goalPointInstance != null");
                return;
            }

            Vector3 spawnPos = _activeMaze.GetCell(_goalCell.x, _goalCell.y).worldCenter;
            spawnPos.y = _spawnY;

            _goalPointInstance = Instantiate(_goalPointPrefab, spawnPos, Quaternion.identity);

            SoundManager.Instance?.PlayGoalSpawned();
        }

        /// <summary>
        /// 재시작 시 기존 골 포인트를 제거하는 메소드
        /// </summary>
        public void ReleaseAll()
        {
            if (_goalPointInstance != null)
            {
                Destroy(_goalPointInstance);
                _goalPointInstance = null;
            }
        }
    }
}
