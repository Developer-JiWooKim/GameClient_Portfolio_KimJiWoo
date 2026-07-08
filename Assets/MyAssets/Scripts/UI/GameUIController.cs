using Assets.MyAssets.Scripts.Player;
using Assets.MyAssets.Scripts.Utility.Core;
using Assets.MyAssets.Scripts.Utility.SingleTon;
using Assets.MyAssets.Scripts.Utility.Spawners;
using Unity.Cinemachine;
using UnityEngine;

namespace Assets.MyAssets.Scripts.UI
{
    public class GameUIController : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private TitlePanelUI _titlePanel;
        [SerializeField] private SelectPanelUI _selectPanel;
        [SerializeField] private InGamePanelUI _inGamePanel;
        [SerializeField] private PausePanelUI _pausePanel;
        [SerializeField] private OptionsPanelUI _optionsPanel;
        [SerializeField] private ResultPanelUI _resultPanel;

        [Header("Damage Flash FX")]
        [SerializeField] private DamageflashUI _damageflashUI;

        [Header("Layer Switch Blocked FX")]
        [SerializeField] private DamageflashUI _layerSwitchBlockedFlashUI;
        [SerializeField] private CinemachineImpulseSource _layerSwitchBlockedImpulseSource;

        [Header("Setting Monster Count")]
        [SerializeField] private int _monsterCount = 10;

        [Header("참조")]
        [SerializeField] private MazeLayerManager _mazeLayerManager;
        [SerializeField] private UnitsSpawner _unitsSpawner;

        private PlayerController _player;
        public PlayerController Player => _player;

        private GameFlowFSM _flowFsm;

        // 게임 시작 연출(카메라 전환 + 플레이어 인트로 애니메이션) 진행 중 여부
        private bool _isGameStarting;
        public bool IsGameStarting => _isGameStarting;

        public TitlePanelUI TitlePanel => _titlePanel;
        public SelectPanelUI SelectPanel => _selectPanel;
        public InGamePanelUI InGamePanel => _inGamePanel;
        public PausePanelUI PausePanel => _pausePanel;
        public OptionsPanelUI OptionsPanel => _optionsPanel;
        public ResultPanelUI ResultPanel => _resultPanel;

        public string PendingResultMessage { get; private set; }

        private void Awake()
        {
            _flowFsm = new GameFlowFSM(this);
        }

        private void Start()
        {
            _titlePanel.OnPlayClicked += () => _flowFsm.ChangeState(_flowFsm.SelectState);

            _selectPanel.OnBackClicked += () => _flowFsm.ChangeState(_flowFsm.TitleState);
            _selectPanel.OnGameModeConfirmed += StartNewGame;

            _inGamePanel.OnPauseClicked += HandlePauseToggle;

            _pausePanel.OnResumeClicked += () => _flowFsm.ChangeState(_flowFsm.PlayingState);
            _pausePanel.OnReplayClicked += StartNewGame;
            _pausePanel.OnEndClicked += () => GameManager.Instance.GameRule.GameOver();

            _resultPanel.OnReplayClicked += StartNewGame;
            _resultPanel.OnSelectClicked += () => _flowFsm.ChangeState(_flowFsm.SelectState);

            _mazeLayerManager.OnLayerSwitchBlocked += HandleLayerSwitchBlocked;

            _selectPanel.Hide();
            _inGamePanel.Hide();
            _pausePanel.Hide();
            _optionsPanel.Hide();
            _resultPanel.Hide();

            _flowFsm.ChangeState(_flowFsm.TitleState);
        }

        private void Update()
        {
            if (_flowFsm.Current == _flowFsm.PlayingState)
            {
                _inGamePanel.UpdateTimer(GameManager.Instance.GameTimer.GetFormattedTime());
            }
        }

        /// <summary>
        /// Pause 버튼 클릭 또는 ESC 키 처리 - Playing 중이면 Pause로, Paused 중이면 다시 Playing으로 토글
        /// 레이어 전환 연출(ripple) 중일 때는 무시
        /// </summary>
        private void HandlePauseToggle()
        {
            if (_mazeLayerManager != null && _mazeLayerManager.IsTransitioning) return;
            if (_isGameStarting) return; // 게임 시작 연출 중에는 Pause 진입을 막음

            if (_flowFsm.Current == _flowFsm.PlayingState)
            {
                _flowFsm.ChangeState(_flowFsm.PausedState);
            }
            else if (_flowFsm.Current == _flowFsm.PausedState)
            {
                _flowFsm.ChangeState(_flowFsm.PlayingState);
            }
        }

        /// <summary>
        /// 레이어 전환이 벽에 막혔을 때 살짝 카메라 흔들림 + 보라색 화면 플래시로 피드백
        /// </summary>
        private void HandleLayerSwitchBlocked()
        {
            _layerSwitchBlockedImpulseSource?.GenerateImpulse();
            _layerSwitchBlockedFlashUI?.Flash();
            _inGamePanel.ShowLayerBlockedWarning();
        }

        /// <summary>
        /// 난이도 선택 확정, Pause의 Replay, Result의 Replay - 새 판을 시작하는 모든 진입점이 공통으로 호출
        /// </summary>
        private void StartNewGame()
        {
            _ = RunGameStartSequence();
        }

        /// <summary>
        /// 유닛 스폰 후 플레이어 인트로 애니메이션이 끝나면 카메라를 인트로->팔로우로 전환하고,
        /// 그것까지 끝날 때까지 타이머/몬스터 이동/플레이어 입력을 정지시켰다가 한번에 게임을 시작하는 시퀀스
        /// </summary>
        private async Awaitable RunGameStartSequence()
        {
            _isGameStarting = true;

            StartGame(_monsterCount);

            // 이전 패널(Select/Pause/Result)은 여기서 즉시 닫히지만, PlayingState.Enter는 IsGameStarting 때문에
            // 재개/입력허용/HUD표시를 건너뜀 - 연출이 끝나면 아래에서 직접 처리
            _flowFsm.ChangeState(_flowFsm.PlayingState);

            // Pause는 InGamePanel을 숨기지 않고 그 위에 오버레이만 띄우는 방식이라(PausedState 참고),
            // Pause -> Replay 경로에서는 HUD가 계속 떠 있는 상태로 넘어옴 - 연출 중에는 무조건 숨겨야 함
            _inGamePanel.Hide();

            GameManager.Instance.PauseGame();
            SetPlayerControlEnabled(false);

            // 플레이어 인트로 애니메이션이 끝난 뒤에야 카메라를 인트로 -> 팔로우로 전환
            await _player.PlayIntroAnimationAsync();
            await _unitsSpawner.SwitchToFollowCameraAsync();

            _isGameStarting = false;

            GameManager.Instance.GameTimer.StartTimer();
            GameManager.Instance.ResumeGame();
            SetPlayerControlEnabled(true);
            _inGamePanel.Show();
        }

        private void StartGame(int monsterCount)
        {
            GameManager.Instance.GameStart();

            _mazeLayerManager.SetLayersAndMazeGenerate();

            _unitsSpawner.SetMonsterCount(monsterCount);
            _unitsSpawner.SpawnAll();

            _player = _unitsSpawner.Player;

            SetupInGame();
        }

        /// <summary>
        /// 게임 시작 후 HP/열쇠 UI 연결, Result 화면 전환 이벤트 연결
        /// </summary>
        private void SetupInGame()
        {
            GameRule gameRule = GameManager.Instance.GameRule;

            _player.OnHPChanged += _inGamePanel.UpdateHp;
            _player.OnHPChanged += (current, max) => _damageflashUI?.Flash();
            _player.OnDead += () => gameRule.GameOver();
            if (_player.TryGetComponent(out PlayerInputHandler playerInputHandler))
            {
                playerInputHandler.OnPauseRequested += HandlePauseToggle; // ESC 키로 Pause 진입/Resume 토글
            }
            if (_player.TryGetComponent(out PlayerSanity playerSanity))
            {
                playerSanity.OnSanityChanged += _inGamePanel.UpdateSanity;
                playerSanity.OnSanityDepleted += () => gameRule.GameOver();
                _inGamePanel.UpdateSanity(playerSanity.CurrentSanity, playerSanity.MaxSanity);
            }

            gameRule.OnClear += () => ShowResult("CLEAR!!");
            gameRule.OnGameOver += () => ShowResult("GAME OVER..");
            gameRule.OnKeyCollected += _inGamePanel.UpdateKeyCount;

            _inGamePanel.UpdateHp(_player.CurrentHp, _player.MaxHp);
            _inGamePanel.UpdateKeyCount(gameRule.CurrentCollectedKeyCount, gameRule.RequiredKeyCount);
        }

        private void ShowResult(string message)
        {
            PendingResultMessage = message;
            _flowFsm.ChangeState(_flowFsm.ResultState);
        }

        /// <summary>
        /// 플레이어의 이동/레이어 전환 허용 여부를 제어 (Pause/Result 진입 시 비활성화, Playing 진입 시 재활성화)
        /// </summary>
        public void SetPlayerControlEnabled(bool isEnabled)
        {
            if (_player.TryGetComponent<PlayerInputHandler>(out PlayerInputHandler playerInputHandler))
            {
                playerInputHandler.IsControlEnabled = isEnabled;
            }
            else
            {
                Debug.LogError("GameUIController SetPlayerControlEnabled(): PlayerInputHandler is null");
            }
        }
    }
}
