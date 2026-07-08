using System.Collections.Generic;
using Assets.MyAssets.Scripts.Monster;
using Assets.MyAssets.Scripts.Utility.Maze;
using Assets.MyAssets.Scripts.Utility.SingleTon;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Pool;

namespace Assets.MyAssets.Scripts.Utility.Spawners
{
    /// <summary>
    /// 몬스터 오브젝트 풀 관리, 스폰, 레이어 전환 시 재동기화, 클리어/게임오버 시 타겟 해제를 전담하는 스포너
    /// </summary>
    public class MonsterSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject _monsterPrefab;
        [SerializeField] private MazeLayerManager _mazeLayerManager;

        private int _monsterCount = 10;

        private readonly List<GameObject> _monsters = new List<GameObject>();
        public List<GameObject> Monsters => _monsters;

        private ObjectPool<GameObject> _monsterPool;

        private void Awake()
        {
            _monsterPool = new ObjectPool<GameObject>(
                createFunc: () => Instantiate(_monsterPrefab),
                actionOnGet: obj => obj.SetActive(true),
                actionOnRelease: obj => obj.SetActive(false),
                actionOnDestroy: obj => Destroy(obj),
                collectionCheck: true,
                defaultCapacity: _monsterCount,
                maxSize: 50);

            if (_mazeLayerManager != null)
            {
                _mazeLayerManager.OnLayerChanged += HandleLayerChanged;
            }
        }

        private void OnDestroy()
        {
            if (_mazeLayerManager != null)
            {
                _mazeLayerManager.OnLayerChanged -= HandleLayerChanged;
            }
        }

        /// <summary>
        /// 레이어 전환 시 두 미로의 벽 배치가 서로 달라 몬스터가 서 있던 자리가 새 레이어에서는 벽 안일 수 있음
        /// NavMeshAgent가 이전 레이어의 NavMesh에 남은 상태로 이동을 계속하면 벽을 뚫고 다니게 되므로,
        /// 전환 직후 제자리에서 Warp를 다시 걸어 새로 활성화된 NavMesh에 강제로 재동기화시킴
        /// </summary>
        private void HandleLayerChanged(MazeLayerManager.LayerType layer)
        {
            foreach (GameObject monster in _monsters)
            {
                if (monster == null || !monster.activeInHierarchy) continue;

                if (monster.TryGetComponent(out MonsterController mc))
                {
                    mc.Move.Warp(mc.transform.position);
                }
            }
        }

        /// <summary>
        /// 몬스터 수 설정 메소드
        /// </summary>
        public void SetMonsterCount(int count)
        {
            _monsterCount = count;
        }

        public bool CheckSerializeFieldIsNull()
        {
            bool hasNull = false;

            if (_monsterPrefab == null) { Debug.LogError("MonsterSpawner: _monsterPrefab is null"); hasNull = true; }
            if (_mazeLayerManager == null) { Debug.LogError("MonsterSpawner: _mazeLayerManager is null"); hasNull = true; }

            return hasNull;
        }

        /// <summary>
        /// 몬스터들을 미로의 랜덤한 위치에 스폰하는 메소드
        /// </summary>
        public void SpawnMonsters(MazeGenerator activeMaze, float spawnY, Transform target, params Vector2Int[] excludeCells)
        {
            List<Cell> candidates = activeMaze.GetShuffledCandidateCells(excludeCells);

            int spawnCount = Mathf.Min(_monsterCount, candidates.Count);

            // _monsterCount 수만큼 몬스터 스폰
            for (int i = 0; i < spawnCount; i++)
            {
                // Cell.worldCenter로 몬스터 스폰해서 위치가 벽과 겹치지 않게
                Vector3 spawnPos = candidates[i].worldCenter;
                spawnPos.y = spawnY;

                GameObject monster = _monsterPool.Get();
                monster.transform.rotation = Quaternion.identity;

                if (monster.TryGetComponent(out MonsterController mc))
                {
                    mc.ResetForReuse(spawnPos, target);
                }

                if (monster.TryGetComponent(out NavMeshAgent agent))
                {
                    agent.avoidancePriority = Random.Range(0, 99);
                }

                // 스폰한 몬스터들을 리스트에 추가
                _monsters.Add(monster);
            }
        }

        /// <summary>
        /// 클리어/게임오버 시 호출(GameRule.OnClear/OnGameOver 구독), 살아있는 몬스터 전부의 타겟을 끊어
        /// 결과 화면이 뜬 뒤에도 계속 쫓아오지 않게 함
        /// </summary>
        public void StopAllMonsters()
        {
            foreach (GameObject monster in _monsters)
            {
                if (monster == null) continue;

                if (monster.TryGetComponent(out MonsterController mc))
                {
                    mc.ClearTarget();
                }
            }
        }

        /// <summary>
        /// 재시작 시 몬스터 전부의 타겟을 해제한 뒤 풀에 반납하는 메소드
        /// </summary>
        public void ReleaseAll()
        {
            foreach (GameObject mon in _monsters)
            {
                if (mon == null) continue;

                if (mon.TryGetComponent(out MonsterController mc))
                {
                    mc.ClearTarget();
                }

                _monsterPool.Release(mon);
            }
            _monsters.Clear();
        }
    }
}
