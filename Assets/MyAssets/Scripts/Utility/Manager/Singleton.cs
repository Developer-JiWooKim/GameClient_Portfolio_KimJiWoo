using UnityEngine;

/// <summary>
/// MonoBehaviour 싱글톤 공통 베이스 (CRTP: class Foo : Singleton&lt;Foo&gt;).
/// Awake에서 static Instance를 등록하고 중복 인스턴스는 파괴하며,
/// 파괴될 때 Instance를 비워 Fast Enter Play Mode(Reload Domain off)에서도
/// 이전 세션의 파괴된 참조가 static에 남지 않게 함.
/// </summary>
public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    public static T Instance { get; private set; }

    /// <summary>
    /// 이 인스턴스가 정식 싱글톤으로 등록됐는지 여부 (중복이라 파괴 예정이면 false).
    /// 파생 클래스의 Awake에서 base.Awake() 호출 후 초기화 진행 여부 판단에 사용
    /// </summary>
    protected bool IsValidInstance => Instance == this;

    protected virtual void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = (T)this;
    }

    protected virtual void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
