using UnityEngine;

namespace Assets.MyAssets.Scripts.Utility
{
    public class GameTimer
    {
        private float _elapsedTime = 0f;
        private bool  _isRunning   = false;

        public bool IsRunning => _isRunning;
        public void StartTimer()
        {
            _elapsedTime = 0f;
            _isRunning = true;
        }

        public void StopTimer()
        {
            _isRunning = false;
        }

        public void UpdateTime()
        {
            _elapsedTime += Time.deltaTime;
        }

        public string GetFormattedTime()
        {
            int minutes = (int)(_elapsedTime / 60f);
            int seconds = (int)(_elapsedTime % 60f);

            return $"{minutes:D2}:{seconds:D2}";
        }
    }
}
