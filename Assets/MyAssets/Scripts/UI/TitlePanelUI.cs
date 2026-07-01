using UnityEngine;
using UnityEngine.UI;

public class TitlePanelUI : BasePanelUI
{
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _exitButton;

    public event System.Action OnPlayClicked;

    private void Awake()
    {
        _playButton.onClick.AddListener(() => OnPlayClicked?.Invoke());
        _exitButton.onClick.AddListener(() => GameManager.Instance.GameExit());
    }
}
