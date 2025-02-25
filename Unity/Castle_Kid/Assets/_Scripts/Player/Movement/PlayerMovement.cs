using System.Numerics;
using _Scripts.Player.Animation;
using _Scripts.Player.Trigger;
using Unity.Netcode;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;

namespace _Scripts.Player.Movement
{
    public class PlayerMovement : NetworkBehaviour
    {

        [Header("References")] // Header is an organizer when we look at the values from unity (like to change them from debug)
        public PlayerMovementStats MoveStats; // Name to access the stats that we defined in PlayerMovemetStats

        // SerializeField : it's like when in python we hide the variable of a parent to a child but instead do method like get to obtain / access those.
        //                  ( : so it's making variable only accessible to the parent object and not the childrens (not private since we can
        //                  still modify those in unity debug))
        // ( https://www.youtube.com/watch?v=_9LJqhAj-FU )
        [SerializeField] private LayerMask groundLayer;

        private Rigidbody2D _rb;

        //Movement vars 
        private Vector2 _moveVelocity;
        private bool _isFacingRight;

        // Jump vars 
        private bool _isJumping;
        private int _numberOfJumpsUsed;
        private bool _initJumpCanceled; //For short hops
        private bool _isJumpCanceled;
        private float _jumpCancelTimer;
        private float _jumpCancelMoment;
        private bool _isLanding;

        // Wall slide and Wall jump vars
        private bool _isWallSliding;
        private bool _initWallSliding; // for animation : WallSlideInit
        private bool _isWallSlidingRight;
        private bool _isWallSlidingLeft;
        private float _wallJumpTime;

        // Dash vars
        private bool _isDashing;
        private bool _initDashing;
        private bool _canDash;
        private float _dashTimer;
        private float _dashDuration;
        private float _dashBuffer;

        // Jump buffer and Coyote vars // Explain coyote and buffer: https://www.youtube.com/watch?v=RFix_Kg2Di0 
        private float _jumpBufferTimer;
        private float _coyoteTimer;

        // Collision check vars
        private bool _isGrounded;
        private bool _bumpedHead;
        private bool _bodyRightWalled;
        private bool _bodyLeftWalled;

        // Trigger 
        public CustomTrigger feetTrigger;
        public CustomTrigger headTrigger;
        public CustomTrigger bodyRightTrigger;
        public CustomTrigger bodyLeftTrigger;
        public CustomTrigger landingTrigger;
        public Transform colliders;
        
        // Animation
        private Animator _animator;
        private AnimationEnum _curAnimationState;
        private float _animationTime;
        
        // To fix child rotation
        private bool _justRotated;
        public bool Rotated { private set; get; }

        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
            {
                enabled = false;
                return;
            }
        }

        private void Awake()
        {
            feetTrigger.EnteredTrigger += OnFeetTriggerEntered;
            feetTrigger.ExitedTrigger += OnFeetTriggerExited;
        
            headTrigger.EnteredTrigger += OnHeadTriggerEntered;
            headTrigger.ExitedTrigger += OnHeadTriggerExited;
        
            bodyRightTrigger.EnteredTrigger += OnBodyRightTriggerEntered;
            bodyRightTrigger.ExitedTrigger += OnBodyRightTriggerExited;
        
            bodyLeftTrigger.EnteredTrigger += OnBodyLeftTriggerEntered;
            bodyLeftTrigger.ExitedTrigger += OnBodyLeftTriggerExited;
            
            landingTrigger.EnteredTrigger += OnLandingTriggerEntered;
            
            _isGrounded = false;
            _bumpedHead = false;
            _isDashing = false;

            _numberOfJumpsUsed = 0;

            _isFacingRight = true;

            _rb = GetComponent<Rigidbody2D>(); // it's like when we linked the camera to the player but automaticly, with the rigid body
            _animator = GetComponent<Animator>();

            _animationTime = 0;
        }
        //============================================================================================
        //UPDATES
        //--------------------------------------------------------------------------------------------
        private void FixedUpdate()
        {
            if (!_isDashing) // we don't want to change the velocity during a dash
            {
                Gravity();
            }
            
            Dash();

            MoveHandler();
            
            Jump();
        }

        private void DebugCollision()
        {
            if (_bumpedHead)
            {
                Debug.Log("Bumped Head");
            }
            if (_isGrounded)
            {
                Debug.Log("Grounded");
            }
            if (_bodyLeftWalled)
            {
                Debug.Log("Left Walled");
            }
            if (_bodyRightWalled)
            {
                Debug.Log("Right Walled");
            }
        }
        private void DebugShortUp()
        {
            if (InputManager.JumpWasReleased && _jumpCancelTimer > 0)
            {
                Debug.Log("Jump Should be Canceled");
            }
            if (_initJumpCanceled && _jumpCancelMoment <= 0)
            {
                Debug.Log("Jump just canceled");
                if (_rb.linearVelocityY < 0)
                {
                    Debug.Log("Too late was already falling");
                }
            }
        }
    
        private void Update()
        {
            //DebugCollision();
            //DebugShortUp();
            
            JumpCheck();
        
            DashCheck();

            CheckWallSliding();
            
            CountTimers();
            
            CheckAnimation();

            UpdateRotated(); // Help for child to rotate at the same time as their parent
        }
        //============================================================================================
    
        //============================================================================================
        //REGIONS
        //--------------------------------------------------------------------------------------------
        #region Movement

        private void MoveHandler()
        {
            if (!_isDashing) // we don't want to change the velocity during a dash
            {
                if (_isGrounded)
                {
                    Move(MoveStats.GroundAcceleration, MoveStats.GroundDeceleration,
                        InputManager.Movement); // change the velocity of our player based on the
                }
                else // if not on the ground (in the air)
                {
                    Move(MoveStats.AirAcceleration, MoveStats.AirDeceleration,
                        InputManager.Movement); // same that above but for in the air
                }
            }
        }
        
        private void Move(float acceleration, float deceleration, Vector2 moveInput)
        {
            _moveVelocity = _rb.linearVelocity;

            if (moveInput != Vector2.zero) // if our player moves
            {
                TurnCheck(moveInput);//check if he needs to turn around

                Vector2 targetVelocity;
                //For running

                if (InputManager.RunIsHeld) // if the player is holding the run key
                {
                    targetVelocity = new Vector2(moveInput.x * MoveStats.MaxRunSpeed, 0f);
                }
                else // if the player isn't holding the run key
                {
                    targetVelocity = new Vector2(moveInput.x * MoveStats.MaxWalkSpeed, 0f);
                }
                

                _moveVelocity = Vector2.Lerp(_moveVelocity, targetVelocity, acceleration * Time.fixedDeltaTime); // to accelerate
                // Simply, Lerp is linear interpollation (~ it's taking our current velocity, the objective velocity and it's reaching a
                // (:like we learned in algo taa)           middle value based on the passed time, to not go max speed instantly) 
            }   
            else // if the player stopped
            {
                _moveVelocity = Vector2.Lerp(_moveVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime); // same as before but to decelerate
                if (Abs(_moveVelocity.x) < 0.001f) _moveVelocity.x = 0f; // to make it to zero fast
            }

            _rb.linearVelocity = new Vector2(_moveVelocity.x, _rb.linearVelocity.y); // we change the velocity of the player with new x velocity and current y velocity
        }

        private float Abs(float num)
        {
            return num < 0f ? -num : num;
        }

        private void UpdateRotated()
        {
            if (_justRotated)
            {
                Rotated = true;
                _justRotated = false;
            }
            else if (Rotated)
            {
                Rotated = false;
            }
        }

        private void TurnCheck(Vector2 moveInput)
        {
            FixChildRotation();

            if (_isWallSliding) TurnOnWallSlide();
            else TurnNormal(moveInput);
        }

        private void TurnNormal(Vector2 moveInput)
        {
            if (_isFacingRight && moveInput.x < 0) // moveInput is returning a Vector2 (= 2 value stored together) of x and y 
                // to understand them imagine a joystick, full left is -1 for the first paramether (x) and 0 for the second (y)
                // and so on for every direction (like in a circle)
            {
                RotateRight(false);
            }
            else if (!_isFacingRight && moveInput.x > 0)
            {
                RotateRight(true);
            }
        }

        private void TurnOnWallSlide()
        {
            if (_isFacingRight && _isWallSlidingRight)
            {
                RotateRight(false);
            }
            else if (!_isFacingRight && _isWallSlidingLeft)
            {
                RotateRight(true);
            }
        }

        private void RotateRight(bool on)
        {
            if (on)
            {
                _isFacingRight = true;
                transform.Rotate(0f, 180f, 0f);
            }
            else
            {
                _isFacingRight = false;
                transform.Rotate(0f, -180f, 0f); // rotating the ~rigidBody, not the sprite (so that walking/... remain positive values to go in front of us)
            }

            _justRotated = true;
        }

        private void FixChildRotation()
        {
            colliders.transform.rotation = Quaternion.Euler (0.0f, 0.0f, gameObject.transform.rotation.z * -1.0f);
        }

        #endregion

        #region Jump
        
        private void JumpCheck()
        {
            if (InputManager.JumpWasPressed)
            {
                _jumpBufferTimer = MoveStats.JumpBufferTime;
            }

            if (_jumpBufferTimer > 0 && _numberOfJumpsUsed < MoveStats.NumberOfJumpsAllowed && !_isJumping && !_bumpedHead)
            {
                _isJumping = true;
            }
            
            if (InputManager.JumpWasReleased && _jumpCancelTimer > 0)
            {
                _jumpCancelTimer = 0;
                _initJumpCanceled = true;
            }
        }
        
        private void Jump()
        {
            if (_initJumpCanceled && _jumpCancelMoment <= 0)
            {
                _initJumpCanceled = false;
                _isJumpCanceled = true;
                _jumpCancelMoment = 0;
            }
            
            if (_isJumping && !_isWallSliding)
            {
                _isJumping = false;
                //_wasJumping = true;
                _jumpBufferTimer = 0;
                if (_numberOfJumpsUsed == 0)
                {
                    _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, MoveStats.JumpHeight);
                    _jumpCancelTimer = MoveStats.JumpCancelTime;
                    _jumpCancelMoment = MoveStats.JumpCancelMoment;
                }
                else
                {
                    _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, MoveStats.JumpHeight * MoveStats.MultipleJumpStrengthPercent);
                }
                _numberOfJumpsUsed++;
            }
            
            else if (_isJumping && _isWallSliding)
            {
                if (_isWallSlidingRight)
                {
                    _rb.linearVelocity = new Vector2(-MoveStats.WallJumpStrength, MoveStats.JumpHeight);
                }
                else if (_isWallSlidingLeft)
                {
                    _rb.linearVelocity = new Vector2(MoveStats.WallJumpStrength, MoveStats.JumpHeight);
                }
                _numberOfJumpsUsed++;
                _isJumping = false;
                _jumpBufferTimer = 0;
            }
        }

        #endregion

        #region Dash

        private void DashCheck() // The one in Update
        {
            if (InputManager.DashWasPressed)
            {
                //Debug.Log("Dash");
                _dashBuffer = MoveStats.DashBufferTime;
            }

            if (_isGrounded)
            {
                _canDash = true;
            }

            if (_dashBuffer > 0 && _canDash && !_isDashing && _dashTimer <= 0)
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
            if (_initDashing)
            {
                _dashBuffer = 0;
                _isDashing = true;
                _dashTimer = MoveStats.DashTimer;
                _dashDuration = MoveStats.DashDuration;
                _canDash = false;

                if (InputManager.Movement != Vector2.zero)
                {
                    _rb.linearVelocity = InputManager.Movement * (MoveStats.MaxWalkSpeed * MoveStats.DashStrength);
                }
                else
                {
                    Vector2 direction = new Vector2(1,0); // if he is facing right
                    if (!_isFacingRight) // if he is facing left 
                    {
                        direction = new Vector2(-1, 0);
                    }
                    _rb.linearVelocity = direction * (MoveStats.MaxWalkSpeed * MoveStats.DashStrength);
                }

                _initDashing = false;
            }
        }

        #endregion

        #region Gravity

        private void Gravity()
        {
            if (!_isGrounded) // _isWallSliding is there so that we can wall jump while being on the ground
            {
                float usedGravity = MoveStats.GravityForce;
                if (_rb.linearVelocity.y <= 0 || _bumpedHead || _isJumpCanceled)
                {
                    usedGravity = MoveStats.GravityFallForce; // to make a beautiful jump curve
                }
                
                if (_isJumpCanceled) ApplyJumpCancelStrength();
                
                Vector2 targetVelocity = _isWallSliding ? GravityWallSlide() : GravityNormal();
                
                Vector2 airVelocity = new Vector2(0f, _rb.linearVelocity.y);
    
                airVelocity = Vector2.Lerp(airVelocity, targetVelocity, usedGravity * Time.fixedDeltaTime);
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, airVelocity.y);
            }

            else if (_isGrounded)
            {
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, 0f);
            }
        }

        private void ApplyJumpCancelStrength()
        {
            if (_rb.linearVelocity.y > 0)
            {
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _rb.linearVelocity.y * MoveStats.JumpCancelStrength);
            }
            _isJumpCanceled = false;
        }

        private Vector2 GravityNormal()
        {
            // may help when we want to change his gravity in some area.
            
            return new Vector2(0f, -MoveStats.MaxFallSpeed);
        }

        private Vector2 GravityWallSlide()
        {
            Vector2 targetVelocity;
            
            _rb.linearVelocityX = 0f;
            if (_rb.linearVelocityY > 0f)
            {
                targetVelocity = new Vector2(0f, -MoveStats.MaxFallSpeed);
                //_rb.linearVelocity = new Vector2(_rb.linearVelocityX, 0f);
            }
            else
            {
                targetVelocity = new Vector2(0f, -MoveStats.WallSlideMaxSpeed);
            }

            return targetVelocity;
        }

        #endregion

        #region Wall Sliding

        private void ResetWallSliding()
        {
            _isWallSliding = false;
            _isWallSlidingRight = false;
            _isWallSlidingLeft = false;
        }
        
        private void CheckWallSliding()
        {
            if (_bodyRightWalled && InputManager.Movement.x > 0 && !_isGrounded)
            {
                _isWallSlidingRight = true;
            }
            else if (_bodyLeftWalled && InputManager.Movement.x < 0 && !_isGrounded)
            {
                _isWallSlidingLeft = true;
            }

            if ((_isWallSlidingRight || _isWallSlidingLeft) && !_isWallSliding && !_initWallSliding) // just so that the value initWallSliding is well updated
            {
                _initWallSliding = true;
            }
            else if (_initWallSliding)
            {
                _initWallSliding = false;
            }
            
            _isWallSliding = _isWallSlidingRight || _isWallSlidingLeft;

            if (_isWallSliding)
            {
                if (_isGrounded) ResetWallSliding();
                else if (_isWallSlidingRight && InputManager.Movement.x < 0) ResetWallSliding();
                else if (_isWallSlidingLeft && InputManager.Movement.x > 0) ResetWallSliding();
                else if (!_bodyLeftWalled && !_bodyRightWalled) ResetWallSliding();
                else
                {
                    _canDash = true;
                    _numberOfJumpsUsed = 0;
                }
            }
        }
        
        #endregion
        
        #region Collision

        void ResetJumpValues()
        {
            _numberOfJumpsUsed = 0;
            _isJumpCanceled = false;
            _isJumping = false;
            _jumpCancelMoment = 0;
            _jumpCancelTimer = 0;
            _initJumpCanceled = false;
        }

        void OnFeetTriggerEntered(Collider2D item)
        {
            _isGrounded = true;
            ResetJumpValues();
            _canDash = true;
        }

        void OnFeetTriggerExited(Collider2D item)
        {
            _isGrounded = false;
            if (_numberOfJumpsUsed == 0) _numberOfJumpsUsed = 1;
            _isLanding = false;
        }
    
        void OnHeadTriggerEntered(Collider2D item)
        {
            _bumpedHead = true;
        }

        void OnHeadTriggerExited(Collider2D item)
        {
            _bumpedHead = false;
        }
    
        void OnBodyRightTriggerEntered(Collider2D item)
        {
            _bodyRightWalled = true;
        }

        void OnBodyRightTriggerExited(Collider2D item)
        {
            _bodyRightWalled = false;
        }
    
        void OnBodyLeftTriggerEntered(Collider2D item)
        {
            _bodyLeftWalled = true;
        }

        void OnBodyLeftTriggerExited(Collider2D item)
        {
            _bodyLeftWalled = false;
        }
        
        void OnLandingTriggerEntered(Collider2D item)
        {
            _isLanding = true;
        }
        
        #endregion
        
        #region Animation

        private string GetEnumName(AnimationEnum parameter)
        {
            switch (parameter)
            {
                case AnimationEnum.Idle:
                    return "Idle";
                case AnimationEnum.Walk:
                    return "Walk";
                case AnimationEnum.Run:
                    return "Run";
                case AnimationEnum.JumpInit:
                    return "JumpInit";
                case AnimationEnum.JumpIdle:
                    return "JumpIdle";
                case AnimationEnum.MultipleJump:
                    return "MultipleJump";
                case AnimationEnum.Landing:
                    return "Landing";
                case AnimationEnum.WallSlideInit:
                    return "WallSlideInit";
                case AnimationEnum.WallSlide:
                    return "WallSlide";
                case AnimationEnum.WallSlideLanding:
                    return "WallSlideLanding";
                default:
                    Debug.Log("What is that : " + parameter);
                    return "";
            }
        }
        
        private void ChangeAnimationState(AnimationEnum newState, float time = 0)
        {
            if (_curAnimationState == newState) return;
            
            if (_animationTime <= 0)
            {
                if (time != 0)
                {
                    _animationTime = time;
                }
                
                _animator.Play(GetEnumName(newState));
                
                _curAnimationState = newState;
            }
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void CheckAnimation()
        {
            // need order of priority :
            
            if (_initWallSliding) // maybe don't need wall slide init
            {
                ChangeAnimationState(AnimationEnum.WallSlideInit, 0.30f);
            }
            /*else if (_isWallSliding && _isJumping)
            {
                ChangeAnimationState(AnimationEnum.WallJump,0.3f);
            }*/
            else if (_isWallSliding && _isLanding && !_isGrounded)
            {
                ChangeAnimationState(AnimationEnum.WallSlideLanding, 0.25f);
            }
            else if (_isWallSliding)
            {
                ChangeAnimationState(AnimationEnum.WallSlide);
            }
            else if (_isJumping && !_bumpedHead)
            {
                if (_numberOfJumpsUsed == 0)
                {
                    ChangeAnimationState(AnimationEnum.JumpInit, 0.20f);
                }
                else
                {
                    ChangeAnimationState(AnimationEnum.MultipleJump, 0.20f);
                }
            }
            else if (_isLanding && !_isGrounded)
            {
                ChangeAnimationState(AnimationEnum.Landing);
            }
            else if (!_isGrounded)
            {
                ChangeAnimationState(AnimationEnum.JumpIdle);
            }
            else if (Abs(_rb.linearVelocityX) > 0.1 && InputManager.RunIsHeld)
            {
                ChangeAnimationState(AnimationEnum.Run);
            }
            else if (Abs(_rb.linearVelocityX) > 0.1)
            {
                ChangeAnimationState(AnimationEnum.Walk);
            }
            else if (Abs(_rb.linearVelocityX) <= 0.1)
            {
                ChangeAnimationState(AnimationEnum.Idle);
            }
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
            if (_jumpCancelTimer > 0)
            {
                _jumpCancelTimer -= deltaTime;
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
            if (_dashBuffer > 0)
            {
                _dashBuffer -= deltaTime;
            }
            if (_animationTime > 0)
            {
                _animationTime -= deltaTime;
            }
        }

        #endregion
        //============================================================================================

    }
}
