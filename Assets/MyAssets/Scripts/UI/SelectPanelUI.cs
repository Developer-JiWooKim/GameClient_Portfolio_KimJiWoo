using UnityEngine;
using UnityEngine.UI;

public class SelectPanelUI : BasePanelUI
{
    [SerializeField] private Button _normalButton;
    [SerializeField] private Button _hardButton;
    [SerializeField] private Button _backButton;

    public event System.Action OnGameModeConfirmed;
    public event System.Action OnBackClicked;

    private void Awake()
    {
        _normalButton.onClick.AddListener(() => Confirm(GameMode.Normal));
        _hardButton.onClick.AddListener(() => Confirm(GameMode.Hard));
        _backButton.onClick.AddListener(() => OnBackClicked?.Invoke());
    }

    private void Confirm(GameMode gameMode)
    {
        GameManager.CurrentGameMode = gameMode;

        OnGameModeConfirmed?.Invoke();
    }
}
