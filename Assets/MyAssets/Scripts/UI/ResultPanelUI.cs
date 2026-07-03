using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Assets.MyAssets.Scripts.Utility.SingleTon;

namespace Assets.MyAssets.Scripts.UI
{
    public class ResultPanelUI : BasePanelUI
    {
        [SerializeField] private TextMeshProUGUI _resultText;
        [SerializeField] private TextMeshProUGUI _resultTimeText;
        [SerializeField] private Button _replayButton;
        [SerializeField] private Button _selectButton;
        [SerializeField] private Button _gameEndButton;

        public event System.Action OnReplayClicked;
        public event System.Action OnSelectClicked;

        private void Awake()
        {
            _replayButton.onClick.AddListener(() => OnReplayClicked?.Invoke());
            _selectButton.onClick.AddListener(() => OnSelectClicked?.Invoke());
            _gameEndButton.onClick.AddListener(() => GameManager.Instance.GameExit());
        }

        /// <summary>
        /// 결과 메시지/시간을 채운 뒤 패널을 표시하는 메소드
        /// </summary>
        public void Show(string message, string formattedTime)
        {
            _resultText.text = message;
            _resultTimeText.text = $"End Time : {formattedTime}";

            Show();
        }
    }
}
