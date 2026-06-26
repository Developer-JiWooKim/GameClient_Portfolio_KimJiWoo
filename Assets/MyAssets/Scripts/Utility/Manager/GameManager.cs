using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static bool SkipTitleOnLoad { get; set; } = false;
    public static GameManager Instance { get; private set; }

    private GameTimer _gameTimer;
    public GameTimer GameTimer => _gameTimer;    

    private bool _isGameEnd = false;

    //#TODO:열쇠로 할지, 상호작용으로 할지 미정
    private int _requiredKeyCount = 5;         // 게임 클리어 위해 모아야 되는 열쇠 개수 
    private int _currentCollectedKeyCount = 0; // 현재 모은 키 개수

    public int CurrentCollectedKeyCount => _currentCollectedKeyCount;
    public int RequiredKeyCount => _requiredKeyCount;
    public bool IsPaused { get; private set; }    

    public event System.Action<int, int> OnKeyCollected;     // 열쇠를 모을때마다 발생할 이벤트(효과음, UI업데이트)
    public event System.Action           OnAllKeysCollected; // 모든 열쇠를 모은 시점에 1회 발생할 이벤트(Goal Point 생성)
    
    public event System.Action OnClear;
    public event System.Action OnGameOver;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        _gameTimer = new GameTimer();
    }

    private void Update()
    {
        _gameTimer.UpdateTime();
    }

    /// <summary>
    /// 게임 일시정지 메소드
    /// 단, GameTimer는 따로 멈추지 않음 -> _gameTimer.UpdateTime()이 Time.deltaTime 기반이라 timeScale이 0이면 자동으로 같이 멈춤
    /// </summary>
    public void PauseGame()
    {
        if (IsPaused) return;
        
        IsPaused = true;

        Time.timeScale = 0f;
    }

    /// <summary>
    /// 게임 일시정지 해제 메소드
    /// </summary>
    public void ResumeGame()
    {
        if (!IsPaused) return;

        IsPaused = false;

        Time.timeScale = 1f;
    }

    /// <summary>
    /// Key 하나를 회수 했을 때 호출되는 메소드
    /// 현재 모은 Key 개수를 하나 증가, 관련 이벤트 실행, 모든 키를 모았을 때도 관련 이벤트 실행
    /// </summary>
    public void CollectKey()
    {
        if (_currentCollectedKeyCount >= _requiredKeyCount) return;

        _currentCollectedKeyCount++;

        OnKeyCollected?.Invoke(_currentCollectedKeyCount, _requiredKeyCount);

        if(_currentCollectedKeyCount >= _requiredKeyCount)
        {
            OnAllKeysCollected?.Invoke();
        }
    }

    public void Clear()
    {
        if (_isGameEnd) return;

        _isGameEnd = true;

        _gameTimer.StopTimer();

        // 모든 몬스터 타겟 제거
        foreach (var monster in FindObjectsByType<MonsterController>())
        {
            monster.Target = null;
        }

        SoundManager.Instance?.PlayGameClear();

        OnClear?.Invoke();
    }

    public void GameStart()
    {
        _isGameEnd = false;

        _gameTimer.StartTimer();
    }

    public void GameOver()
    {
        if (_isGameEnd) return;

        _isGameEnd = true;

        _gameTimer.StopTimer();

        SoundManager.Instance?.PlayGameOver();

        OnGameOver?.Invoke();
    }

    public void Replay()
    {
        SkipTitleOnLoad = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GameExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
