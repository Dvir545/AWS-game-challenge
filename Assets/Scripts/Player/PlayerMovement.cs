using System.Diagnostics;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;
using Utils;
using Debug = UnityEngine.Debug;

namespace Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float movementSpeed = 5f;
        private Animator _animator;
        private Rigidbody2D _rb;
        public int facingDirection;
        private static readonly int Beans = Animator.StringToHash("beans");

        private Vector2 _movementDirection;
        private bool _isMoving;
    
        void Start()
        {
            _rb = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
            facingDirection = (int)PlayerFacingDirection.Down;
        }

        void Update()
        {
            CheckInput();
        }

        private void CheckInput()
        {
            if (Input.GetMouseButtonDown(0)) // Left mouse button pressed
            {
                _isMoving = true;
                // get player position relative to parent
                Vector2 playerPos = transform.localPosition;
                // get mouse position, relative to middle of screen
                Vector2 mousePos = Input.mousePosition - new Vector3(Screen.width / 2, Screen.height / 2, 0);
                // get direction normalized vector from player to mouse
                _movementDirection = (mousePos - playerPos).normalized;
                SetFacingDirection();
                Debug.Log(_movementDirection);
            } else
            {
                _movementDirection = Vector2.zero;
            }
        }
        
        private void SetFacingDirection()
        {
            if (_movementDirection.x > 0)
            {
                facingDirection = (int)PlayerFacingDirection.Right;
            }
            else if (_movementDirection.x < 0)
            {
                facingDirection = (int)PlayerFacingDirection.Left;
            }
            else if (_movementDirection.y > 0)
            {
                facingDirection = (int)PlayerFacingDirection.Up;
            }
            else if (_movementDirection.y < 0)
            {
                facingDirection = (int)PlayerFacingDirection.Down;
            }
            // animator.SetInteger("FacingDirection", facingDirection);
        }

        private void Move()
        {
            _rb.linearVelocity = _movementDirection * movementSpeed;
        }
    }
}
