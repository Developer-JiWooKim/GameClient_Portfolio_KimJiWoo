using UnityEngine;

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

        // 모든 몬스터 타겟 제거 #TODO 이 부분을 게임 룰에서 해야되는가? 아마 필요없을듯, 그냥 OnClear만 발생시키는게 나을듯
        // 사운드도 OnClear에 등록해두면 여기서 할 필요없음
        foreach (var monster in Object.FindObjectsByType<MonsterController>())
        {
            monster.Target = null;
        }
        SoundManager.Instance?.PlayGameClear();


        OnClear?.Invoke();
    }

    public void GameOver()
    {
        if (_isGameEnd) return;

        _isGameEnd = true;

        // #TODO: 여기도 위와 동일
        SoundManager.Instance?.PlayGameOver();

        OnGameOver?.Invoke();
    }
}
