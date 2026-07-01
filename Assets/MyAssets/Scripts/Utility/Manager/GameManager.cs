using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static GameMode CurrentGameMode { get; set; } = GameMode.Normal;

    private GameTimer _gameTimer;
    public GameTimer GameTimer => _gameTimer;

    private GameRule _gameRule;
    public GameRule GameRule => _gameRule;
    public bool IsPaused { get; private set; }

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
        _gameRule  = new GameRule();
    }

    private void Update()
    {
        if (_gameTimer.IsRunning)
        {
            _gameTimer.UpdateTime();
        }
    }

    public void PauseGame()
    {
        if (IsPaused) return;
        
        IsPaused = true;

        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        if (!IsPaused) return;

        IsPaused = false;

        Time.timeScale = 1f;
    }

    /// <summary>
    /// 새 판을 시작할 때 호출하는 메소드
    /// </summary>
    public void GameStart()
    {
        _gameRule = new GameRule();

        // 타이머는 GameRule이 모르는 영역이라 GameManager가 둘을 이어주는 역할
        // GameRule이 클리어/오버를 선언하면 그에 맞춰 타이머를 멈춤
        _gameRule.OnClear    += _gameTimer.StopTimer;
        _gameRule.OnGameOver += _gameTimer.StopTimer;

        _gameTimer.StartTimer();
    }

    public void GameExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// Hard 난이도 + Arcane 레이어 조합 여부 (몬스터의 항시 감지/추격속도 배율 판정에 공통으로 사용)
    /// </summary>
    public static bool IsHardArcaneMode()
    {
        if (MazeLayerManager.Instance == null) return false;

        return CurrentGameMode == GameMode.Hard && MazeLayerManager.Instance.CurrentLayer == MazeLayerManager.LayerType.Arcane;
    }
}
