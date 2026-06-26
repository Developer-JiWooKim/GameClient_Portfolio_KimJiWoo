using UnityEngine;

public class GameUIController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private TitlePanelUI _titlePanel;
    [SerializeField] private InGamePanelUI _inGamePanel;
    [SerializeField] private ResultPanelUI _resultPanel;
    [SerializeField] private DamageflashUI _damageflashUI;

    [Header("참조")]
    [SerializeField] private MazeLayerManager _mazeLayerManager;
    [SerializeField] private UnitSpawner      _unitSpawner;    

    [Header("Fixed Size")]
    [Tooltip("고정 미로 크기/몬스터 수. true면 Setup Panel을 건너뛰고 바로 시작")]
    [SerializeField] private int _fixedCols = 20;
    [SerializeField] private int _fixedRows = 20;
    [SerializeField] private int _fixedMonsterCnt = 10;

    private PlayerController _player;

    private void Start()
    {
        _titlePanel.OnPlayClicked += OnPlayClicked;

        _inGamePanel.Hide();
        _resultPanel.Hide();

        if (GameManager.SkipTitleOnLoad)
        {
            GameManager.SkipTitleOnLoad = false; // 한 번 쓰고 초기화 (다음 정상 시작 때는 다시 타이틀 보이게)
            _titlePanel.Hide();
            OnPlayClicked();
        }
        else
        {
            _titlePanel.Show();
        }
    }

    private void Update()
    {
        if (_inGamePanel.IsActive)
        {
            _inGamePanel.UpdateTimer(GameManager.Instance.GameTimer.GetFormattedTime()); 
        }
    }

    private void OnPlayClicked()
    {
        _titlePanel.Hide();


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
        _player.OnHPChanged += _inGamePanel.UpdateHp;
        _player.OnHPChanged += (current, max) => _damageflashUI?.Flash();
        _player.OnDead      += () => GameManager.Instance.GameOver();

        GameManager.Instance.OnClear        += () => ShowResult("CLEAR!!");
        GameManager.Instance.OnGameOver     += () => ShowResult("GAME OVER..");
        GameManager.Instance.OnKeyCollected += _inGamePanel.UpdateKeyCount;

        _inGamePanel.Show();

        _inGamePanel.UpdateHp(_player.CurrentHp, _player.MaxHp);
        _inGamePanel.UpdateKeyCount(GameManager.Instance.CurrentCollectedKeyCount, GameManager.Instance.RequiredKeyCount);

        GameManager.Instance.GameStart();
    }

    /// <summary>
    /// 결과 화면 전환, 플레이어 입력 막기
    /// </summary>
    private void ShowResult(string message)
    {
        _inGamePanel.Hide();
        _resultPanel.Show(message, GameManager.Instance.GameTimer.GetFormattedTime());

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
