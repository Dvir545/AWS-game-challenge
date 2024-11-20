using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Utils;

namespace Enemies
{
    public class EnemyMovementManager : MonoBehaviour
    {
        [SerializeField] private Transform[] targets;
        [SerializeField] private float secondsToUpdateTarget = 5f;
        private Rigidbody2D _rb;
        private NavMeshAgent _agent;
        private Transform _currentTarget;
        public bool IsMoving => _agent.velocity.magnitude > 0f;

        // Start is called before the first frame update
        void Start()
        {
            _rb = GetComponent<Rigidbody2D>();
            _agent = GetComponent<NavMeshAgent>();
            _agent.updateRotation = false;
            _agent.updateUpAxis = false;
        
            StartCoroutine(UpdatePath());
        }

        private IEnumerator UpdatePath()
        {
            while (true)
            {
                FindClosestTarget();
                yield return new WaitForSeconds(secondsToUpdateTarget);
            }
        }

        void Update()
        {
            _agent.SetDestination(_currentTarget.position);
        }
    
        private void FindClosestTarget()
        {
            Transform closestTarget = null;
            float closestDistance = Mathf.Infinity;
            foreach (Transform target in targets)
            {
                float distance = Vector3.Distance(transform.position, target.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = target;
                }
            }
            _currentTarget = closestTarget;
        }
    
        public CharacterFacingDirection GetFacingDirection()
        {
            Vector2 direction = _currentTarget.position - transform.position;
            return direction.GetFacingDirection();
        }

        public void Knockback(Vector2 hitDirection, float hitTime)
        {
            StartCoroutine(KnockbackCoroutine(hitDirection, hitTime));
        }

        private IEnumerator KnockbackCoroutine(Vector2 hitDirection, float hitTime, bool dead = false)
        {
           _agent.updatePosition = false;
           _rb.AddForce(hitDirection * Constants.KnockbackForce, ForceMode2D.Impulse);
           yield return new WaitForSeconds(hitTime);
           _rb.velocity = Vector2.zero;
           if (!dead)
               _agent.updatePosition = true;
        }

        public void Die(Vector2 hitDirection)
        {
            _agent.isStopped = true;
            StartCoroutine(KnockbackCoroutine(hitDirection,  .25f, true));
        }
    }
}
