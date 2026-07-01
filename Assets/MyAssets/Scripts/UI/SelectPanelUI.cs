using UnityEngine;
using UnityEngine.UI;

public class SelectPanelUI : BasePanelUI
{
    [SerializeField] private Button _normalButton;
    [SerializeField] private Button _hardButton;

    public event System.Action OnGameModeConfirmed;

    private void Awake()
    {
        _normalButton.onClick.AddListener(() => Confirm(GameMode.Normal));
        _hardButton.onClick.AddListener(() => Confirm(GameMode.Hard));
    }

    private void Confirm(GameMode gameMode)
    {
        GameManager.CurrentGameMode = gameMode;

        OnGameModeConfirmed?.Invoke();
    }
}
