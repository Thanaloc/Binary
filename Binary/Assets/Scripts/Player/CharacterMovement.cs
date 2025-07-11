using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CharacterMovement : MonoBehaviour
{
    #region SerializeFields

    [SerializeField] private Rigidbody2D _Rigidbody;
    [SerializeField] private BoxCollider2D _CharacterColliderForGroundChecking;
    [SerializeField] private PlayerSO _CharacterIdentity;
    [SerializeField] private InputActionReference _MoveInputs;

    [SerializeField] private BoxCollider2D _DeathCollider;
    [SerializeField] private BoxCollider2D _VictoryCollider;
    #endregion

    #region Variables

    private Vector2 _moveVelocity;
    private Vector2 _targetSpeed;

    private RaycastHit2D _groundHit;

    public float VerticalVelocity { get; private set; }
    private bool _isJumping;
    private bool _isFastFalling;
    private bool _isFalling;
    private float _fastFallTime;
    private float _fastFallReleaseSpeed;
    private int _numberOfJumpsUsed;

    private float _apexPoint;
    private float _timePastApexThreshold;
    private bool _isPastApexThreshold;

    private float _jumpBufferTimer;
    private bool _jumpReleaseDuringBuffer;

    private float _coyoteTimer;

    private bool _bumpHead;

    #endregion

    private void Awake()
    {
        _CharacterIdentity.IsFacingRight = true;
    }

    void Update()
    {
        CountTimers();
        JumpChecks();
    }

    void FixedUpdate()
    {
        CollisionChecks();
        Jump();

        if (_CharacterIdentity.IsGrounded)
        {
            Run(_CharacterIdentity.GroundedAccel, _CharacterIdentity.GroundedDeccel);
        }

        else
        {
            Run(_CharacterIdentity.AirAccel, _CharacterIdentity.AirDeccel);
        }
    }

    #region Movements
    private void Run(float p_accel, float p_deccel)
    {
        if (InputManager.Movement != Vector2.zero)
        {
            TurnCheck();

            _targetSpeed = Vector2.zero;

            _targetSpeed = new Vector2(InputManager.Movement.x, 0f) * _CharacterIdentity.MaxSpeed;

            _moveVelocity = Vector2.Lerp(_moveVelocity, _targetSpeed, p_accel * Time.fixedDeltaTime);
            _Rigidbody.linearVelocity = new Vector2(_moveVelocity.x, _Rigidbody.linearVelocity.y);
        }

        else if (InputManager.Movement == Vector2.zero)
        {
            _moveVelocity = Vector2.Lerp(_moveVelocity, Vector2.zero, p_deccel * Time.fixedDeltaTime);
            _Rigidbody.linearVelocity = new Vector2(_moveVelocity.x, _Rigidbody.linearVelocity.y);
        }
    }

    private void TurnCheck()
    {
        if (_CharacterIdentity.IsFacingRight && InputManager.Movement.x < 0)
        {
            Turn(false);
        }

        else if (!_CharacterIdentity.IsFacingRight && InputManager.Movement.x > 0)
        {
            Turn(true);
        }
    }

    private void Turn(bool p_turnRight)
    {
        if (p_turnRight)
        {
            _CharacterIdentity.IsFacingRight = true;
            transform.Rotate(0f, 180f, 0f);
        }

        else
        {
            _CharacterIdentity.IsFacingRight = false;
            transform.Rotate(0f, -180f, 0f);
        }
    }

    #endregion

    #region Jumping

    void JumpChecks()
    {
        if (InputManager.JumpWasPressed)
        {
            _jumpBufferTimer = _CharacterIdentity.BufferTime;
            _jumpReleaseDuringBuffer = false;
        }

        if (InputManager.JumpWasReleased)
        {
            if (_jumpBufferTimer > 0)
            {
                _jumpReleaseDuringBuffer = true;
            }

            if (_isJumping && VerticalVelocity > 0f)
            {
                if (_isPastApexThreshold)
                {
                    _isPastApexThreshold = false;
                    _isFastFalling = true;
                    _fastFallTime = _CharacterIdentity.TimeForUpwardsCancel;
                    VerticalVelocity = 0;
                }

                else
                {
                    _isFastFalling = true;
                    _fastFallReleaseSpeed = VerticalVelocity;
                }
            }
        }

        if (_jumpBufferTimer > 0 && !_isJumping && (_CharacterIdentity.IsGrounded || _coyoteTimer > 0))
        {
            InitiateJump(1);

            if (_jumpReleaseDuringBuffer)
            {
                _isFastFalling = true;
                _fastFallReleaseSpeed = VerticalVelocity;
            }
        }
        else if (_jumpBufferTimer > 0 && _isJumping && _numberOfJumpsUsed < _CharacterIdentity.NumberOfJumps)
        {
            _isFastFalling = false;
            InitiateJump(1);
        }

        else if (_jumpBufferTimer > 0 && _isFalling && _numberOfJumpsUsed < _CharacterIdentity.NumberOfJumps - 1)
        {
            InitiateJump(2);
            _isFastFalling = false;
        }

        //landing

        if ((_isJumping || _isFalling) && _CharacterIdentity.IsGrounded && VerticalVelocity <= 0)
        {
            _isJumping = false;
            _isFalling = false;
            _isFastFalling = false;
            _fastFallTime = 0;
            _isPastApexThreshold = false;
            _numberOfJumpsUsed = 0;

            VerticalVelocity = Physics2D.gravity.y;
        }

    }

    private void InitiateJump(int p_numberOfJumpsUsed)
    {
        if (!_isJumping)
        {
            _isJumping = true;
        }

        _jumpBufferTimer = 0;
        _numberOfJumpsUsed += p_numberOfJumpsUsed;
        VerticalVelocity = _CharacterIdentity.InitialJumpVelocity;
    }

    private void Jump()
    {
        //apply grav
        if (_isJumping)
        {
            //check head bump
            if (_bumpHead)
            {
                _isFastFalling = true;
            }

            //gravity on asending
            if (VerticalVelocity >= 0)
            {
                //apex controls
                _apexPoint = Mathf.InverseLerp(_CharacterIdentity.InitialJumpVelocity, 0, VerticalVelocity);

                if (_apexPoint > _CharacterIdentity.ApexThreshold)
                {
                    if (!_isPastApexThreshold)
                    {
                        _isPastApexThreshold = true;
                        _timePastApexThreshold = 0;
                    }

                    if (_isPastApexThreshold)
                    {
                        _timePastApexThreshold += Time.fixedDeltaTime;
                        if (_timePastApexThreshold < _CharacterIdentity.ApexHangTime)
                        {
                            VerticalVelocity = 0;
                        }

                        else
                        {
                            VerticalVelocity = -.01f;
                        }
                    }
                }

                else
                {
                    VerticalVelocity += _CharacterIdentity.Gravity * Time.fixedDeltaTime;
                    if (_isPastApexThreshold)
                    {
                        _isPastApexThreshold = false;
                    }
                }
            }

            //grav on desending
            else if (!_isFastFalling)
            {
                VerticalVelocity += _CharacterIdentity.Gravity * _CharacterIdentity.GravityOnReleaseMultipl * Time.fixedDeltaTime;
            }

            else if (VerticalVelocity < 0)
            {
                if (!_isFalling)
                {
                    _isFalling = true;
                }
            }
        }

        //jump cut
        if (_isFastFalling)
        {
            if (_fastFallTime >= _CharacterIdentity.TimeForUpwardsCancel)
            {
                VerticalVelocity += _CharacterIdentity.Gravity * _CharacterIdentity.GravityOnReleaseMultipl * Time.fixedDeltaTime;
            }

            else if (_fastFallTime < _CharacterIdentity.TimeForUpwardsCancel)
            {
                VerticalVelocity = Mathf.Lerp(_fastFallReleaseSpeed, 0, (_fastFallTime / _CharacterIdentity.TimeForUpwardsCancel));
            }

            _fastFallTime += Time.fixedDeltaTime;
        }

        //normal grav while falling
        if (!_CharacterIdentity.IsGrounded && !_isJumping)
        {
            if(!_isFalling)
            {
                _isFalling = true;
            }

            VerticalVelocity += _CharacterIdentity.Gravity * Time.fixedDeltaTime;
        }

        //clamp fall speed
        VerticalVelocity = Mathf.Clamp(VerticalVelocity, -_CharacterIdentity.MaxFallSpeed, 50f);

        _Rigidbody.linearVelocity = new Vector2(_Rigidbody.linearVelocity.x, VerticalVelocity);
    }

    #endregion

    #region Collision Checks

    private void GroundCheck()
    {
        Vector2 boxCastOrigin = new Vector2(_CharacterColliderForGroundChecking.bounds.center.x, _CharacterColliderForGroundChecking.bounds.min.y);
        Vector2 boxCastSize = new Vector2(_CharacterColliderForGroundChecking.bounds.size.x, _CharacterIdentity.GroundDetectionRayLenght);

        _groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, _CharacterIdentity.GroundDetectionRayLenght, _CharacterIdentity.GroundLayer);

        if (_groundHit.collider != null)
        {
            _CharacterIdentity.IsGrounded = true;
        }

        else
            _CharacterIdentity.IsGrounded = false;
    }

    private void BumpedHeadCheck()
    {
        Vector2 boxCastOrigin = new Vector2(_CharacterColliderForGroundChecking.bounds.center.x, _CharacterColliderForGroundChecking.bounds.min.y);
        Vector2 boxCastSize = new Vector2(_CharacterColliderForGroundChecking.bounds.size.x * _CharacterIdentity.HeadWidth, _CharacterIdentity.HeadDetectionRayLenght);

        _groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.up, _CharacterIdentity.HeadDetectionRayLenght, _CharacterIdentity.GroundLayer);

        if (_groundHit.collider != null)
        {
            _bumpHead = true;
        }

        else
            _bumpHead = false;
    }

    private void CollisionChecks()
    {
        GroundCheck();
        BumpedHeadCheck();
    }

    #endregion

    #region Timers

    private void CountTimers()
    {
        _jumpBufferTimer -= Time.deltaTime;

        if (!_CharacterIdentity.IsGrounded)
        {
            _coyoteTimer -= Time.deltaTime;
        }

        else
            _coyoteTimer = _CharacterIdentity.CoyoteTime;
    }

    #endregion

    #region TO REMOVE

    private IEnumerator VictoryCountdownTEMP()
    {
        int secs = 0;

        while (secs < 3)
        {
            yield return new WaitForSeconds(1);
            secs++;
        }

        SceneManager.LoadScene("TestFeaturesScene");
    }

    #endregion
}
