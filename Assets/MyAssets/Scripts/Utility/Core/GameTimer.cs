using UnityEngine;

namespace Assets.MyAssets.Scripts.Utility.Core
{
    public class GameTimer
    {
        private float _elapsedTime = 0f;
        private bool _isRunning = false;

        private int _lastFormattedSeconds = -1;
        private string _cachedFormattedTime = "00:00";

        public bool IsRunning => _isRunning;

        public void StartTimer()
        {
            _elapsedTime = 0f;
            _isRunning = true;
            _lastFormattedSeconds = -1;
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
            int totalSeconds = (int)_elapsedTime;

            if (totalSeconds != _lastFormattedSeconds)
            {
                _lastFormattedSeconds = totalSeconds;

                int minutes = totalSeconds / 60;
                int seconds = totalSeconds % 60;

                _cachedFormattedTime = minutes.ToString("D2") + ":" + seconds.ToString("D2");
            }

            return _cachedFormattedTime;
        }
    }
}
