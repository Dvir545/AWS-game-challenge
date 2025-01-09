using System;
using Player;
using UnityEngine;
using UnityEngine.AI;
using Utils;

namespace Enemies.Demon
{
    public class EvilBallBehaviour : MonoBehaviour
    {
        private Transform _pivot;
        private float _rotationSpeed;
        private float _horizontalRadius;
        private float _verticalRadius;
        private int _ballIndex;
        private int _totalBalls;
        public float angle;
        public float newAngle;
        private bool _sent;
        
        private AudioSource _audioSource;
        [SerializeField] private AudioClip sendSound;
        [SerializeField] private AudioClip hitSound;
    
        private NavMeshAgent _agent;
        protected Transform CurrentTarget;
        private GameObject[] _targets;
        public bool Targeted => _agent.updatePosition && CurrentTarget != null;
        
        private float _currentAngle;  // Add this field to track current angle
        private float _targetAngle;   // Add this field for the target angle


    
        public void Init(float rotationSpeed, float horizontalRadius, float verticalRadius, int ballIndex, int totalBalls, Transform pivot, AudioSource audioSource)
        {
            _rotationSpeed = rotationSpeed;
            _horizontalRadius = horizontalRadius;
            _verticalRadius = verticalRadius;
            _ballIndex = ballIndex;
            _totalBalls = totalBalls;
            _pivot = pivot;
            transform.position = _pivot.position;
            _audioSource = audioSource;
            _agent.updatePosition = false;
            _agent.enabled = false;
            // Calculate starting angle based on index
            float angleOffset = MathUtils.Mod((360f / _totalBalls) * _ballIndex + 180, 360);
            angle = angleOffset * Mathf.Deg2Rad; // Convert to radians
            newAngle = angle;
            _sent = false;
        }

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _agent.updateRotation = false;
            _agent.updateUpAxis = false;
            _agent.updatePosition = false;
            _targets = GameObject.FindGameObjectsWithTag("Player");
        }
        
        private void Update()
        {
            if (!_sent)
            {
                CircleAround();
            } else if (!_agent.enabled)
            {
                _agent.enabled = true;
                _agent.updatePosition = true;
            }
            if (Targeted)
            {
                _agent.SetDestination(CurrentTarget.position);
            }
        }

        private void CircleAround()
        {
            angle = MathUtils.Mod(angle + _rotationSpeed * Time.deltaTime, 2*Mathf.PI);
            newAngle = MathUtils.Mod(newAngle + _rotationSpeed * Time.deltaTime, 2*Mathf.PI);
            if (Math.Abs(angle - newAngle) > 0.01f)
                angle = MathUtils.Mod(angle + Time.deltaTime, 2 * Mathf.PI);
        
            float x = _horizontalRadius * Mathf.Cos(angle);
            float y = _verticalRadius * Mathf.Sin(angle);
        
            Vector3 offset = new Vector3(x, y, 0);
            var transform1 = transform;
            transform1.position = _pivot.position + offset;
        }
    
        private void FindClosestTarget()
        {
            Transform closestTarget = null;
            float closestDistance = Mathf.Infinity;
            foreach (GameObject target in _targets)
            {
                float distance = Vector3.Distance(transform.position, target.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = target.transform;
                }
            }
            CurrentTarget = closestTarget;
        }

        public void Send()
        {
            _audioSource.PlayOneShot(sendSound);
            _sent = true;
            _agent.Warp(transform.position);
            _agent.updatePosition = true;
            FindClosestTarget();
        }
        
        public void Release()
        {
            _sent = false;
            BallPool.Instance.ReleaseBall(gameObject);
            _agent.updatePosition = false;
            _agent.enabled = false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                other.GetComponent<PlayerHealthManager>().GotHit(Enemy.EvilBall, other.transform.position - transform.position);
            }
            if (other.CompareTag("Player") || other.CompareTag("Sword"))
            {
                _audioSource.PlayOneShot(hitSound);
                Release();
            }
        }
    }
}
