using UnityEngine;
using UnityEngine.PlayerLoop;
using Utils;

namespace Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float movementSpeed = 5f;
        private Rigidbody2D _rb;
        public int facingDirection;

        private Vector2 _movementDirection;
        private bool _isMoving;
    
        void Start()
        {
            _rb = GetComponent<Rigidbody2D>();
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
                // get player position relative to parent
                Vector3 playerPos = transform.position;
                // get mouse position, relative to middle of screen
                Vector3 mousePos = Input.mousePosition - new Vector3(Screen.width / 2, Screen.height / 2, 0);
                // get direction normalized vector from player to mouse
                Vector3 direction = (mousePos - playerPos).normalized;
                Debug.Log(direction);
            }
        }
    }
}
