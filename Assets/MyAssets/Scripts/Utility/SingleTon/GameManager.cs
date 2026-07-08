using Assets.MyAssets.Scripts.Utility.Core;
using UnityEngine;

namespace Assets.MyAssets.Scripts.Utility.SingleTon
{
    public class GameManager : Singleton<GameManager>
    {
        public static GameMode CurrentGameMode { get; set; } = GameMode.Normal;

        private GameTimer _gameTimer;
        public GameTimer GameTimer => _gameTimer;

        private GameRule _gameRule;
        public GameRule GameRule => _gameRule;

        public bool IsPaused { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            if (!IsValidInstance) return;

            _gameTimer = new GameTimer();
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
            _gameRule.OnKeyCollected += (current, required) => SoundManager.Instance?.PlayKeyCollected();
            _gameRule.OnClear += _gameTimer.StopTimer;
            _gameRule.OnClear += () => SoundManager.Instance?.PlayGameClear();
            _gameRule.OnGameOver += _gameTimer.StopTimer;
            _gameRule.OnGameOver += () => SoundManager.Instance?.PlayGameOver();
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
}
