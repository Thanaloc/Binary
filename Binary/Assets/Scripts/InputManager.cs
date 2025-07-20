using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static PlayerInput PlayerInput;

    public static Vector2 Movement;

    //Jump
    public static bool JumpWasPressed;
    public static bool JumpIsHeld;
    public static bool JumpWasReleased;

    //Grapple

    public static bool GrappleWasPressed;
    public static bool GrappleIsHeld;
    public static bool GrappleWasReleased;

    //Actions
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _grappleAction;

    private void Awake()
    {
        PlayerInput = GetComponent<PlayerInput>();

        _moveAction = PlayerInput.actions["Move"];
        _jumpAction = PlayerInput.actions["Jump"];
        _grappleAction = PlayerInput.actions["Grapple"];

        _jumpAction.performed += JumpAction;
        _jumpAction.started += JumpAction;
        _jumpAction.canceled += JumpAction;

        _grappleAction.performed += GrappleAction;
        _grappleAction.started += GrappleAction;
        _grappleAction.canceled += GrappleAction;
    }

    private void Update()
    {
        Movement = _moveAction.ReadValue<Vector2>();
        JumpIsHeld = _jumpAction.IsPressed();
        GrappleIsHeld = _grappleAction.IsPressed();

    }

    private void LateUpdate()
    {
        // Reset les états après que tous les scripts les aient lus
        JumpWasPressed = false;
        JumpWasReleased = false;

        GrappleWasPressed = false;
        GrappleWasReleased = false;
    }

    private void OnEnable()
    {
        _jumpAction.Enable();
        _grappleAction.Enable();
    }

    private void OnDisable()
    {
        _jumpAction.Disable();
        _grappleAction.Disable();
    }

    private void OnDestroy()
    {
        _jumpAction.performed -= JumpAction;
        _jumpAction.started -= JumpAction;
        _jumpAction.canceled -= JumpAction;

        _grappleAction.performed -= GrappleAction;
        _grappleAction.started -= GrappleAction;
        _grappleAction.canceled -= GrappleAction;
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

    private void GrappleAction(InputAction.CallbackContext p_ctx)
    {
        if (p_ctx.started || p_ctx.performed)
        {
            GrappleWasPressed = true;
            GrappleWasReleased = false;
        }
        else if (p_ctx.canceled)
        {
            GrappleWasPressed = false;
            GrappleWasReleased = true;
        }
    }
}