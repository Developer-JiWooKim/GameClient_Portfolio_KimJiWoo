using UnityEngine;

namespace Assets.MyAssets.Scripts.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMove : MonoBehaviour
    {
        private CharacterController _characterController;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
        }

        /// <summary>
        /// 방향과 속력을 받아 플레이어를 이동 방향으로 회전 및 이동 시키는 메소드
        /// </summary>
        public void Move(Vector3 moveDir, float moveSpeed)
        {
            moveDir.Normalize();

            RotateToward(moveDir);

            // CharacterController 기반 캐릭터 이동
            _characterController.Move(moveDir * moveSpeed * Time.deltaTime);
        }

        private void RotateToward(Vector3 dir)
        {
            transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        }
    }
}
