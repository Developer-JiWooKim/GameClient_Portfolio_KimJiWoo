using System.Collections.Generic;
using Assets.MyAssets.Scripts.Utility.Maze;
using Assets.MyAssets.Scripts.Utility.TriggerEvent;
using UnityEngine;
using UnityEngine.Pool;

namespace Assets.MyAssets.Scripts.Utility.Spawners
{
    /// <summary>
    /// 키 오브젝트 풀 관리, 스폰, 획득 이벤트 처리를 전담하는 스포너
    /// </summary>
    public class KeySpawner : MonoBehaviour
    {
        [SerializeField] private GameObject _keyPrefab;

        private int _keyCount = 5;

        private readonly List<GameObject> _spawnedKeys = new List<GameObject>();

        private ObjectPool<GameObject> _keyPool;

        private void Awake()
        {
            _keyPool = new ObjectPool<GameObject>(
                createFunc: () => Instantiate(_keyPrefab),
                actionOnGet: obj => obj.SetActive(true),
                actionOnRelease: obj => obj.SetActive(false),
                actionOnDestroy: obj => Destroy(obj),
                collectionCheck: true,
                defaultCapacity: _keyCount,
                maxSize: 20);
        }

        public bool CheckSerializeFieldIsNull()
        {
            if (_keyPrefab == null)
            {
                Debug.LogError("KeySpawner: _keyPrefab is null");
                return true;
            }

            return false;
        }

        /// <summary>
        /// 시작/목표 셀을 제외한 랜덤 셀에 열쇠를 배치하는 메소드.
        /// 레이어 간 셀 좌표(worldCenter)는 공유되므로, 활성 레이어 기준 셀 정보만 읽어와도 충분함.
        /// </summary>
        public void SpawnKeys(MazeGenerator activeMaze, float spawnY, params Vector2Int[] excludeCells)
        {
            List<Cell> candidates = activeMaze.GetShuffledCandidateCells(excludeCells);

            int spawnCount = Mathf.Min(_keyCount, candidates.Count);

            for (int i = 0; i < spawnCount; i++)
            {
                Vector3 spawnPos = candidates[i].worldCenter;
                spawnPos.y = spawnY;

                GameObject key = _keyPool.Get();
                key.transform.SetPositionAndRotation(spawnPos, Quaternion.identity);

                if (key.TryGetComponent(out Key keyComponent))
                {
                    keyComponent.OnCollected += HandleKeyCollected;
                }

                _spawnedKeys.Add(key);
            }
        }

        /// <summary>
        /// 열쇠 획득 시 호출, 구독 해제 후 풀에 반납하는 메소드
        /// </summary>
        private void HandleKeyCollected(Key key)
        {
            key.OnCollected -= HandleKeyCollected;

            _spawnedKeys.Remove(key.gameObject);
            _keyPool.Release(key.gameObject);
        }

        /// <summary>
        /// 재시작 시 남은 열쇠 전부의 구독을 해제한 뒤 풀에 반납하는 메소드
        /// </summary>
        public void ReleaseAll()
        {
            foreach (GameObject key in _spawnedKeys)
            {
                if (key == null) continue;

                // 획득되지 않고 남은 열쇠는 구독을 끊고 Pool에 반납
                if (key.TryGetComponent(out Key keyComponent))
                {
                    keyComponent.OnCollected -= HandleKeyCollected;
                }

                _keyPool.Release(key);
            }
            _spawnedKeys.Clear();
        }
    }
}
