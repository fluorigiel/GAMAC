using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{

    public static PlayerInput PlayerInput; // static ~= that creates only one variable, because it is an unique object that is not changing 
                                           // (when other script use that variable it will refer to that one and not copy it to another)

    public static Vector2 Movement;
    public static bool JumpWasPressed;
    public static bool JumpIsHeld;
    public static bool JumpWasReleased;
    public static bool RunIsHeld;
    public static bool DashWasPressed;

    public static bool PowerUp1WasPressed;
    public static bool PowerUp1WasReleased;
    public static bool PowerUp2WasPressed;
    public static bool PowerUp2WasReleased;
    public static bool PowerUp3WasPressed;
    public static bool PowerUp3WasReleased;
    
    public static bool PauseMenuWasPressed;

    public static bool AttackWasPressed;

    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _runAction;
    private InputAction _dashAction;

    private InputAction _power1Action;
    private InputAction _power2Action;
    private InputAction _power3Action;

    private InputAction _pauseMenuAction;

    private InputAction _attackAction;

    void Awake()
    {
        PlayerInput = GetComponent<PlayerInput>();

        _moveAction = PlayerInput.actions["Move"];
        _jumpAction = PlayerInput.actions["Jump"];
        _runAction = PlayerInput.actions["Run"];
        _dashAction = PlayerInput.actions["Dash"];

        _power1Action = PlayerInput.actions["PowerUp1"];
        _power2Action = PlayerInput.actions["PowerUp2"];
        _power3Action = PlayerInput.actions["PowerUp3"];

        _pauseMenuAction = PlayerInput.actions["PauseMenu"];

        _attackAction = PlayerInput.actions["Attack"];
    }

    void Update()
    {
        PauseMenuWasPressed = _pauseMenuAction.WasPressedThisFrame();

        if (!PauseMenu.isPaused)
        {
            Movement = _moveAction.ReadValue<Vector2>();

            JumpWasPressed = _jumpAction.WasPressedThisFrame();
            JumpIsHeld = _jumpAction.IsPressed();
            JumpWasReleased = _jumpAction.WasReleasedThisFrame();

            RunIsHeld = _runAction.IsPressed();

            DashWasPressed = _dashAction.WasPressedThisFrame();

            PowerUp1WasPressed = _power1Action.WasPressedThisFrame();
            PowerUp1WasReleased = _power1Action.WasReleasedThisFrame();
            PowerUp2WasPressed = _power2Action.WasPressedThisFrame();
            PowerUp2WasReleased = _power2Action.WasReleasedThisFrame();
            PowerUp3WasPressed = _power3Action.WasPressedThisFrame();
            PowerUp3WasReleased = _power3Action.WasReleasedThisFrame();

            AttackWasPressed = _attackAction.WasPressedThisFrame();
        }
    }
}
