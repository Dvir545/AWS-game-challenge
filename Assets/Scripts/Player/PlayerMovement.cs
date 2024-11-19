using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using Utils;

namespace Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private float movementSpeed = 5f;
        [SerializeField] private Animator playerAnimator;
        [SerializeField] private Animator toolAnimator;
        private Rigidbody2D _rb;
        [SerializeField] private SpriteRenderer bodySpriteRenderer;
        [SerializeField] private SpriteRenderer toolSpriteRenderer;
        private static readonly int AnimationFacing = Animator.StringToHash("facing");
        private static readonly int AnimationMoving = Animator.StringToHash("moving");

        private Vector2 _movementDirection;
        private bool _isMoving;
        private CharacterFacingDirection _facing;

        void Start()
        {
            _rb = GetComponent<Rigidbody2D>();
            ChangeDirection(CharacterFacingDirection.Down);
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
                playerAnimator.SetBool(AnimationMoving, true);
                SetFacingDirection();
            }
            else  // if no keyboard input, check for mouse input
            {
                // Check if the mouse is over a UI element
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        playerAnimator.SetBool(AnimationMoving, true);
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
                        playerAnimator.SetBool(AnimationMoving, false);
                        _movementDirection = Vector2.zero;
                    }
                }
                else
                {
                    // Mouse is over a UI element, so we ignore mouse input
                    playerAnimator.SetBool(AnimationMoving, false);
                    _movementDirection = Vector2.zero;
                }
            }
        }

        
        private void SetFacingDirection()
        {
            CharacterFacingDirection facingDirection = _movementDirection.GetFacingDirection();
            if (facingDirection == CharacterFacingDirection.Right)
            {
                bodySpriteRenderer.flipX = true;
                toolSpriteRenderer.flipX = true;
            } else if (facingDirection == CharacterFacingDirection.Left)
            {
                bodySpriteRenderer.flipX = false;
                toolSpriteRenderer.flipX = false;
            }
            ChangeDirection(facingDirection);
        }

        private void Move()
        {
            _rb.velocity = _movementDirection * movementSpeed;
        }

        private void ChangeDirection(CharacterFacingDirection direction)
        {
            _facing = direction;
            playerAnimator.SetInteger(AnimationFacing, (int)direction);
            toolAnimator.SetInteger(AnimationFacing, (int)direction);
        }
        
        public CharacterFacingDirection GetFacingDirection()
        {
            return _facing;
        }
    }
}
