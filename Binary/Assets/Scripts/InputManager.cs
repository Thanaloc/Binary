using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static PlayerInput PlayerInput;

    public static Vector2 Movement;
    public static bool JumpWasPressed;
    public static bool JumpIsHeld;
    public static bool JumpWasReleased;

    private InputAction _moveAction;
    private InputAction _jumpAction;

    private void Awake()
    {
        PlayerInput = GetComponent<PlayerInput>();

        _moveAction = PlayerInput.actions["Move"];
        _jumpAction = PlayerInput.actions["Jump"];

        _jumpAction.performed += JumpAction;
        _jumpAction.started += JumpAction;
        _jumpAction.canceled += JumpAction;
    }

    private void Update()
    {
        Movement = _moveAction.ReadValue<Vector2>();
        JumpIsHeld = _jumpAction.IsPressed();

    }

    private void LateUpdate()
    {
        // Reset les états après que tous les scripts les aient lus
        JumpWasPressed = false;
        JumpWasReleased = false;
    }

    private void OnEnable()
    {
        _jumpAction.Enable();
    }

    private void OnDisable()
    {
        _jumpAction.Disable();
    }

    private void OnDestroy()
    {
        _jumpAction.performed -= JumpAction;
        _jumpAction.started -= JumpAction;
        _jumpAction.canceled -= JumpAction;
    }

    private void JumpAction(InputAction.CallbackContext p_ctx)
    {
        if (p_ctx.started || p_ctx.performed)
        {
            JumpWasPressed = true;
            JumpWasReleased = false;
        }
        else if (p_ctx.canceled)
        {
            JumpWasPressed = false;
            JumpWasReleased = true;
        }
    }
}