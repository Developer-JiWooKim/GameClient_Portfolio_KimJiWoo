using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGamePanelUI : BasePanelUI
{
    [SerializeField] private TextMeshProUGUI _hpText;
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private TextMeshProUGUI _keyCountText;
    [SerializeField] private Button _pauseButton;

    public event System.Action OnPauseClicked;

    private void Start()
    {
        _pauseButton.onClick.AddListener(() => OnPauseClicked?.Invoke());
    }

    public void UpdateHp(int current, int max)
    {
        _hpText.text = $"HP : {current} / {max}";
    }

    public void UpdateKeyCount(int current, int required)
    {
        _keyCountText.text = $"Keys : {current} / {required}";
    }

    public void UpdateTimer(string formattedTime)
    {
        _timerText.text = formattedTime;
    }
}
