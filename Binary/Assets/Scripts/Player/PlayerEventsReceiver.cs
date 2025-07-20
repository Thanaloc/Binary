using System;
using UnityEngine;

public class PlayerEventsReceiver : MonoBehaviour
{
    public static PlayerEventsReceiver Instance;

    [SerializeField] private PlayerController _Player;

    private string _grappleAbilityId = "Grapple";

    void Start()
    {
        if (Instance == null)
            Instance = this;
    }

    void OnDestroy()
    {
        Instance = null;
    }

    #region Grapple
    public void GrapplePointEvent(bool p_enter, Transform p_objToGrapple)
    {
        _Player.CanGrapple = p_enter;

        GrappleAbility grapple = _Player.GetAbilityController(_grappleAbilityId) as GrappleAbility;

        if (grapple != null)
            grapple.SetObjectToGrapple(p_objToGrapple);
    }
    #endregion
}
