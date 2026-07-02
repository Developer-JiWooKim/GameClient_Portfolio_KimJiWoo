/// <summary>
/// 게임의 클리어 여부와 고유 규칙을 담당하는 순수 클래스
/// </summary>
public class GameRule
{
    private readonly int _requiredKeyCount         = 5; // 게임 클리어 위해 모아야 되는 열쇠 개수
    private int          _currentCollectedKeyCount = 0;  // 현재 모은 키 개수

    public int RequiredKeyCount => _requiredKeyCount;
    public int CurrentCollectedKeyCount => _currentCollectedKeyCount;

    private bool _isGameEnd = false;

    public event System.Action<int, int> OnKeyCollected;     // 열쇠를 모을때마다 발생할 이벤트(효과음, UI업데이트)
    public event System.Action           OnAllKeysCollected; // 모든 열쇠를 모은 시점에 1회 발생할 이벤트(Goal Point 생성)
    public event System.Action           OnClear;            // 게임 클리어 시 발생할 이벤트(Result UI, 효과음)
    public event System.Action           OnGameOver;         // 게임 패배 시 발생할 이벤트(Result UI, 효과음)

    public void CollectKey()
    {
        if (_currentCollectedKeyCount >= _requiredKeyCount) return;

        _currentCollectedKeyCount++;

        OnKeyCollected?.Invoke(_currentCollectedKeyCount, _requiredKeyCount);

        if (_currentCollectedKeyCount >= _requiredKeyCount)
        {
            OnAllKeysCollected?.Invoke();
        }
    }

    public void Clear()
    {
        if (_isGameEnd) return;

        _isGameEnd = true;

        SoundManager.Instance?.PlayGameClear();

        // 몬스터가 더 이상 플레이어를 쫓지 않도록 타겟을 끊는 처리는 몬스터 인스턴스를 들고 있는
        // UnitSpawner가 OnClear를 구독해서 직접 담당 (씬 전체를 스캔할 필요가 없어짐)
        OnClear?.Invoke();
    }

    public void GameOver()
    {
        if (_isGameEnd) return;

        _isGameEnd = true;

        SoundManager.Instance?.PlayGameOver();

        OnGameOver?.Invoke();
    }
}
