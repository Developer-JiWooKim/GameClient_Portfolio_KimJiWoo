using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGamePanelUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _hpText;
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private TextMeshProUGUI _keyCountText;
    [SerializeField] private Button _endButton;

    public bool IsActive => gameObject.activeSelf;

    private void Awake()
    {
        _endButton.onClick.AddListener(() => GameManager.Instance.GameOver());
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);

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
