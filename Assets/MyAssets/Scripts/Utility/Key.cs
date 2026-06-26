using UnityEngine;

public class Key : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        GameManager.Instance.CollectKey();

        SoundManager.Instance?.PlayKeyCollected();

        Destroy(this.gameObject);
    }
}
