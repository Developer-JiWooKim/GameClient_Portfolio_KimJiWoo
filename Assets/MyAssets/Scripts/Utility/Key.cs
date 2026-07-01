using UnityEngine;

public class Key : MonoBehaviour
{
    /// <summary>
    /// 획득 시 발행되는 이벤트, 오브젝트 풀 반납은 구독자(UnitSpawner)가 처리
    /// </summary>
    public event System.Action<Key> OnCollected;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        GameManager.Instance.GameRule.CollectKey();

        SoundManager.Instance?.PlayKeyCollected();

        OnCollected?.Invoke(this);
    }
}
