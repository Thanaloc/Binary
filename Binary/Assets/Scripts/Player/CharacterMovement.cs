using System;
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
    [SerializeField] private PlayerController _PlayerController;
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

    public Action<bool> JumpAction;
    public Action<bool> MoveAction;

    #endregion

    private void Awake()
    {
        _PlayerController.IsFacingRight = true;
        VerticalVelocity = 0f;
    }

    void Update()
    {
        CountTimers();
        JumpChecks();
        ResetSceneCheck();
    }

    void FixedUpdate()
    {
        CollisionChecks();
        Jump();

        if (_PlayerController.IsGrounded)
        {
            Run(_CharacterIdentity.GroundedAccel, _CharacterIdentity.GroundedDeccel);
        }
        else
        {
            Run(_CharacterIdentity.AirAccel, _CharacterIdentity.AirDeccel);
        }
    }

    #region Debug

    private void ResetSceneCheck()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    #endregion

    #region Movements
    private void Run(float p_accel, float p_deccel)
    {
        //if (_PlayerController.IsUsingSpecialMovementAbility) //in case of me wanting to have different movement based in the grappling situation
        //    return; 

        if (InputManager.Movement != Vector2.zero)
        {
            MoveAction?.Invoke(true);

            TurnCheck();

            _targetSpeed = Vector2.zero;
            _targetSpeed = new Vector2(InputManager.Movement.x, 0f) * _CharacterIdentity.MaxSpeed;

            _moveVelocity = Vector2.Lerp(_moveVelocity, _targetSpeed, p_accel * Time.fixedDeltaTime);
            _Rigidbody.linearVelocity = new Vector2(_moveVelocity.x, _Rigidbody.linearVelocity.y);
        }
        else if (InputManager.Movement == Vector2.zero)
        {
            MoveAction?.Invoke(false);

            _moveVelocity = Vector2.Lerp(_moveVelocity, Vector2.zero, p_deccel * Time.fixedDeltaTime);
            _Rigidbody.linearVelocity = new Vector2(_moveVelocity.x, _Rigidbody.linearVelocity.y);
        }
    }

    private void TurnCheck()
    {
        if (_PlayerController.IsFacingRight && InputManager.Movement.x < 0)
        {
            Turn(false);
        }
        else if (!_PlayerController.IsFacingRight && InputManager.Movement.x > 0)
        {
            Turn(true);
        }
    }

    private void Turn(bool p_turnRight)
    {
        if (p_turnRight)
        {
            _PlayerController.IsFacingRight = true;
            transform.Rotate(0f, 180f, 0f);
        }
        else
        {
            _PlayerController.IsFacingRight = false;
            transform.Rotate(0f, -180f, 0f);
        }
    }

    #endregion

    #region Jumping

    void JumpChecks()
    {
        if (!_PlayerController.CanJump)
            return;
        
        if (InputManager.JumpWasPressed)
        {
            _jumpBufferTimer = _CharacterIdentity.BufferTime;
            _jumpReleaseDuringBuffer = false;
        }

        if (InputManager.JumpWasReleased)
        {
            //if (_jumpBufferTimer > 0)
            //{
            //    _jumpReleaseDuringBuffer = true;
            //}

            //if (_isJumping && VerticalVelocity > 0f)
            //{
            //    if (_isPastApexThreshold)
            //    {
            //        _isPastApexThreshold = false;
            //        _isFastFalling = true;
            //        _fastFallTime = _CharacterIdentity.TimeForUpwardsCancel;
            //        VerticalVelocity = 0;
            //    }
            //    else
            //    {
            //        _isFastFalling = true;
            //        _fastFallReleaseSpeed = VerticalVelocity;
            //    }
            //}
        }

        if (_jumpBufferTimer > 0 && (_PlayerController.IsGrounded || _coyoteTimer > 0) && _numberOfJumpsUsed == 0)
        {
            InitiateJump(1);

            if (_jumpReleaseDuringBuffer)
            {
                _isFastFalling = true;
                _fastFallReleaseSpeed = VerticalVelocity;
            }
        }
        // Double saut en l'air
        else if (_jumpBufferTimer > 0 && _isJumping && _numberOfJumpsUsed < _CharacterIdentity.NumberOfJumps)
        {
            _isFastFalling = false;
            InitiateJump(1);
        }

        //Atterrissage
        if (_PlayerController.IsGrounded && VerticalVelocity <= 0 && (_isJumping || _isFalling))
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
        _isJumping = true;
        _isFalling = false;
        _jumpBufferTimer = 0;
        _numberOfJumpsUsed += p_numberOfJumpsUsed;
        VerticalVelocity = _CharacterIdentity.InitialJumpVelocity;

    }

    private void Jump()
    {
        // Physique du saut
        if (_isJumping)
        {
            // Vérification collision tête
            if (_bumpHead)
            {
                _isFastFalling = true;
            }

            // Gravité en montée
            if (VerticalVelocity >= 0)
            {
                // Contrôles de l'apex
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
                            VerticalVelocity = -0.01f;
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

            else if (!_isFastFalling)
            {
                VerticalVelocity += _CharacterIdentity.Gravity /* * _CharacterIdentity.GravityOnReleaseMultipl*/ * Time.fixedDeltaTime;
            }

            // Gravité en descente
            else if (VerticalVelocity < 0)
            {
                if (!_isFalling)
                {
                    _isFalling = true;
                }
            }
        }

        // Fast fall
        //if (_isFastFalling)
        //{
        //    if (_fastFallTime >= _CharacterIdentity.TimeForUpwardsCancel)
        //    {
        //        VerticalVelocity += _CharacterIdentity.Gravity * _CharacterIdentity.GravityOnReleaseMultipl * Time.fixedDeltaTime;
        //    }
        //    else if (_fastFallTime < _CharacterIdentity.TimeForUpwardsCancel)
        //    {
        //        VerticalVelocity = Mathf.Lerp(_fastFallReleaseSpeed, 0, (_fastFallTime / _CharacterIdentity.TimeForUpwardsCancel));
        //    }

        //    _fastFallTime += Time.fixedDeltaTime;
        //}

        // Gravité normale en chute
        if (!_PlayerController.IsGrounded && !_isJumping)
        {
            if (!_isFalling)
            {
                _isFalling = true;
            }

            VerticalVelocity += _CharacterIdentity.Gravity * Time.fixedDeltaTime;
        }

        // Limiter la vitesse de chute
        VerticalVelocity = Mathf.Clamp(VerticalVelocity, -_CharacterIdentity.MaxFallSpeed, 50f);

        _Rigidbody.linearVelocity = new Vector2(_Rigidbody.linearVelocity.x, VerticalVelocity);
    }

    #endregion

    #region Collision Checks

    private void GroundCheck()
    {
        Vector2 boxCastOrigin = new Vector2(_CharacterColliderForGroundChecking.bounds.center.x,
                                           _CharacterColliderForGroundChecking.bounds.min.y + 0.01f);

        Vector2 boxCastSize = new Vector2(_CharacterColliderForGroundChecking.bounds.size.x * 0.9f,
                                         _CharacterIdentity.GroundDetectionRayLenght);

        _groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down,
                                      _CharacterIdentity.GroundDetectionRayLenght, _CharacterIdentity.GroundLayer);

        //Color debugColor = _groundHit.collider != null ? Color.green : Color.red;
        //Debug.DrawRay(boxCastOrigin, Vector2.down * _CharacterIdentity.GroundDetectionRayLenght, debugColor, 0.1f);

        if (_groundHit.collider != null)
        {
            JumpAction?.Invoke(false);
            _PlayerController.IsGrounded = true;
        }
        else
        {
            JumpAction?.Invoke(true);
            _PlayerController.IsGrounded = false;
        }
    }

    private void BumpedHeadCheck()
    {
        Vector2 boxCastOrigin = new Vector2(_CharacterColliderForGroundChecking.bounds.center.x,
                                           _CharacterColliderForGroundChecking.bounds.max.y);
        Vector2 boxCastSize = new Vector2(_CharacterColliderForGroundChecking.bounds.size.x * _CharacterIdentity.HeadWidth,
                                         _CharacterIdentity.HeadDetectionRayLenght);

        _groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.up,
                                      _CharacterIdentity.HeadDetectionRayLenght, _CharacterIdentity.GroundLayer);

        if (_groundHit.collider != null && !_bumpHead)
        {
            _bumpHead = true;
        }
        else if (_groundHit.collider == null && _bumpHead)
        {
            _bumpHead = false;
        }
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

        if (!_PlayerController.IsGrounded)
        {
            _coyoteTimer -= Time.deltaTime;
        }
        else
        {
            _coyoteTimer = _CharacterIdentity.CoyoteTime;
        }
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