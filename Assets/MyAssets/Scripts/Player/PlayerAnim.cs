using UnityEngine;

namespace Assets.MyAssets.Scripts.Player
{
    public class PlayerAnim : MonoBehaviour
    {
        private Animator _animator;

        private readonly int IsMovingHash = Animator.StringToHash("IsMoving");

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
            if (_animator == null)
            {
                Debug.LogError("_animator is null");
            }
        }

        public void SetMoving(bool isMoving)
        {
            _animator.SetBool(IsMovingHash, isMoving);
        }
    }
}
