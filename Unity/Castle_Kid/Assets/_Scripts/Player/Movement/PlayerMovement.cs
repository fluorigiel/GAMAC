using System.Numerics;
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
        public Vector2 _moveVelocity;
        private bool _isFacingRight;

        // Jump vars 
        private bool _isJumping;
        private int _numberOfJumpsUsed;
        private bool _initJumpCanceled; //For short hops
        private bool _isJumpCanceled;
        private float _jumpCancelTimer;
        private float _jumpCancelMoment;

        // Wall slide and Wall jump vars
        private bool _isWallSliding;
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
        public CustomTrigger feetTriger;
        public CustomTrigger headTriger;
        public CustomTrigger bodyRightTriger;
        public CustomTrigger bodyLeftTriger;
        public Transform colliders;

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
            feetTriger.EnteredTrigger += OnFeetTriggerEntered;
            feetTriger.ExitedTrigger += OnFeetTriggerExited;
        
            headTriger.EnteredTrigger += OnHeadTriggerEntered;
            headTriger.ExitedTrigger += OnHeadTriggerExited;
        
            bodyRightTriger.EnteredTrigger += OnBodyRightTriggerEntered;
            bodyRightTriger.ExitedTrigger += OnBodyRightTriggerExited;
        
            bodyLeftTriger.EnteredTrigger += OnBodyLeftTriggerEntered;
            bodyLeftTriger.ExitedTrigger += OnBodyLeftTriggerExited;
            
            _isGrounded = false;
            _bumpedHead = false;
            _isDashing = false;

            _numberOfJumpsUsed = 0;

            _isFacingRight = true;

            _rb = GetComponent<Rigidbody2D>(); // it's like when we linked the camera to the player but automaticly, with the rigid body
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
                Debug.Log("Bumbed Head");
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
    
        private void Update()
        {
            //DebugCollision();
            
            JumpCheck();
        
            DashCheck();

            CheckWallSliding();
            
            CountTimers();
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
                _isFacingRight = false;
                transform.Rotate(0f, -180f, 0f);
            }
            else if (!_isFacingRight && moveInput.x > 0)
            {
                _isFacingRight = true;
                transform.Rotate(0f, 180f, 0f);  // rotating the ~rigidBody, not the sprite (so that walking/... remain positive values to go in front of us)
            }
        }

        private void TurnOnWallSlide()
        {
            if (_isFacingRight && _isWallSlidingRight)
            {
                _isFacingRight = false;
                transform.Rotate(0f, -180f, 0f);
            }
            if (!_isFacingRight && _isWallSlidingLeft)
            {
                _isFacingRight = true;
                transform.Rotate(0f, 180f, 0f);
            }
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
            }
            
            if (_isJumping && !_isWallSliding)
            {
                _isJumping = false;
                _jumpBufferTimer = 0;
                if (_numberOfJumpsUsed == 0)
                {
                    _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, MoveStats.JumpHeight);
                    _jumpCancelTimer = MoveStats.JumpCancelTime;
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
        
        /*
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
            }

            if (_isJumping && !_isWallSliding)
            {
                _isJumping = false;
                _jumpBufferTimer = 0;
                if (_numberOfJumpsUsed == 0)
                {
                    _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, MoveStats.JumpHeight);
                    _jumpCancelTimer = MoveStats.JumpCancelTime;
                }
                else
                {
                    _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, MoveStats.JumpHeight * MoveStats.MultipleJumpStrengthPercent);
                }
                _numberOfJumpsUsed++;
            }
            else if (_isJumping && _isWallSliding)
            {
                if (_bodyRightWalled)
                {
                    _rb.linearVelocity = new Vector2(-MoveStats.WallJumpStrength, MoveStats.JumpHeight);
                }
                else if (_bodyLeftWalled)
                {
                    _rb.linearVelocity = new Vector2(MoveStats.WallJumpStrength, MoveStats.JumpHeight);
                }
                _numberOfJumpsUsed++;
                _isJumping = false;
                _jumpBufferTimer = 0;
            }
        }*/

        #endregion

        #region Dash

        private void DashCheck() // The one in Update
        {
            if (InputManager.DashWasPressed)
            {
                //Debug.Log("Dash");
                _dashBuffer = MoveStats.DashBufferTime;
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
        
        /*private void Gravity()
        {
            if (!_isGrounded || (_isWallSliding && !_isGrounded)) // _isWallSliding is there so that we can wall jump while being on the ground
            {
                float usedGravity = MoveStats.GravityForce;
                if (_rb.linearVelocity.y <= 0 || _bumpedHead || _isJumpCanceled)
                {
                    usedGravity = MoveStats.GravityFallForce; // to make a beautiful jump curve
                }

                Vector2 targetVelocity = new Vector2(0f, -MoveStats.MaxFallSpeed);

                //Interactions with walls (wall slide)

                if (_isWallSliding && !_isGrounded  && ((_bodyRightWalled && InputManager.Movement == Vector2.right) || (_bodyLeftWalled && InputManager.Movement == Vector2.left))) // we don't want to be stopped in the middle of the wall
                {
                    if (_rb.linearVelocityY > 0f) { _rb.linearVelocity = new Vector2(_rb.linearVelocityX, 0f); }
                    _rb.linearVelocityX = 0f;

                    targetVelocity = new Vector2(0f, -MoveStats.WallSlideMaxSpeed);
                    _numberOfJumpsUsed = 0;
                    _isWallSliding = true;
                }
                else
                {
                    _isWallSliding = false;
                }

                Vector2 airVelocity = new Vector2(0f, _rb.linearVelocity.y);
    
                airVelocity = Vector2.Lerp(airVelocity, targetVelocity, usedGravity * Time.fixedDeltaTime);
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, airVelocity.y);
            }

            else if (_isGrounded)
            {
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, 0f);
                _isWallSliding = false;
            }
        }*/

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

        /*private bool IsGroundUnder()
        {
            return Physics2D.BoxCastAll(transform.position, feetBoxSize, 0f, Vector3.down, feetCastDistance).Length > 1;
        }*/

        /*private bool IsTouchingGround()
    {
        return _onCollisionVal;
    }
    
    #region OnCollision

    private void OnCollisionEnter(Collision touching)
    {
        _onCollisionVal = (touching.gameObject.layer == groundLayer || touching.gameObject.CompareTag("Player"));
    }

    private void OnCollisionExit(Collision touching)
    {
        if (touching.gameObject.layer == groundLayer || touching.gameObject.CompareTag("Player") && _onCollisionVal)
            _onCollisionVal = false;
    }

    #endregion*/

        /*private void UpdateGrounded()
        {
            // BoxCast is quite ugly but simply projecting a box to see if there is an object
            if (_rb.linearVelocityY > 0)
            {
                Debug.Log(_rb.linearVelocityY);
                Debug.Log("Is wall sliding ?" + _isWallSliding);
            }
            if (IsGroundUnder() && _rb.linearVelocity.y <= 0)
            {
                _isGrounded = true;
                _numberOfJumpsUsed = 0;
                _isJumpCanceled = false;
                _isWallSliding = false;
                _canDash = true;
            }
            else
            {
                _isGrounded = false;
            }
        }

        private void UpdateBumpedHead()
        {
            // https://www.youtube.com/watch?v=CKz4vTWshuc

            if (Physics2D.BoxCastAll(transform.position, headBoxSize, 0f, Vector3.up, headCastDistance).Length > 1 && _rb.linearVelocity.y >= 0)
            {
                _bumpedHead = true;
            }
            else
            {
                _bumpedHead = false;
            }
        }

        private void UpdateWalledBodyRight()
        {
            if (Physics2D.BoxCastAll(transform.position, bodyRightBoxSize, 0f, Vector3.right, bodyRightCastDistance).Length > 1)
            {
                _bodyRightWalled = true;
                if (InputManager.Movement == Vector2.right && !IsGroundUnder())
                {
                    _canDash = true;
                    _isWallSliding = true;
                }
            }
            else
            {
                _bodyRightWalled = false;
                if (!_bodyLeftWalled)
                {
                    _isWallSliding = false;
                }
            }
        }


        private void UpdateWalledBodyLeft()
        {
            if (Physics2D.BoxCastAll(transform.position, bodyLeftBoxSize, 0f, Vector3.left, bodyLeftCastDistance).Length > 1)
            {
                _bodyLeftWalled = true;
                if (InputManager.Movement == Vector2.left && !IsGroundUnder())
                {
                    _canDash = true;
                    _isWallSliding = true;
                }
            }
            else
            {
                _bodyLeftWalled = false;
                if (!_bodyRightWalled)
                {
                    _isWallSliding = false;
                }
            }
        }*/
        /*
    private void UpdateGrounded()
    {
        // BoxCast is quite ugly but simply projecting a box to see if there is an object
        if (Physics2D.BoxCast(transform.position, feetBoxSize, 0f, Vector3.down, feetCastDistance, groundLayer) && _rb.linearVelocity.y <= 0)
        {
            _isGrounded = true;
            _numberOfJumpsUsed = 0;
            _isJumpCanceled = false;
            _isWallSliding = false;
            _canDash = true;
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

    private void UpdateWalledBodyRight()
    {
        if (Physics2D.BoxCast(transform.position, bodyRightBoxSize, 0f, Vector3.right, bodyRightCastDistance, groundLayer))
        {
            _bodyRightWalled = true;
            if (InputManager.Movement == Vector2.right)
            {
                _canDash = true;
                _isWallSliding = true;
            }
        }
        else
        {
            _bodyRightWalled = false;
            if (!_bodyLeftWalled)
            {
                _isWallSliding = false;
            }
        }
    }

    private void UpdateWalledBodyLeft()
    {
        if (Physics2D.BoxCast(transform.position, bodyLeftBoxSize, 0f, Vector3.left, bodyLeftCastDistance, groundLayer))
        {
            _bodyLeftWalled = true;
            if (InputManager.Movement == Vector2.left)
            {
                _canDash = true;
                _isWallSliding = true;
            }
        }
        else
        {
            _bodyLeftWalled = false;
            if (!_bodyRightWalled)
            {
                _isWallSliding = false;
            }
        }
    }
*/
        /*private void UpdateCollision()
        {
            UpdateGrounded();
            UpdateBumpedHead();
            UpdateWalledBodyRight();
            UpdateWalledBodyLeft();
        }*/

        void OnFeetTriggerEntered(Collider2D item)
        {
            _isGrounded = true;
            _numberOfJumpsUsed = 0;
            _isJumpCanceled = false;
            _canDash = true;
        }

        void OnFeetTriggerExited(Collider2D item)
        {
            _isGrounded = false;
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
        }

        #endregion
        //============================================================================================

    }
}

/*#region Movement

        private void Move(float acceleration, float deceleration, Vector2 moveInput)
        {
            _moveVelocity = _rb.linearVelocity;

            if (moveInput != Vector2.zero) // if our player moves
            {

                TurnCheck(moveInput);//check if he needs to turn around

                Vector2 targetVelocity = Vector2.zero;
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

        private void TurnCheck(Vector2 moveInput)
        {
            if (_isFacingRight && moveInput.x < 0) // moveInput is returning a Vector2 (= 2 value stored together) of x and y 
                // to understand them imagine a joystick, full left is -1 for the first paramether (x) and 0 for the second (y)
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
            }

            if (_isJumping && !_isWallSliding)
            {
                _isJumping = false;
                _jumpBufferTimer = 0;
                if (_numberOfJumpsUsed == 0)
                {
                    _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, MoveStats.JumpHeight);
                    _jumpCancelTimer = MoveStats.JumpCancelTime;
                }
                else
                {
                    _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, MoveStats.JumpHeight * MoveStats.MultipleJumpStrengthPercent);
                }
                _numberOfJumpsUsed++;
            }
            else if (_isJumping && _isWallSliding)
            {
                if (_bodyRightWalled)
                {
                    _rb.linearVelocity = new Vector2(-MoveStats.WallJumpStrength, MoveStats.JumpHeight);
                }
                else if (_bodyLeftWalled)
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
                //Debug.Log("Start Dashing");
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
            if (!_isGrounded || (_isWallSliding && !_isGrounded)) // _isWallSliding is there so that we can wall jump while being on the ground
            {
                float usedGravity = MoveStats.GravityForce;
                if (_rb.linearVelocity.y <= 0 || _bumpedHead || _isJumpCanceled)
                {
                    usedGravity = MoveStats.GravityFallForce; // to make a beautiful jump curve
                }

                Vector2 targetVelocity = new Vector2(0f, -MoveStats.MaxFallSpeed);

                //Interactions with walls (wall slide)

                if (_isWallSliding && !_isGrounded  && ((_bodyRightWalled && InputManager.Movement == Vector2.right) || (_bodyLeftWalled && InputManager.Movement == Vector2.left))) // we don't want to be stopped in the middle of the wall
                {
                    if (_rb.linearVelocityY > 0f) { _rb.linearVelocity = new Vector2(_rb.linearVelocityX, 0f); }
                    _rb.linearVelocityX = 0f;

                    targetVelocity = new Vector2(0f, -MoveStats.WallSlideMaxSpeed);
                    _numberOfJumpsUsed = 0;
                    _isWallSliding = true;
                }
                else
                {
                    _isWallSliding = false;
                }

                Vector2 airVelocity = new Vector2(0f, _rb.linearVelocity.y);
    
                airVelocity = Vector2.Lerp(airVelocity, targetVelocity, usedGravity * Time.fixedDeltaTime);
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, airVelocity.y);
            }

            else if (_isGrounded)
            {
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, 0f);
                _isWallSliding = false;
            }
        }

        #endregion*/

