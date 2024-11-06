using UnityEngine;
using Utils;

namespace Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private float movementSpeed = 5f;
        private Animator _animator;
        private Rigidbody2D _rb;
        private SpriteRenderer _spriteRenderer;
        private static readonly int AnimationFacing = Animator.StringToHash("facing");
        private static readonly int AnimationMoving = Animator.StringToHash("moving");

        private Vector2 _movementDirection;
        private bool _isMoving;
    
        void Start()
        {
            _rb = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _animator = GetComponent<Animator>();
            _animator.SetInteger(AnimationFacing, (int)PlayerFacingDirection.Down);
        }

        void Update()
        {
            CheckInput();
            Move();
        }

        private void CheckInput()
        {
            // first try and check for keyboard input
            _movementDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
            if (_movementDirection.magnitude > 0)
            {
                _animator.SetBool(AnimationMoving, true);
                SetFacingDirection();
            }
            else  // if no keyboard input, check for mouse input
            {
                if (Input.GetMouseButtonDown(0))
                {
                    _animator.SetBool(AnimationMoving, true);
                }
                else if (Input.GetMouseButton(0))
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
                    _animator.SetBool(AnimationMoving, false);
                    _movementDirection = Vector2.zero;
                }
            }
        }
        
        private void SetFacingDirection()
        {
            // set facing direction based on direction angle
            float angle = Vector2.SignedAngle(Vector2.up, _movementDirection);
            if (angle is > -45 and < 45)
            {
                _animator.SetInteger(AnimationFacing, (int)PlayerFacingDirection.Up);
            }
            else if (angle is >= 45 and <= 135)
            {
                _animator.SetInteger(AnimationFacing, (int)PlayerFacingDirection.Right);
                _spriteRenderer.flipX = true;
            }
            else if (angle is > 135 or < -135)
            {
                _animator.SetInteger(AnimationFacing, (int)PlayerFacingDirection.Down);
            }
            else if (angle is >= -135 and <= -45)
            {
                _animator.SetInteger(AnimationFacing, (int)PlayerFacingDirection.Left);
                _spriteRenderer.flipX = false;
            }
        }

        private void Move()
        {
            _rb.linearVelocity = _movementDirection * movementSpeed;
        }
    }
}
