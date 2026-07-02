using UnityEngine;

public class GameUIController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private TitlePanelUI   _titlePanel;
    [SerializeField] private SelectPanelUI  _selectPanel;
    [SerializeField] private InGamePanelUI  _inGamePanel;
    [SerializeField] private PausePanelUI   _pausePanel;
    [SerializeField] private OptionsPanelUI _optionsPanel;
    [SerializeField] private ResultPanelUI  _resultPanel;
    [SerializeField] private DamageflashUI  _damageflashUI;

    [Header("참조")]
    [SerializeField] private MazeLayerManager _mazeLayerManager;
    [SerializeField] private UnitSpawner      _unitSpawner;

    // #TODO: 이 부분 MazeGenerator에서 SerializeField로 옮기고 항상 이렇게 생성할듯? 아니면 하드 모드일때는 몬스터 수 늘리기?
    [Header("Fixed Size")]
    [Tooltip("고정 미로 크기/몬스터 수")]
    [SerializeField] private int  _fixedCols = 20;
    [SerializeField] private int  _fixedRows = 20;
    [SerializeField] private int  _fixedMonsterCnt = 10;

    private PlayerController _player;
    public PlayerController Player => _player;

    private GameFlowFSM _flowFsm;

    public TitlePanelUI   TitlePanel   => _titlePanel;
    public SelectPanelUI  SelectPanel  => _selectPanel;
    public InGamePanelUI  InGamePanel  => _inGamePanel;
    public PausePanelUI   PausePanel   => _pausePanel;
    public OptionsPanelUI OptionsPanel => _optionsPanel;
    public ResultPanelUI  ResultPanel  => _resultPanel;

    public string PendingResultMessage { get; private set; }

    private void Awake()
    {
        _flowFsm = new GameFlowFSM(this);
    }

    private void Start()
    {
        _titlePanel.OnPlayClicked        += () => _flowFsm.ChangeState(_flowFsm.SelectState);

        _selectPanel.OnBackClicked       += () => _flowFsm.ChangeState(_flowFsm.TitleState);
        _selectPanel.OnGameModeConfirmed += StartNewGame;

        _inGamePanel.OnPauseClicked      += HandlePauseClicked;

        _pausePanel.OnResumeClicked      += () => _flowFsm.ChangeState(_flowFsm.PlayingState);
        _pausePanel.OnReplayClicked      += StartNewGame;
        _pausePanel.OnEndClicked         += () => GameManager.Instance.GameRule.GameOver();

        _resultPanel.OnReplayClicked     += StartNewGame;
        _resultPanel.OnSelectClicked     += () => _flowFsm.ChangeState(_flowFsm.SelectState);

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
    /// 일시정지 버튼 클릭 처리. 레이어 전환 연출(ripple)이 진행 중일 때는 무시한다.
    /// 전환은 내부적으로 PauseGame/ResumeGame으로 timeScale을 제어하는데, 그 도중 일시정지로 진입하면
    /// 전환의 finally ResumeGame()이 PausedState가 기대하는 timeScale=0을 1로 되돌려 게임이 멈추지 않는 문제가 생김
    /// </summary>
    private void HandlePauseClicked()
    {
        if (_mazeLayerManager != null && _mazeLayerManager.IsTransitioning) return;

        _flowFsm.ChangeState(_flowFsm.PausedState);
    }

    /// <summary>
    /// 난이도 선택 확정, Pause의 Replay, Result의 Replay - 새 판을 시작하는 모든 진입점이 공통으로 호출
    /// </summary>
    private void StartNewGame()
    {
        StartGame(_fixedCols, _fixedRows, _fixedMonsterCnt);
        _flowFsm.ChangeState(_flowFsm.PlayingState);
    }

    private void StartGame(int cols, int rows, int monsterCount)
    {
        // UnitSpawner.SpawnAll()이 GameRule.OnAllKeysCollected를 구독하므로,
        // 그보다 먼저 GameStart()를 호출해 이번 판에서 실제로 쓰일 GameRule 인스턴스를 확정해야 함.
        // 순서가 바뀌면 SpawnAll()이 구독한 인스턴스가 GameStart()에서 교체돼버려
        // 열쇠를 다 모아도 Goal Point가 생성되지 않는 버그가 생김
        GameManager.Instance.GameStart();

        _mazeLayerManager.SetLayersAndMazeGenerate(cols, rows);

        _unitSpawner.SetMonsterCount(monsterCount);
        _unitSpawner.SpawnAll();

        _player = _unitSpawner.Player;

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
        _player.OnDead      += () => gameRule.GameOver();

        gameRule.OnClear        += () => ShowResult("CLEAR!!");
        gameRule.OnGameOver     += () => ShowResult("GAME OVER..");
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
    /// 플레이어의 입력 활성화 여부를 제어 (Pause/Result 진입 시 비활성화, Playing 진입 시 재활성화)
    /// </summary>
    public void SetPlayerInputEnabled(bool isEnabled)
    {
        if (_player.TryGetComponent<PlayerInputHandler>(out PlayerInputHandler playerInputHandler))
        {
            playerInputHandler.enabled = isEnabled;
        }
        else
        {
            Debug.LogError("GameUIController SetPlayerInputEnabled(): PlayerInputHandler is null");
        }
    }
}
