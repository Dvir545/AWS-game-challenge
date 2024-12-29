using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;
using Utils.Data;

namespace Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private float baseMovementSpeed = 5f;
        private Rigidbody2D _rb;
        [SerializeField] private SpriteRenderer bodySpriteRenderer;
        [SerializeField] private SpriteRenderer toolSpriteRenderer;
        [SerializeField] private PlayerData playerData;
        [SerializeField] private Joystick movementJoystick;

        [SerializeField] private InputActionReference moveAction;

        private bool _canMove = true;
        private Vector2 _movementDirection;
        public bool IsMoving => _movementDirection.magnitude > 0;
        private FacingDirection _facing;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            ChangeDirection(FacingDirection.Down);
            EventManager.Instance.StartListening(EventManager.PlayerGotHit, GotHit);
            EventManager.Instance.StartListening(EventManager.PlayerDied, Die);
        }

        private void Die(object arg0)
        {
            _canMove = false;
            _rb.velocity = Vector2.zero;
            _movementDirection = Vector2.zero;
        }

        void Update()
        {
            if (_canMove)
            {
                CheckInput();
                Move();
            }
        }

        private void CheckInput()
        {
            _movementDirection = moveAction.action.ReadValue<Vector2>();
            if (_movementDirection.magnitude <= 0 && movementJoystick.Direction != Vector2.zero)
            {
                _movementDirection = movementJoystick.Direction.normalized;
            } 
            if (_movementDirection.magnitude > 0)
                SetFacingDirection();
        }
        
        private void SetFacingDirection()
        {
            FacingDirection facingDirection = _movementDirection.GetFacingDirection();
            ChangeDirection(facingDirection);
        }

        public float GetMovementSpeed()
        {
            return baseMovementSpeed * playerData.SpeedMultiplier;
        }

        private void Move()
        {
            _rb.velocity = _movementDirection * GetMovementSpeed();
        }

        private void ChangeDirection(FacingDirection direction)
        {
            _facing = direction;
        }

        public FacingDirection GetFacingDirection()
        {
            return _facing;
        }
        
        private void GotHit(object arg0)
        {
            if (arg0 is (float hitTime, Vector3 hitDirection, float pushForceMultiplier))
            {
                Knockback(hitTime, hitDirection, pushForceMultiplier);
            }
        }
        
        public void Knockback(float knockbackTime, Vector2 hitDirection, float pushForceMultiplier)
        {
            StartCoroutine(KnockbackCoroutine(hitDirection, knockbackTime, pushForceMultiplier));
        }

        private IEnumerator KnockbackCoroutine(Vector2 hitDirection, float knockbackTime, float pushForceMultiplier)
        {
            _canMove = false;
            _rb.velocity = Vector2.zero;
            _rb.AddForce(hitDirection * (Constants.BaseKnockbackForce * pushForceMultiplier), ForceMode2D.Impulse);
            yield return new WaitForSeconds(knockbackTime);
            _rb.velocity = Vector2.zero;
            _canMove = true;
        }

        public void Reset()
        {
            _canMove = true;
            _rb.velocity = Vector2.zero;
            _movementDirection = Vector2.zero;
            ChangeDirection(FacingDirection.Down);
        }
    }
}
