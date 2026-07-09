using Assets.MyAssets.Scripts.Player;
using Assets.MyAssets.Scripts.Utility.Maze;
using Assets.MyAssets.Scripts.Utility.Visuals;
using FischlWorks_FogWar;
using Unity.AI.Navigation;
using UnityEngine;

namespace Assets.MyAssets.Scripts.Utility.SingleTon
{
    [RequireComponent(typeof(PhysicalWallGrimeCycler))]
    [RequireComponent(typeof(HardModeIdleWaver))]
    [RequireComponent(typeof(LayerTransitionSequencer))]
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
        [SerializeField] private float _overlapCheckRadius = 0.3f; // 전환 시 플레이어 위치에서 벽과의 겹침을 검사할 반경

        [Header("Fog War System")]
        [SerializeField] private csFogWar _fogWarSystem;

        [Header("NavMesh Surface")]
        [SerializeField] private NavMeshSurface _physicalNavMeshSurface;
        [SerializeField] private NavMeshSurface _arcaneNavMeshSurface;

        private PhysicalWallGrimeCycler _grimeCycler;
        private HardModeIdleWaver _idleWaver;
        private LayerTransitionSequencer _transitionSequencer;

        public bool IsTransitioning => _transitionSequencer.IsTransitioning; // 전환 연출 중 일시정지 등 다른 흐름 진입을 막기 위한 상태 노출

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
        public event System.Action OnLayerSwitchBlocked; // 전환 실패(벽에 끼임) 시 호출, 사운드/전환 불가 UI, 화면 쉐이킹

        protected override void Awake()
        {
            base.Awake();
            if (!IsValidInstance) return;

            _physicalWallMask = LayerMask.GetMask("Wall_Physical");
            _arcaneWallMask = LayerMask.GetMask("Wall_Arcane");

            _grimeCycler = GetComponent<PhysicalWallGrimeCycler>();
            _idleWaver = GetComponent<HardModeIdleWaver>();
            _transitionSequencer = GetComponent<LayerTransitionSequencer>();
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

            _grimeCycler.RestartCycle();
            _idleWaver.RestartCycle();
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
            if (_transitionSequencer.IsTransitioning) return; // 전환 연출 중 또 전환 연출하는걸 방지

            LayerType targetLayer = _currentLayer == LayerType.Physical ? LayerType.Arcane : LayerType.Physical;

            int targetWallMask = targetLayer == LayerType.Physical ? _physicalWallMask : _arcaneWallMask;

            bool isBlocked = Physics.CheckSphere(playerPosition, _overlapCheckRadius, targetWallMask);

            if (isBlocked)
            {
                OnLayerSwitchBlocked?.Invoke();
                return;
            }

            _ = _transitionSequencer.PlayTransition(_playerInputHandler, () => SetActiveLayer(targetLayer));
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
