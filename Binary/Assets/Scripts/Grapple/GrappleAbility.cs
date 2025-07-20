using UnityEngine;

public class GrappleAbility : Ability
{
    private Transform _objectToGrapple = null;
    private float _lastUsedTime = 0f;
    private bool _isGrappling = false;

    private void Awake()
    {
        CanBeCanceled = false;
    }

    public void SetObjectToGrapple(Transform p_objectToGrapple)
    {
        _objectToGrapple = p_objectToGrapple;
    }


    public override void Update()
    {
        base.Update();
        GrappleCheck();
    }
    private void FixedUpdate()
    {
        Grapple();
    }

    private void GrappleCheck()
    {
        if (InputManager.GrappleWasPressed && PlayerController().CanGrapple && _objectToGrapple != null && CanUse(_lastUsedTime, 1))
        {
            _lastUsedTime = Time.time;
            RemoveCharges();
            Activate();
        }

        if (InputManager.GrappleWasReleased) //Maybe to change condition, like timerelated
        {
            Cancel();
        }
    }
    public override void Activate()
    {
        base.Activate();
        _isGrappling = true;
    }

    public override void Cancel()
    {
        base.Cancel();
        _isGrappling = false;
        _lastUsedTime = 0f;
    }

    private void Grapple()
    {
        if (_isGrappling)
        {
            Debug.Log("GRAPPLING");
        }
    }





}
