using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.MyAssets.Scripts.Player
{
    /// <summary>
    /// Unity의 PlayerInput 컴포넌트가 보내주는 onActionTriggered 콜백을 받아서,
    /// 이동 입력(Vector2)과 레이어 전환 요청을 다른 스크립트에 전달
    ///
    /// 기존에는 매 프레임 Keyboard.current를 직접 폴링했지만,
    /// 이제는 Input Action이 트리거될 때만 콜백으로 값을 받는 이벤트 기반 방식으로 변경
    /// </summary>
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerInputHandler : MonoBehaviour
    {
        private Vector2 _inputVector = Vector2.zero;

        public Vector2 InputVector => _inputVector;

        private PlayerInput _playerInput; // 유니티 내장 PlayerInput 컴포넌트

        public event System.Action<Vector3> OnLayerSwitchRequested; // SwitchLayer 액션(Tab 키)이 트리거됐을 때 발생하는 이벤트
        public event System.Action          OnPauseRequested;       // Pause 액션(ESC 키)이 트리거됐을 때 발생하는 이벤트

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
        }

        private void OnEnable()
        {
            _playerInput.onActionTriggered += HandleActionTriggered;
        }

        private void OnDisable()
        {
            _playerInput.onActionTriggered -= HandleActionTriggered;

            _inputVector = Vector2.zero; // 비활성화되는 순간 입력 초기화
        }

        /// <summary>
        /// PlayerInput이 액션 하나가 트리거될 때마다 호출해주는 콜백 메소드
        /// </summary>
        private void HandleActionTriggered(InputAction.CallbackContext context)
        {
            switch (context.action.name)
            {
                case "Move":
                    _inputVector = context.ReadValue<Vector2>();
                    break;
                case "SwitchLayer":
                    if (context.phase == InputActionPhase.Performed)
                    {
                        OnLayerSwitchRequested?.Invoke(transform.position);
                    }
                    break;
                case "Pause":
                    if (context.phase == InputActionPhase.Performed)
                    {
                        OnPauseRequested?.Invoke();
                    }
                    break;
            }
        }
    }
}
