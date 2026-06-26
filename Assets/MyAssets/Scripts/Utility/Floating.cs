using UnityEngine;

/// <summary>
/// 오브젝트를 제자리에서 위아래로 천천히 떠다니게 하고, 동시에 회전시키는 비주얼 전용 컴포넌트
/// </summary>
public class Floating : MonoBehaviour
{
    [Header("Floating Setting")]
    [SerializeField] private float _bobbingHeight = 0.3f; // 위아래 움직이는 폭
    [SerializeField] private float _bobbingSpeed  = 1.5f; // 부유 속도

    private Vector3 _startLocalPos;
    private float _bobbingTimer;

    private void OnEnable()
    {
        _startLocalPos = transform.localPosition;

        // 여러 개가 동시에 시작해도 똑같이 움직이지 않도록 시작 타이밍을 랜덤하게 어긋나게
        _bobbingTimer = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Update()
    {
        _bobbingTimer += Time.deltaTime * _bobbingSpeed;

        Vector3 pos = _startLocalPos;
        pos.y += Mathf.Sin(_bobbingTimer) * _bobbingHeight;

        transform.localPosition = pos;
    }
}
