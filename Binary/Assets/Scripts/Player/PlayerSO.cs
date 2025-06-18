using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSO", menuName = "Scriptable Objects/PlayerSO")]
public class PlayerSO : ScriptableObject
{
    public string CharacterName;
    public float MaxHealth;
    public float MaxSpeed;
    public float JumpHeight;
    public float Mass;
    public bool IsGrounded;
    public bool CanJump;
    public Utils.Identity Identity;
}
