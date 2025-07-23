using System.Collections;
using UnityEngine;

public class GrappleAbility : Ability
{
    [Header("Configuration du Grappin")]
    [SerializeField] private float _GrappleRange = 8f;
    [SerializeField] private float _SwingForce = 12f;
    [SerializeField] private float _PullForce = 20f;
    [SerializeField] private float _MaxSwingSpeed = 15f;
    [SerializeField] private VerletRop _VerletRope;

    [SerializeField] private Rigidbody2D _PlayerRigidbody;

    private Transform _objectToGrapple = null;
    private float _lastUsedTime = 0f;
    private float _timeElapsed = 0f;
    private bool _isGrappling = false;

    private const float GRAPPLE_DURATION = 3f;
    public float GRAPPLE_DISTANCE = 5f;

    private Vector2 _moveVelocity = Vector2.zero;

    private void Awake()
    {
        CanBeCanceled = false;
    }

    public void SetObjectToGrapple(Transform p_objectToGrapple)
    {
        _objectToGrapple = p_objectToGrapple;

        if (_objectToGrapple == null && _isGrappling)
            Cancel();
    }


    public override void Update()
    {
        base.Update();
        GrappleCheck();

        if (_isGrappling)
        {
            UpdateGrappling();
        }
    }

    private void FixedUpdate()
    {
        if (_isGrappling)
        {
            //TODO apply physics ?
        }
    }

    private void GrappleCheck()
    {
        if (InputManager.GrappleWasPressed && PlayerController().CanGrapple && _objectToGrapple != null && CanUse(_lastUsedTime, 1) && !_isGrappling)
        {
            _lastUsedTime = Time.time;
            RemoveCharges();
            Activate();
        }

        //if (InputManager.GrappleWasReleased && _isGrappling) //Maybe to change condition, like timerelated
        //{
        //    Cancel();
        //}
    }
    public override void Activate()
    {
        base.Activate();

        PlayerController().IsUsingSpecialMovementAbility = true;

        _isGrappling = true;
        _timeElapsed = 0;

        // Créer la corde Verlet
        _VerletRope.CreateRope(transform.position, _objectToGrapple.position);

        // Modifier les propriétés du joueur pendant le grappin
        PlayerController().CanJump = false;
    }

    public override void Cancel()
    {
        base.Cancel();

        PlayerController().IsUsingSpecialMovementAbility = false;

        _VerletRope.DestroyRope();

        _isGrappling = false;
        _lastUsedTime = 0f;
        _timeElapsed = 0;
    }

    private void UpdateGrappling()
    {
        if (!_VerletRope.IsActive())
        {
            Cancel();
            return;
        }

        // Mettre à jour la position du joueur dans la corde
        _VerletRope.UpdatePlayerPosition(PlayerController().gameObject.transform.position); //to change to _charactercontroller.position je pense
    }


    private void InitiateGrappling()
    {
        Vector2 startPos = transform.localPosition;
        Vector2 endPos = new Vector2(transform.localPosition.x + GRAPPLE_DISTANCE, transform.localPosition.y);

        Debug.DrawLine(startPos, _objectToGrapple.transform.position, Color.red, GRAPPLE_DURATION);


        Cancel();
    }





}
