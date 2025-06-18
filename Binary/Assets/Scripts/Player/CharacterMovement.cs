using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CharacterMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _Rigidbody;
    [SerializeField] private TerrainController _TerrainCollider;
    [SerializeField] private PlayerSO _CharacterIdentity;
    [SerializeField] private InputActionReference _MoveInputs;

    [SerializeField] private BoxCollider2D _DeathCollider;
    [SerializeField] private BoxCollider2D _VictoryCollider;

    private Vector2 _moveDirection;

    void Update()
    {
        _moveDirection = _MoveInputs.action.ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        _Rigidbody.linearVelocity = new Vector2(_moveDirection.x * _CharacterIdentity.MaxSpeed, _Rigidbody.linearVelocity.y);
        Jump();
    }

    void OnJump()
    {
        if (_CharacterIdentity.IsGrounded)
        {
            _CharacterIdentity.CanJump = true;
        }
    }

    private void Jump()
    {
        if (_CharacterIdentity.CanJump)
        {
            _Rigidbody.linearVelocity = new Vector2(_Rigidbody.linearVelocity.x, _CharacterIdentity.JumpHeight);
        }
    }

    private void OnTriggerEnter2D(Collider2D p_collision)
    {
        foreach (BoxCollider2D coll in _TerrainCollider.TerrainColliders)
        {
            if (p_collision.Equals(coll))
            {
                _CharacterIdentity.IsGrounded = true;
            }
        }

        if (p_collision.Equals(_DeathCollider))
            SceneManager.LoadScene("TestFeaturesScene");

        if (p_collision.Equals(_VictoryCollider))
        {
            Debug.Log("Victory !");
            StartCoroutine(VictoryCountdownTEMP());
        }
    }

    private void OnTriggerExit2D(Collider2D p_collision)
    {
        foreach (BoxCollider2D coll in _TerrainCollider.TerrainColliders)
        {
            if (p_collision.Equals(coll))
            {
                _CharacterIdentity.IsGrounded = false;
                _CharacterIdentity.CanJump = false;
            }
        }
    }

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
}
