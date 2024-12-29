using UnityEngine;
using UnityEngine.AI;
using Utils;
using Utils.Data;

namespace Player
{
    public class PetBehaviour: MonoBehaviour
    {
        private  static readonly int AnimationWalking = Animator.StringToHash("walking");
        private  static readonly int AnimationFacing = Animator.StringToHash("facing");
        private static readonly int AnimationWalkSpeed = Animator.StringToHash("walkspeed");
        
        private Animator animator;
        private SpriteRenderer _spriteRenderer;
        private NavMeshAgent _agent;
        private Transform _target;
        private PlayerMovement _playerMovement;
        
        private void Awake()
        {
            animator = transform.GetChild(0).GetComponent<Animator>();
            _spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
            _agent = GetComponent<NavMeshAgent>();
            _agent.updateRotation = false;
            _agent.updateUpAxis = false;
            _agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        }

        public void Init(Pet type, int index, PlayerMovement playerMovement, float animationSpeed, Transform target)
        {
            if (animator == null)
                Awake();
            animator.SetFloat(AnimationWalkSpeed, animationSpeed);
            _playerMovement = playerMovement;
            UpdateSpeed();
            _target = target;
            var position = _target.position;
            transform.position = position;
            _agent.Warp(position);
            _spriteRenderer.color = PetsData.GetColor(type, index);
        }

        public void UpdateSpeed()
        {
            _agent.speed = _playerMovement.GetMovementSpeed();
        }

        private void SetFacingDirection(Vector2 direction)
        { 
            animator.SetInteger(AnimationFacing, direction.x > 0 ? 1 : -1);
            if (direction.x > 0)
            {
                transform.localScale = new Vector3(1, 1, 1);
            } else if (direction.x < 0)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
        }
        
        private void Update()
        {
            if (_target == null)
            {
                return;
            }
            if (_agent.speed == 0)
                UpdateSpeed();

            var position = _target.position;
            Vector2 direction = position - transform.position;
            animator.SetBool(AnimationWalking, direction.magnitude > _agent.stoppingDistance);
            SetFacingDirection(direction);
            _agent.SetDestination(position);
        }
    }
}