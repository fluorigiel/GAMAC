using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    [Header("References")] // Header is an organizer when we look at the values from unity (like to change them from debug)
    public PlayerMovementStats MoveStats; // Name to access the stats that we defined in PlayerMovemetStats

    // SerializeField : it's like when in python we hide the variable of a parent to a child but instead do method like get to obtain / access those.
    //                  ( : so it's making varibale only accessible to the parent object and not the childrens (not private since we can
    //                  still modify those in unity debug))
    // ( https://www.youtube.com/watch?v=_9LJqhAj-FU )
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D _rb;

    //Movement vars 
    private Vector2 _moveVelocity;
    private bool _isFacingRight;

    // jump vars 
    private bool _isJumping;
    private int _numberOfJumpsUsed;
    private bool _initJumpCanceled;
    private bool _isJumpCanceled;
    private float _jumpCancelTime;
    private float _jumpCancelMoment;

    // dash vars
    private bool _isDashing;
    private bool _initDashing;
    private float _dashTimer;
    private float _dashDuration;

    // jump buffer and coyote vars // explain coyote and buffer: https://www.youtube.com/watch?v=RFix_Kg2Di0 
    private float _jumpBufferTimer;
    private float _coyoteTimer;

    // collision check vars
    private bool _isGrounded;
    private bool _bumpedHead;

    [Header("Feet box")]
    public Vector2 feetBoxSize;
    public float feetCastDistance;

    [Header("Head box")]
    public Vector2 headBoxSize;
    public float headCastDistance;


    private void Awake()
    {
        _isGrounded = false;
        _bumpedHead = false;
        _isDashing = false;

        _numberOfJumpsUsed = 0;

        _isFacingRight = true;

        _rb = GetComponent<Rigidbody2D>(); // it's like when we linked the camera to the player but automaticly, with the rigid body
    }

    private void FixedUpdate()
    {
        Jump();

        Dash();

        UpdateCollision();

        if (!_isDashing) // we don't want to change the velocity during a dash
        {
            if (_isGrounded)
            {
                Move(MoveStats.GroundAcceleration, MoveStats.GroundDeceleration, InputManager.Movement); // change the velocity of our player based on the
            }
            else // if not on the ground (in the air)
            {
                Move(MoveStats.AirAcceleration, MoveStats.AirDeceleration, InputManager.Movement); // same that above but for in the air
            }

            Gravity();
        }
    }

    private void Update()
    {
        JumpCheck();

        DashCheck();

        Debug.Log("Is Grounded ? " + _isGrounded);
        Debug.Log("Head Bumped ? " + _bumpedHead);
        Debug.Log(InputManager.Movement);

        CountTimers();
    }

    #region Movement

    private void Move(float acceleration, float deceleration, Vector2 moveInput)
    {
        _moveVelocity = _rb.linearVelocity;

        if (moveInput != Vector2.zero) // if our player moved 
        {
            TurnCheck(moveInput);//check if he needs to turn around

            Vector2 targetVelocity = Vector2.zero;
            if (InputManager.RunIsHeld)
            {
                targetVelocity = new Vector2(moveInput.x * MoveStats.MaxRunSpeed, 0f);
            }
            else
            {
                targetVelocity = new Vector2(moveInput.x * MoveStats.MaxWalkSpeed, 0f);
            }

            _moveVelocity = Vector2.Lerp(_moveVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            // Simply, Lerp is linear interpollation (~ it's taking our current velocity, the objective velocity and it's reaching a
            // (:like we learned in algo taa)           middle value based on the passed time, to not go max speed instantly) 
        }
        else // if the player stopped
        {
            _moveVelocity = Vector2.Lerp(_moveVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime); // same as before but to decelerate
        }


        _rb.linearVelocity = new Vector2(_moveVelocity.x, _rb.linearVelocity.y); // we change the velocity of the player with new x velocity and current y velocity
    }

    private void TurnCheck(Vector2 moveInput)
    {
        if (_isFacingRight && moveInput.x < 0) // moveInput is returning a Vector2 (= 2 value stored together) of x and y 
                                               // to understand them image a joystick, full left is -1 for the first paramether (x) and 0 for the second (y)
                                               // and so on for every direction (like in a circle)
        {
            _isFacingRight = false;
            transform.Rotate(0f, -180f, 0f);
        }
        else if (!_isFacingRight && moveInput.x > 0)
        {
            _isFacingRight = true;
            transform.Rotate(0f, 180f, 0f);  // rotating the ~rigidBody, not the sprite (so that walking/... remain positive values to go in front of us)
        }
    }

    #endregion

    #region Jump

    private void JumpCheck()
    {
        if (InputManager.JumpWasPressed)
        {
            _jumpBufferTimer = MoveStats.JumpBufferTime;
        }

        if (_jumpBufferTimer > 0 && _numberOfJumpsUsed < MoveStats.NumberOfJumpsAllowed && !_isJumping)
        {
            _isJumping = true;
        }

        if (InputManager.JumpWasReleased && _jumpCancelTime > 0)
        {
            _jumpCancelTime = 0;
            _initJumpCanceled = true;
        }
    }

    private void Jump()
    {
        if (_initJumpCanceled && _jumpCancelMoment <= 0)
        {
            _initJumpCanceled = false;
            _isJumpCanceled = true;
        }

        if (_isJumping)
        {
            _isJumping = false;
            _jumpBufferTimer = 0;
            if (_numberOfJumpsUsed == 0)
            {
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, MoveStats.JumpHeight);
                _jumpCancelTime = MoveStats.JumpCancelTime;
            }
            else
            {
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, MoveStats.JumpHeight * MoveStats.MultipleJumpStrengthPercent);
            }
            _numberOfJumpsUsed++;
        }
    }

    #endregion

    #region Dash

    private void DashCheck()
    {
        if (InputManager.DashWasPressed && _dashTimer <= 0)
        {
            _initDashing = true;
        }

        if (_dashDuration <= 0)
        {
            _isDashing = false;
        }
    }

    private void Dash()
    {
        if (_initDashing && InputManager.Movement != Vector2.zero)
        {
            _initDashing = false;
            _isDashing = true;
            _dashTimer = MoveStats.DashTime;
            _dashDuration = MoveStats.DashDuration;
            _rb.linearVelocity = InputManager.Movement * MoveStats.MaxWalkSpeed * MoveStats.DashStrength;
        }
        else
        {
            _initDashing = false;
        }
    }

    #endregion

    #region Gravity

    private void Gravity()
    {
        if (!_isGrounded)
        {
            float usedGravity = MoveStats.GravityForce;
            if (_rb.linearVelocity.y <= 0 || _bumpedHead || _isJumpCanceled)
            {
                usedGravity = MoveStats.GravityFallForce; // to make a beautiful jump curve
            }

            Vector2 targetVelocity = new Vector2(0f, -MoveStats.MaxFallSpeed);
            Vector2 airVelocity = new Vector2(0f, _rb.linearVelocity.y);

            airVelocity = Vector2.Lerp(airVelocity, targetVelocity, usedGravity * Time.fixedDeltaTime);
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, airVelocity.y);
        }

        else if (_isGrounded)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, 0f);
        }
    }

    #endregion

    #region Collision

    private void UpdateGrounded()
    {
        // BoxCast is quite ugly but simply projecting a box to see if there is an object
        if (Physics2D.BoxCast(transform.position, feetBoxSize, 0f, Vector3.down, feetCastDistance, groundLayer) && _rb.linearVelocity.y <= 0)
        {
            _isGrounded = true;
            _numberOfJumpsUsed = 0;
            _isJumpCanceled = false;
        }
        else
        {
            _isGrounded = false;
        }
       }

    private void UpdateBumpedHead()
    {
        // https://www.youtube.com/watch?v=CKz4vTWshuc
        if (Physics2D.BoxCast(transform.position, headBoxSize, 0f, Vector3.up, headCastDistance, groundLayer) && _rb.linearVelocity.y >= 0)
        {
            _bumpedHead = true;
        }
        else
        {
            _bumpedHead = false;
        }
    }

    private void UpdateCollision()
    {
        UpdateGrounded();
        UpdateBumpedHead();
    }

    private void OnDrawGizmos() // for init and debug to see the BoxCast
    {
        // for feet box test :
        Gizmos.color = Color.blue;
        Gizmos.DrawCube(transform.position + Vector3.down * feetCastDistance, feetBoxSize);
        // for head box test :
        Gizmos.color = Color.red;
        Gizmos.DrawCube(transform.position + Vector3.up * headCastDistance, headBoxSize);
    }

    #endregion

    #region Timer

    private void CountTimers()
    {
        float deltaTime = Time.deltaTime;

        if (_jumpCancelMoment > 0)
        {
            _jumpCancelMoment -= deltaTime;
        }
        if (_jumpCancelTime > 0)
        {
            _jumpCancelTime -= deltaTime;
        }
        if (_jumpBufferTimer > 0)
        {
            _jumpBufferTimer -= deltaTime;
        }
        if (_dashTimer > 0)
        {
            _dashTimer -= deltaTime;
        }
        if (_dashDuration > 0)
        {
            _dashDuration -= deltaTime;
        }
    }

    #endregion
}
