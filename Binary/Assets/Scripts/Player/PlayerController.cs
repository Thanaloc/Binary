using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private List<Ability> _Abilities;

    public bool IsGrounded;
    public bool CanGrapple;
    public bool IsFacingRight;
    public bool CanJump = true;

    public Ability GetAbilityController(string p_abilityId)
    {
        return _Abilities.Find(ability => ability.AbilityId().Equals(p_abilityId));
    }

}
