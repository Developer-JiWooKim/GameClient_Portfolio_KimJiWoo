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
        [SerializeField] private float _invincibleDuration = 1.5f; // 무적 시간

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
        private int _detectingMonsterCount; // 현재 나를 감지한(추격)중인 몬스터 수, 여러 마리가 동시에 감지할 수 있으므로 카운트로 관리

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
                _myRevealer = new csFogWar.FogRevealer(this.transform, (int)_sightRange, true);

                _fogRevealerIndex = _fogWarSystem.AddFogRevealer(_myRevealer);
            }
            else
            {
                Debug.LogError("RegisterToFogSystem(): Parameter fogWarSystem is null!!");
            }
        }

        /// <summary>
        /// 파괴되기 전 csFogWar(FogWarSystem)에서 자신의 리빌러를 제거하는 메소드
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

            // 움직임 여부에 따라 애니메이션 재생
            _playerAnim.SetMoving(hasInput);

            // 움직였을때만 플레이어 위치 이동
            if (hasInput)
            {
                _playerMove.Move(dir, _moveSpeed);
            }
        }

        /// <summary>
        /// 스폰 시 재생되는 인트로 애니메이션이 끝날 때까지 대기하는 메소드
        /// </summary>
        public Awaitable PlayIntroAnimationAsync() => _playerAnim.PlayIntroAsync();

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
        /// 몬스터가 플레이어를 발각(Chase/Attack 진입, 이탈)했을 때 호출하는 메소드
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
