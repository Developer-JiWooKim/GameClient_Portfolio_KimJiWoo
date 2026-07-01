using UnityEngine;

/// <summary>
/// GameObject 활성/비활성으로 표시 여부를 제어하는 UI 패널의 공통 기반 클래스
/// </summary>
public abstract class BasePanelUI : MonoBehaviour
{
    public bool IsActive => gameObject.activeSelf;

    public virtual void Show() => gameObject.SetActive(true);
    public virtual void Hide() => gameObject.SetActive(false);
}
