using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGamePanelUI : BasePanelUI
{
    [SerializeField] private TextMeshProUGUI _hpText;
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private TextMeshProUGUI _keyCountText;
    [SerializeField] private Button _endButton;

    private void Start()
    {
        _endButton.onClick.AddListener(() => GameManager.Instance.GameRule.GameOver());
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
