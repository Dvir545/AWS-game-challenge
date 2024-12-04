using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Utils;

namespace Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private float movementSpeed = 5f;
        private Rigidbody2D _rb;
        [SerializeField] private SpriteRenderer bodySpriteRenderer;
        [SerializeField] private SpriteRenderer toolSpriteRenderer;

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
            // first try and check for keyboard input
            _movementDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
            if (_movementDirection.magnitude > 0)
            {
                SetFacingDirection();
            }
            else  // if no keyboard input, check for mouse input
            {
                // Check if the mouse is over a UI element
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    if (Input.GetMouseButton(0))
                    {
                        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
                        // get player position relative to middle of screen
                        Vector2 playerPos = mainCamera.WorldToScreenPoint(transform.position) - screenCenter;
                        // get mouse position, relative to middle of screen
                        Vector2 mousePos = Input.mousePosition - screenCenter;
                        // get direction normalized vector from player to mouse
                        _movementDirection = (mousePos - playerPos).normalized;
                        SetFacingDirection();
                    }
                    else // no input
                    {
                        _movementDirection = Vector2.zero;
                    }
                }
                else
                {
                    // Mouse is over a UI element, so we ignore mouse input
                    _movementDirection = Vector2.zero;
                }
            }
        }
        
        private void SetFacingDirection()
        {
            FacingDirection facingDirection = _movementDirection.GetFacingDirection();
            ChangeDirection(facingDirection);
        }

        private void Move()
        {
            _rb.velocity = _movementDirection * movementSpeed;
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
            _rb.AddForce(hitDirection * (Constants.KnockbackForce * pushForceMultiplier), ForceMode2D.Impulse);
            yield return new WaitForSeconds(knockbackTime);
            _rb.velocity = Vector2.zero;
            _canMove = true;
        }
    }
}
