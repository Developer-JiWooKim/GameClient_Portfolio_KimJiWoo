using UnityEngine;
using FischlWorks_FogWar;
using Unity.Cinemachine;

[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(PlayerMove))]
[RequireComponent(typeof(CinemachineImpulseSource))]
public class PlayerController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _moveSpeed = 10f;
    [SerializeField] private int   _maxHp     = 3;
    [SerializeField] private float _sightRange = 10f;

    private PlayerInputHandler _playerInputHandler;
    private PlayerMove         _playerMove;

    private CinemachineImpulseSource _impulseSource;

    private csFogWar             _fogWarSystem;
    private csFogWar.FogRevealer _myRevealer;

    private int _currentHp;

    public int CurrentHp => _currentHp;
    public int MaxHp => _maxHp;

    public event System.Action<int, int> OnHPChanged;  // 체력이 변경됐을 때 (현재, 최대) 이벤트
    public event System.Action           OnDead;       // 플레이어 체력이 0이 되어 죽었을 때 이벤트 

    private void Awake() => Initialize();
    private void Initialize()
    {
        _playerInputHandler = GetComponent<PlayerInputHandler>();
        _playerMove         = GetComponent<PlayerMove>();
        _impulseSource      = GetComponent<CinemachineImpulseSource>();

        _currentHp = _maxHp;
    }

    /// <summary>
    /// 외부 에셋(AOS Fog System)과 PlayerController 연결 메소드
    /// </summary>
    public void RegisterToFogSystem(csFogWar fogWarSystem)
    {
        _fogWarSystem = fogWarSystem;

        if (_fogWarSystem != null)
        {
            // 에셋 규칙에 맞는 전용 생성자를 사용하여 객체를 생성
            // 인자 순서: (추적할 Transform, 시야 반지름, Update에서 움직일때만)
            _myRevealer = new csFogWar.FogRevealer(this.transform, (int)_sightRange, true);

            // private 리스트인 _fogRevealers에 직접 접근하는 대신, 
            // 에셋 내부 전용 공개 메서드인 'AddFogRevealer'를 호출하여 등록
            _fogWarSystem.AddFogRevealer(_myRevealer);
        }
    }

    private void Update()
    {
        // PlayerInputHandler가 onActionTriggered 콜백으로 갱신해둔 입력값을 그대로 읽어서 사용
        Vector3 dir = new Vector3(_playerInputHandler.InputVector.x, 0, _playerInputHandler.InputVector.y);

        _playerMove.Move(dir, _moveSpeed);               
    }

    /// <summary>
    /// 몬스터 공격 범위 안에 닿았을 때 호출될 메소드
    /// </summary>
    public void TakeDamage()
    {
        if (_currentHp <= 0) return;

        _currentHp--;

        // 피격 시 카메라 흔들림 발생 (Cinemachine Impulse Listener가 받아서 처리)
        _impulseSource?.GenerateImpulse();

        SoundManager.Instance?.PlayPlayerDamaged();

        OnHPChanged?.Invoke(_currentHp, _maxHp);

        if (_currentHp <= 0) OnDead?.Invoke();
    }
}
