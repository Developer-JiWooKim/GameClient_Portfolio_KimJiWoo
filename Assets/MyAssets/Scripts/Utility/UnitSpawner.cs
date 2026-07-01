using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Pool;

public class UnitSpawner : MonoBehaviour
{
    [Header("Maze Layer Manager")]
    [SerializeField] private MazeLayerManager _mazeLayerManager;

    [Header("Cinemachine Cameras")]
    [SerializeField] private CinemachineCamera _followPlayerCamera;
    [SerializeField] private CinemachineCamera _introCamera;

    [Header("Prefabs")]
    [SerializeField] private GameObject _monsterPrefab;
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private GameObject _keyPrefab;
    [SerializeField] private GameObject _goalPointPrefab;

    private int _monsterCount = 10;
    private int _keyCount     = 5;

    private float _spawnY            = 1f;   // 유닛 스폰 y 좌표
    private float _introHoldDuration = 0.5f; // 씬 재시작 시 안정적인 카메라 전환을 위해 기다릴 시간

    private Vector2Int _playerStartCell; // 플레이어 시작 셀
    private Vector2Int _goalCell;        // 목표 지점 셀    

    private GameObject _player;
    private GameObject _goalPointInstance;

    private MazeGenerator _activeMaze;

    private readonly List<GameObject> _spawnedKeys = new List<GameObject>();

    public PlayerController Player => _player?.GetComponent<PlayerController>();

    private List<GameObject> _monsters = new List<GameObject>();
    public List<GameObject> Monsters => _monsters;

    private ObjectPool<GameObject> _monsterPool;
    private ObjectPool<GameObject> _keyPool;

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

        _keyPool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(_keyPrefab),
            actionOnGet: obj => obj.SetActive(true),
            actionOnRelease: obj => obj.SetActive(false),
            actionOnDestroy: obj => Destroy(obj),
            collectionCheck: true,
            defaultCapacity: _keyCount,
            maxSize: 20);
    }

    /// <summary>
    /// 몬스터 수 설정 메소드 TODO: 하드 난이도일때는 이 몬스터 수를 변경할 듯?
    /// </summary>
    public void SetMonsterCount(int count)
    {
        _monsterCount = count;
    }

    private bool CheckSerializeFieldIsNull()
    {
        if (_mazeLayerManager == null || _followPlayerCamera == null ||
            _introCamera == null || _monsterPrefab == null ||
            _playerPrefab == null || _keyPrefab == null || _goalPointPrefab == null)
        {
            Debug.LogError("UnitSpawner CheckSerializeFieldNull():SerializeField 중 Null이 있음");
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 유닛들을 미로의 랜덤한 위치에 스폰하는 메소드
    /// </summary>
    public void SpawnAll()
    {
        // SerializeField에서 null이 하나라도 있으면 Spawn X
        if (CheckSerializeFieldIsNull()) return;

        ClearAll();
        Initialize();
        SpawnPlayer();
        SpawnMonsters();
        SpawnKeys();


        if (GameManager.Instance == null)
        {
            Debug.LogError("UnitSpawner SpawnAll(): GameManager.Instance가 Null임");
            return;
        }
        // 열쇠를 전부 모으면 골 포인트를 생성하도록 구독
        GameManager.Instance.GameRule.OnAllKeysCollected += SpawnGoalPoint;

        if (_mazeLayerManager == null || _mazeLayerManager.FogWarSystem == null || Player == null)
        {
            Debug.LogError("UnitSpawner SpawnAll():Null이 있음");
            return;
        }

        FischlWorks_FogWar.csFogWar fogWar = _mazeLayerManager.FogWarSystem;

        // 실시간 생성된 초기 미로 벽을 감지하도록 첫 스캔 실행
        fogWar.ScanLevel();

        // 스폰된 플레이어에게 안개 시스템을 넘겨주며 시야를 킴
        PlayerController pc = Player;
        pc.RegisterToFogSystem(fogWar);
    }

    private void Initialize()
    {
        _activeMaze = _mazeLayerManager.GetActiveMaze();

        _playerStartCell = new Vector2Int(0, 0);

        _goalCell = new Vector2Int(_activeMaze.Cols - 1, _activeMaze.Rows - 1);
    }

    private void SpawnPlayer()
    {
        Vector3 spawnPos = _activeMaze.GetCell(_playerStartCell.x, _playerStartCell.y).worldCenter;

        spawnPos.y = _spawnY;

        _player = Instantiate(_playerPrefab, spawnPos, Quaternion.identity);

        // 재시작(Replay) 시 이전 판에서 올려둔 팔로우 카메라 우선순위가 남아있으면 인트로 카메라가 안 보이므로,
        // 매번 인트로보다 낮은 값으로 되돌려둔 뒤 SwitchToFollowCameraAfterDelay()에서 다시 올림
        _followPlayerCamera.Priority = _introCamera.Priority - 1;

        _followPlayerCamera.Target.TrackingTarget = _player.transform;

        // 즉시 Priority를 높이면 블렌딩이 생략되는 현상 생김
        // -> Awaitable.WaitForSecondsAsync()로 _introHoldDuration 만큼 기다린 후 _followPlayerCamera의 Priority를 높임
        _ = SwitchToFollowCameraAfterDelay();

        // 플레이어의 입력(Tab) 시 미로를 전환하는 이벤트를 연결
        if (_player.TryGetComponent(out PlayerInputHandler playerInput))
        {
            _mazeLayerManager.RegisterPlayerInput(playerInput);
        }
    }

    /// <summary>
    /// 인트로 카메라를 일정 시간 동안 무조건 보여준 뒤, 추적 카메라의 우선순위를 올려 카메라 전환시키는 메소드
    /// </summary>
    private async Awaitable SwitchToFollowCameraAfterDelay()
    {
        try
        {
            await Awaitable.WaitForSecondsAsync(_introHoldDuration, destroyCancellationToken);

            _followPlayerCamera.Priority = _introCamera.Priority + 1;
        }
        catch (System.OperationCanceledException oce)
        {
            Debug.LogException(oce);
        }
    }

    private void SpawnMonsters()
    {
        List<Cell> candidates = GetCandidatesCellList();

        int spawnCount = Mathf.Min(_monsterCount, candidates.Count);

        // _monsterCount 수만큼 몬스터 스폰
        for (int i = 0; i < spawnCount; i++)
        {
            // Cell.worldCenter로 몬스터 스폰해서 위치가 벽과 겹치지 않게
            Vector3 spawnPos = candidates[i].worldCenter;
            spawnPos.y = _spawnY;

            GameObject monster = _monsterPool.Get();
            monster.transform.rotation = Quaternion.identity;

            if (monster.TryGetComponent(out MonsterController mc))
            {
                mc.ResetForReuse(spawnPos, _player != null ? _player.transform : null);
            }

            if(monster.TryGetComponent(out NavMeshAgent agent))
            {
                agent.avoidancePriority = Random.Range(0, 99);
            }

            // 스폰한 몬스터들을 리스트에 추가
            _monsters.Add(monster);
        }
    }

    /// <summary>
    /// 랜덤 스폰할(시작점, 골 포인트 제외) Candidates 리스트 생성 메소드
    /// </summary>
    private List<Cell> GetCandidatesCellList()
    {
        List<Cell> candidates = new List<Cell>();

        foreach (Cell cell in _activeMaze.AllCells)
        {
            // 플레이어 시작점, 목표 지점에는 생성 X
            if (cell.col == _playerStartCell.x && cell.row == _playerStartCell.y) continue;
            if (cell.col == _goalCell.x && cell.row == _goalCell.y) continue;

            candidates.Add(cell);
        }

        // Fisher-Yates 셔플로 랜덤한 위치 보장
        for (int i = candidates.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
        }

        return candidates;
    }

    /// <summary>
    /// 시작/목표 셀을 제외한 랜덤 셀에 열쇠를 배치하는 메소드.
    /// 레이어 간 셀 좌표(worldCenter)는 공유되므로, 활성 레이어 기준 셀 정보만 읽어와도 충분함.
    /// </summary>
    private void SpawnKeys()
    {
        List<Cell> candidates = GetCandidatesCellList();

        int spawnCount = Mathf.Min(_keyCount, candidates.Count);

        for(int i = 0; i < spawnCount; i++)
        {
            Vector3 spawnPos = candidates[i].worldCenter;
            spawnPos.y = _spawnY;

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
    /// 골 포인트를 1개만 생성하는 메소드 (열쇠를 전부 모으면 GameManager.OnAllKeysCollected로 호출)
    /// </summary>
    private void SpawnGoalPoint()
    {
        if(_goalPointInstance != null)
        {
            Debug.LogError("UnitSpawner SpawnGoalPoint(): _goalPointInstance != null");
            return;
        }

        Vector3 spawnPos = _activeMaze.GetCell(_goalCell.x, _goalCell.y).worldCenter;
        spawnPos.y = _spawnY;

        _goalPointInstance = Instantiate(_goalPointPrefab, spawnPos, Quaternion.identity);

        SoundManager.Instance?.PlayGoalSpawned();
    }

    /// <summary>
    /// 초기화 메소드(리스트, 이벤트 구독 해제, 기존 오브젝트 제거)
    /// </summary>
    private void ClearAll()
    {
        GameManager.Instance.GameRule.OnAllKeysCollected -= SpawnGoalPoint;

        if (_player != null)
        {
            // 안개 시스템에 리빌러가 계속 쌓이지 않도록 파괴 전에 해제
            if (_player.TryGetComponent(out PlayerController pc))
            {
                pc.UnregisterFromFogSystem();
            }

            Destroy(_player);
            _player = null;
        }

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

        foreach (GameObject key in _spawnedKeys)
        {
            if (key == null) continue;

            // 획득되지 않고 남은 열쇠는 구독을 끊고 반납 (재사용 시 중복 구독 방지)
            if (key.TryGetComponent(out Key keyComponent))
            {
                keyComponent.OnCollected -= HandleKeyCollected;
            }

            _keyPool.Release(key);
        }
        _spawnedKeys.Clear();

        if(_goalPointInstance != null)
        {
            Destroy(_goalPointInstance);
            _goalPointInstance = null;
        }
    }
}