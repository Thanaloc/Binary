using System;
using UnityEngine;

public abstract class Ability : MonoBehaviour
{
    [SerializeField] private string _AbilityId;

    [SerializeField] private PlayerController _PlayerController;
    [SerializeField] private PlayerSO _PlayerIdentity;

    public int Charges;
    public float Cooldown;
    public bool CanBeCanceled = true;

    protected bool _isActive;

    public bool IsActive() => _isActive;
    public string AbilityId() => _AbilityId;
    public PlayerController PlayerController() => _PlayerController;
    public PlayerSO PlayerIdentity() => _PlayerIdentity;

    public Action OnAbilityActivated;
    public Action OnAbilityCanceled;


    public virtual void Update()
    {
        if (!CanBeCanceled && IsActive())
        {
            _PlayerController.CanJump = false;
        }
    }

    public virtual bool CanUse(float p_lastUsedTime, int p_chargesUsed)
    {
        return (Time.time >= p_lastUsedTime + Cooldown) && p_chargesUsed <= Charges;
    }

    public virtual void AddCharges()
    {
        Charges++;
    }

    public virtual void RemoveCharges()
    {
        Charges--;
    }

    public virtual void Activate()
    {
        OnAbilityActivated?.Invoke();
        _isActive = true;
    }

    public virtual void Cancel()
    {
        OnAbilityCanceled?.Invoke();

        if (!CanBeCanceled)
            _PlayerController.CanJump = true;

        _isActive = false;
    }
}
