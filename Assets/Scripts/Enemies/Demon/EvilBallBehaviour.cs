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
        private float _angle;
        private float _newAngle;
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
            // Calculate starting angle based on index
            float angleOffset = MathUtils.Mod((360f / _totalBalls) * _ballIndex + 180, 360);
            _angle = angleOffset * Mathf.Deg2Rad; // Convert to radians
            _newAngle = _angle;
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
            } else if (!_agent.updatePosition)
            {
                _agent.updatePosition = true;
            }
            if (Targeted)
            {
                _agent.SetDestination(CurrentTarget.position);
            }
        }

        private void CircleAround()
        {
            _angle += _rotationSpeed * Time.deltaTime;
            _newAngle += _rotationSpeed * Time.deltaTime;
            if (Math.Abs(_angle - _newAngle) > 0.01f)
                _angle = Mathf.Lerp(_angle, _newAngle, Time.deltaTime/20f);
        
            float x = _horizontalRadius * Mathf.Cos(_angle);
            float y = _verticalRadius * Mathf.Sin(_angle);
        
            Vector3 offset = new Vector3(x, y, 0);
            var transform1 = transform;
            transform1.position = _pivot.position + offset;
            _agent.Warp(transform1.position);
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

        public void UpdateBallPosition(int i, int ballsCount)
        {
            _ballIndex = i;
            _totalBalls = ballsCount;
            float angleOffset = MathUtils.Mod((360f / _totalBalls) * _ballIndex + 180, 360);
            _newAngle = angleOffset * Mathf.Deg2Rad;
            // force going clockwise
            if (_angle > _newAngle)
                _angle -= 2 * Mathf.PI;
        }
        
        public void Release()
        {
            _sent = false;
            BallPool.Instance.ReleaseBall(gameObject);
            _agent.updatePosition = false;
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
