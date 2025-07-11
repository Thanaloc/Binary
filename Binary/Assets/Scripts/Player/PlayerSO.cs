using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "PlayerSO", menuName = "Scriptable Objects/PlayerSO")]
public class PlayerSO : ScriptableObject
{
    public string CharacterName;
    public LayerMask GroundLayer;

    [Header("Identity")]
    [Range(1f, 200f)] public float MaxHealth;
    public Utils.Identity Identity;
    public float HeadDetectionRayLenght = .2f;
    [Range(0, 1f)] public float HeadWidth = .75f;

    [Header("Speed Related")]
    public bool IsGrounded;
    public bool IsFacingRight;
    [Range(1f, 50)] public float MaxSpeed;
    [Range(1f, 50)] public float GroundedAccel;
    [Range(1f, 50)] public float GroundedDeccel;
    [Range(1f, 50)] public float AirAccel;
    [Range(1f, 50)] public float AirDeccel;

    [Header("Jump Related")]
    public float JumpHeight;
    [Range(0f, 1)] public float CoyoteTime;
    [Range(0f, 1)] public float BufferTime;
    [Range(1f, 20)] public float WallJumpTime;
    [Range(1f, 1.1f)] public float JumpHeightCompensFactor;
    [Range(.01f, 5f)] public float GravityOnReleaseMultipl;
    public float TimeTillJumpApex;
    public float MaxFallSpeed;
    [Range(1f, 5f)] public float NumberOfJumps;

    [Range(.02f, .3f)] public float TimeForUpwardsCancel;
    [Range(.5f, 1f)] public float ApexThreshold;
    [Range(.01f, 1f)] public float ApexHangTime;

    [Header("Physics Related")]
    [Range(0f, 1)] public float GroundDetectionRayLenght;

    public float Gravity { get; private set; }
    public float InitialJumpVelocity { get; private set; }
    public float AdjustedJumpHeight { get; private set; }


    private void OnValidate()
    {
        CalculateValues();
    }

    private void OnEnable()
    {
        CalculateValues();
    }

    private void CalculateValues()
    {
        AdjustedJumpHeight = JumpHeight * JumpHeightCompensFactor;
        Gravity = -(2f * AdjustedJumpHeight) / Mathf.Pow(TimeTillJumpApex, 2f);
        InitialJumpVelocity = Mathf.Abs(Gravity) * TimeTillJumpApex;
    }
}
