using Assets.MyAssets.Scripts.Utility.SingleTon;
using FischlWorks_FogWar;
using Unity.Cinemachine;
using UnityEngine;

namespace Assets.MyAssets.Scripts.Player
{
    [RequireComponent(typeof(PlayerMove))]
    [RequireComponent(typeof(PlayerAnim))]
    [RequireComponent(typeof(PlayerInputHandler))]
    [RequireComponent(typeof(PlayerFaceController))]
    [RequireComponent(typeof(CinemachineImpulseSource))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _moveSpeed = 10f;
        [SerializeField] private int _maxHp = 3;
        [SerializeField] private float _sightRange = 10f;
        [SerializeField] private float _invincibleDuration = 1.5f;

        private PlayerInputHandler _playerInputHandler;
        private PlayerMove _playerMove;
        private PlayerAnim _playerAnim;
        private PlayerFaceController _faceController;

        private CinemachineImpulseSource _impulseSource;

        private csFogWar _fogWarSystem;
        private csFogWar.FogRevealer _myRevealer;
        private int _fogRevealerIndex = -1;

        private int _currentHp;

        private float _invincibleTimer;

        private int _detectingMonsterCount; // 현재 나를 발각(추격)중인 몬스터 수, 여러 마리가 동시에 감지할 수 있으므로 카운트로 관리

        public int CurrentHp => _currentHp;
        public int MaxHp => _maxHp;

        public event System.Action<int, int> OnHPChanged;  // 체력이 변경됐을 때 (현재, 최대) 이벤트
        public event System.Action OnDead;                 // 플레이어 체력이 0이 되어 죽었을 때 이벤트

        private void Awake() => Initialize();
        private void Initialize()
        {
            _faceController = GetComponent<PlayerFaceController>();
            _playerInputHandler = GetComponent<PlayerInputHandler>();
            _playerMove = GetComponent<PlayerMove>();
            _playerAnim = GetComponent<PlayerAnim>();

            _impulseSource = GetComponent<CinemachineImpulseSource>();

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
                _fogRevealerIndex = _fogWarSystem.AddFogRevealer(_myRevealer);
            }
        }

        /// <summary>
        /// 파괴되기 전 안개 시스템에서 자신의 리빌러를 제거하는 메소드
        /// 재시작(Replay) 시 이전 플레이어의 리빌러가 안개 시스템에 계속 쌓이는 것을 방지
        /// </summary>
        public void UnregisterFromFogSystem()
        {
            if (_fogWarSystem == null || _fogRevealerIndex < 0) return;

            _fogWarSystem.RemoveFogRevealer(_fogRevealerIndex);
            _fogRevealerIndex = -1;
        }

        private void Update()
        {
            if (_invincibleTimer > 0)
            {
                _invincibleTimer -= Time.deltaTime;
            }

            // PlayerInputHandler가 onActionTriggered 콜백으로 갱신해둔 입력값을 그대로 읽어서 사용
            Vector3 dir = new Vector3(_playerInputHandler.InputVector.x, 0, _playerInputHandler.InputVector.y);

            bool hasInput = dir.sqrMagnitude > 0.0001f;

            _playerAnim.SetMoving(hasInput);

            if (hasInput)
            {
                _playerMove.Move(dir, _moveSpeed);
            }
        }

        /// <summary>
        /// 몬스터 공격 범위 안에 닿았을 때 호출될 메소드
        /// </summary>
        public bool TakeDamage()
        {
            if (_currentHp <= 0) return false;
            if (_invincibleTimer > 0f) return false;

            _currentHp--;
            _invincibleTimer = _invincibleDuration;

            // 피격 시 카메라 흔들림 발생 (Cinemachine Impulse Listener가 받아서 처리)
            _impulseSource?.GenerateImpulse();

            SoundManager.Instance?.PlayPlayerDamaged();

            OnHPChanged?.Invoke(_currentHp, _maxHp);

            if (_currentHp <= 0) OnDead?.Invoke();

            return true;
        }

        /// <summary>
        /// 몬스터가 이 플레이어를 발각(Chase/Attack 진입, 이탈)했을 때 호출하는 메소드
        /// 동시에 여러 몬스터가 감지할 수 있으므로 카운트가 0보다 큰 동안만 발각 표정을 유지
        /// </summary>
        public void NotifyDetected(bool isDetected)
        {
            _detectingMonsterCount = Mathf.Max(0, _detectingMonsterCount + (isDetected ? 1 : -1));

            if (_detectingMonsterCount > 0)
            {
                _faceController?.SetCaughtExpression();
            }
            else
            {
                _faceController?.SetNormalExpression();
            }
        }
    }
}
