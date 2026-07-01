using UnityEngine;

public class GameUIController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private TitlePanelUI  _titlePanel;
    [SerializeField] private SelectPanelUI _selectPanel;
    [SerializeField] private InGamePanelUI _inGamePanel;
    [SerializeField] private ResultPanelUI _resultPanel;
    [SerializeField] private DamageflashUI _damageflashUI;

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

    private void Start()
    {
        _titlePanel.OnPlayClicked        += OnPlayClicked;
        _selectPanel.OnGameModeConfirmed += OnGameModeConfirmed;
        _resultPanel.OnReplayClicked     += OnReplayClicked;

        _selectPanel.Hide();
        _inGamePanel.Hide();
        _resultPanel.Hide();

        _titlePanel.Show();
    }

    private void Update()
    {
        if (_inGamePanel.IsActive)
        {
            _inGamePanel.UpdateTimer(GameManager.Instance.GameTimer.GetFormattedTime()); 
        }
    }

    private void OnGameModeConfirmed()
    {
        _selectPanel.Hide();
        StartGame(_fixedCols, _fixedRows, _fixedMonsterCnt);
    }

    private void OnPlayClicked()
    {
        _titlePanel.Hide();
        _selectPanel.Show();
    }

    /// <summary>
    /// 결과 화면의 재시작 버튼 클릭 시 호출 - 씬을 리로드하지 않고 그 자리에서 새 판을 시작
    /// </summary>
    private void OnReplayClicked()
    {
        _resultPanel.Hide();
        StartGame(_fixedCols, _fixedRows, _fixedMonsterCnt);
    }

    private void StartGame(int cols, int rows, int monsterCount)
    {
        _mazeLayerManager.SetLayersAndMazeGenerate(cols, rows);

        _unitSpawner.SetMonsterCount(monsterCount);
        _unitSpawner.SpawnAll();

        _player = _unitSpawner.Player;

        SetupInGame(cols, rows);
    }

    /// <summary>
    /// 게임 시작 후 HP/열쇠 UI 연결, Result 화면 전환 이벤트 연결
    /// </summary>
    private void SetupInGame(int cols, int rows)
    {
        // GameStart()가 새 GameRule 인스턴스를 만들기 때문에, 아래에서 구독할 참조를 얻기 전에 먼저 호출해야 함
        // (순서가 바뀌면 재시작 시 방금 만든 새 GameRule이 아닌 이전 판의 GameRule을 구독하게 됨)
        GameManager.Instance.GameStart();

        GameRule gameRule = GameManager.Instance.GameRule;

        _player.OnHPChanged += _inGamePanel.UpdateHp;
        _player.OnHPChanged += (current, max) => _damageflashUI?.Flash();
        _player.OnDead      += () => gameRule.GameOver();

        gameRule.OnClear        += () => ShowResult("CLEAR!!");
        gameRule.OnGameOver     += () => ShowResult("GAME OVER..");
        gameRule.OnKeyCollected += _inGamePanel.UpdateKeyCount;

        _inGamePanel.Show();

        _inGamePanel.UpdateHp(_player.CurrentHp, _player.MaxHp);
        _inGamePanel.UpdateKeyCount(gameRule.CurrentCollectedKeyCount, gameRule.RequiredKeyCount);
    }

    /// <summary>
    /// 결과 화면 전환, 플레이어 입력 막기
    /// </summary>
    private void ShowResult(string message)
    {
        _inGamePanel.Hide();
        _resultPanel.Show(message, GameManager.Instance.GameTimer.GetFormattedTime());

        // #TODO: UI에서 플레이어의 입력을 제한하는 코드? 이거 이상함
        if (_player.TryGetComponent<PlayerInputHandler>(out PlayerInputHandler playerInputHandler))
        {
            playerInputHandler.enabled = false;
        }
        else
        {
            Debug.LogError("GameUIController ShowResult(): PlayerInputHandler is null");
        }
    }
}
