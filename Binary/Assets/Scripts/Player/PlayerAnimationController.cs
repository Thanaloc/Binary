using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private Animator _PlayerMovementAnimator;
    [SerializeField] private CharacterMovement _CharacterMovementController;

    private bool _isJumping = false;
    private bool _isMoving = false;

    private void Awake()
    {
        _CharacterMovementController.JumpAction += JumpActionHandler;
        _CharacterMovementController.MoveAction += MoveActionHandler;
    }

    private void OnDestroy()
    {
        _CharacterMovementController.JumpAction -= JumpActionHandler;
        _CharacterMovementController.MoveAction -= MoveActionHandler;
    }

    private void JumpActionHandler(bool p_isJumping)
    {
        _isJumping = p_isJumping;

        if (_isMoving)
            _PlayerMovementAnimator.SetBool("IsMoving", false);

        _PlayerMovementAnimator.SetBool("IsJumping", p_isJumping);
    }

    private void MoveActionHandler(bool p_isMoving)
    {
        _isMoving = p_isMoving;

        if (_isJumping)
            return;

        _PlayerMovementAnimator.SetBool("IsMoving", p_isMoving);
    }

}
