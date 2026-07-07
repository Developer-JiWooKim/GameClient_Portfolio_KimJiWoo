using Assets.MyAssets.Scripts.Player;
using Assets.MyAssets.Scripts.Utility.Maze;
using Assets.MyAssets.Scripts.Utility.Visuals;
using FischlWorks_FogWar;
using Unity.AI.Navigation;
using UnityEngine;

namespace Assets.MyAssets.Scripts.Utility.SingleTon
{
    public class MazeLayerManager : Singleton<MazeLayerManager>
    {
        public enum LayerType
        {
            Physical,
            Arcane,
        }

        [Header("Maze Generators")]
        [SerializeField] private MazeGenerator _physicalMaze;
        [SerializeField] private MazeGenerator _arcaneMaze;

        [Header("Layer Switch Check")]
        [SerializeField] private float _overlapCheckRadius = 0.5f; // 전환 시 플레이어 위치에서 벽과의 겹침을 검사할 반경

        [Header("Fog War System")]
        [SerializeField] private csFogWar _fogWarSystem;

        [Header("NavMesh Surface")]
        [SerializeField] private NavMeshSurface _physicalNavMeshSurface;
        [SerializeField] private NavMeshSurface _arcaneNavMeshSurface;

        [Header("Layer Transition FX")]
        [SerializeField] private ScreenRippleController _rippleController;
        [SerializeField] private float _rippleInDuration = 0.1f;
        [SerializeField] private float _rippleHoldDuration = 0.05f; // 일렁임이 최고조일 때 실제로 미로를 바꿔치기하는 구간
        [SerializeField] private float _rippleOutDuration = 0.1f;

        private bool _isTransitioning = false;
        public bool IsTransitioning => _isTransitioning; // 전환 연출 중 일시정지 등 다른 흐름 진입을 막기 위한 상태 노출

        private int _physicalWallMask;
        private int _physicalSeed;
        private int _arcaneWallMask;
        private int _arcaneSeed;

        private int _currentWallLayerMask;
        public int CurrentWallLayerMask => _currentWallLayerMask;

        public csFogWar FogWarSystem => _fogWarSystem;

        private PlayerInputHandler _playerInputHandler;

        private LayerType _currentLayer = LayerType.Physical;
        public LayerType CurrentLayer => _currentLayer;

        public event System.Action<LayerType> OnLayerChanged;
        public event System.Action OnLayerSwitchBlocked; // 전환 실패(벽에 끼임) 시 호출, 사운드/전환 불가 UI 혹은 화면 쉐이킹?

        protected override void Awake()
        {
            base.Awake();
            if (!IsValidInstance) return; // 중복 인스턴스는 base.Awake()가 파괴 처리하므로 초기화 생략

            _physicalWallMask = LayerMask.GetMask("Wall_Physical");
            _arcaneWallMask = LayerMask.GetMask("Wall_Arcane");
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_playerInputHandler != null)
            {
                _playerInputHandler.OnLayerSwitchRequested -= SwitchLayer;
            }
        }

        public void RandomSeed()
        {
            _physicalSeed = Random.Range(0, 1100);
            _arcaneSeed = Random.Range(1100, 2200);
        }

        /// <summary>
        /// 미로 생성 후 레이어 설정. 미로 크기는 각 MazeGenerator의 SerializeField(_cols/_rows)에 고정으로 설정됨
        /// </summary>
        public void SetLayersAndMazeGenerate()
        {
            RandomSeed();

            ResetFogMemory();

            _physicalMaze.SetSeed(_physicalSeed);
            _physicalMaze.Generate();

            Physics.SyncTransforms();
            _physicalNavMeshSurface.BuildNavMesh(); // Physical 전용 NavMeshSurface Bake

            _arcaneMaze.SetSeed(_arcaneSeed);
            _arcaneMaze.Generate();

            Physics.SyncTransforms();
            _arcaneNavMeshSurface.BuildNavMesh(); // Arcane 전용 NavMeshSurface Bake

            SetActiveLayer(LayerType.Physical);
        }

        /// <summary>
        /// 런타임에 스폰된 플레이어의 PlayerInput을 등록, 레이어 전환 이벤트 구독
        /// </summary>
        public void RegisterPlayerInput(PlayerInputHandler playerInputHandler)
        {
            if (_playerInputHandler != null)
            {
                _playerInputHandler.OnLayerSwitchRequested -= SwitchLayer;
            }

            _playerInputHandler = playerInputHandler;
            _playerInputHandler.OnLayerSwitchRequested += SwitchLayer;
        }

        private void SwitchLayer(Vector3 playerPosition)
        {
            if (_isTransitioning) return; // 전환 연출 중 또 전환 연출하는걸 방지

            LayerType targetLayer = _currentLayer == LayerType.Physical ? LayerType.Arcane : LayerType.Physical;

            int targetWallMask = targetLayer == LayerType.Physical ? _physicalWallMask : _arcaneWallMask;

            bool isBlocked = Physics.CheckSphere(playerPosition, _overlapCheckRadius, targetWallMask);

            if (isBlocked)
            {
                OnLayerSwitchBlocked?.Invoke();
                return;
            }

            _ = PlayLayerTransition(targetLayer);
        }

        /// <summary>
        /// 타이머/입력/유닛 움직임을 멈추고 화면 일렁임 효과 안에서 실제 미로를 교체한 뒤 다시 재생시키는 시퀀스
        /// </summary>
        private async Awaitable PlayLayerTransition(LayerType targetLayer)
        {
            if (_playerInputHandler == null)
            {
                Debug.LogError("MazeLayerManager PlayLayerTransition(): _playerInputHandler is Null");
                return;
            }

            _isTransitioning = true;

            try
            {
                SoundManager.Instance?.PlayLayerSwitch();

                GameManager.Instance.PauseGame();

                // PlayerInputHandler.OnDisable()에서 입력값도 같이 초기화됨
                _playerInputHandler.enabled = false;

                await FadeRipple(0f, 1f, _rippleInDuration);
                await WaitUnscaled(_rippleHoldDuration);

                SetActiveLayer(targetLayer); // 일렁임이 화면을 가리는 동안 레이어 교체

                await FadeRipple(1f, 0f, _rippleOutDuration);

            }
            catch (System.OperationCanceledException oce)
            {
                Debug.LogException(oce);
            }
            finally
            {
                // 예외가 나도(또는 정상 취소돼도) 입력/시간이 영원히 멈춰있지 않도록 무조건 복구
                _playerInputHandler.enabled = true;

                GameManager.Instance.ResumeGame();

                _isTransitioning = false;
            }
        }

        /// <summary>
        /// 일렁임 강도를 duration 동안 from -> to로 보간 (unscaled time 기준)
        /// </summary>
        private async Awaitable FadeRipple(float from, float to, float duration)
        {
            try
            {
                float elapsed = 0f;

                while (elapsed < duration)
                {
                    elapsed += Time.unscaledDeltaTime;

                    _rippleController.SetIntensity(Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration)));

                    await Awaitable.NextFrameAsync(destroyCancellationToken);
                }

                _rippleController.SetIntensity(to);
            }
            catch (System.OperationCanceledException oce)
            {
                Debug.LogException(oce);
            }
        }

        /// <summary>
        /// Time.timeScale과 무관하게 실제 시간 기준으로 대기
        /// </summary>
        private async Awaitable WaitUnscaled(float duration)
        {
            try
            {
                float elapsed = 0f;

                while (elapsed < duration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    await Awaitable.NextFrameAsync(destroyCancellationToken);
                }
            }
            catch (System.OperationCanceledException oce)
            {
                Debug.LogException(oce);
            }
        }

        private void SetActiveLayer(LayerType layer)
        {
            _currentLayer = layer;

            bool physicalActive = layer == LayerType.Physical;

            // 현재 활성화된 레이어에 따라 벽 레이어 마스크 변경
            _currentWallLayerMask = physicalActive ? _physicalWallMask : _arcaneWallMask;

            _physicalMaze.SetWallsActiveState(physicalActive);
            _arcaneMaze.SetWallsActiveState(!physicalActive);

            if (physicalActive)
            {
                _physicalNavMeshSurface.AddData();
                _arcaneNavMeshSurface.RemoveData();
            }
            else
            {
                _arcaneNavMeshSurface.AddData();
                _physicalNavMeshSurface.RemoveData();
            }

            if (_fogWarSystem != null)
            {
                _fogWarSystem.ScanLevel();
            }

            OnLayerChanged?.Invoke(layer);
        }

        /// <summary>
        /// 현재 활성화된 레이어의 MazeGenerator 반환
        /// </summary>
        public MazeGenerator GetActiveMaze()
        {
            return _currentLayer == LayerType.Physical ? _physicalMaze : _arcaneMaze;
        }

        /// <summary>
        /// FogWar의 타일별 시야 기억(Revealed/PreviouslyRevealed)을 전부 Hidden으로 되돌리는 메소드
        /// </summary>
        private void ResetFogMemory()
        {
            if (_fogWarSystem == null) return;

            Shadowcaster.FogField fogField = _fogWarSystem.shadowcaster.fogField;

            for (int x = 0; x < _fogWarSystem.levelData.levelDimensionX; x++)
            {
                Shadowcaster.LevelColumn column = fogField[x];

                for (int y = 0; y < column.Count(); y++)
                {
                    column[y] = Shadowcaster.LevelColumn.ETileVisibility.Hidden;
                }
            }
        }
    }
}
