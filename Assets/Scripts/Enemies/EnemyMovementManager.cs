using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Utils;

namespace Enemies
{
    public class EnemyMovementManager : MonoBehaviour
    {
        private GameObject[] _targets;
        [SerializeField] private float secondsToUpdateTarget = 5f;
        protected Rigidbody2D Rb;
        protected NavMeshAgent Agent;
        protected Transform CurrentTarget;
        private Coroutine _updatePathCR;
        private Coroutine _kbCR;
        public bool IsMoving { get; protected set; } = true;

        // Start is called before the first frame update
        void Awake()
        {
            Rb = GetComponent<Rigidbody2D>();
            Agent = GetComponent<NavMeshAgent>();
            Agent.updateRotation = false;
            Agent.updateUpAxis = false;

            _targets = GameObject.FindGameObjectsWithTag("Player");
        
            _updatePathCR = StartCoroutine(UpdatePath());
        }

        private IEnumerator UpdatePath()
        {
            while (true)
            {
                FindClosestTarget();
                yield return new WaitForSeconds(secondsToUpdateTarget);
            }
        }

        protected void StopUpdatingPath()
        {
            StopCoroutine(_updatePathCR);
        }

        void Update()
        {
            Agent.SetDestination(CurrentTarget.position);
        }
    
        protected void FindClosestTarget()
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
    
        public CharacterFacingDirection GetFacingDirection()
        {
            Vector2 direction = CurrentTarget.position - transform.position;
            return direction.GetFacingDirection();
        }

        public virtual void Knockback(Vector2 hitDirection, float hitTime)
        {
            if (_kbCR != null)
            {
                StopCoroutine(_kbCR);
                Rb.velocity = Vector2.zero;
            }
            _kbCR = StartCoroutine(KnockbackCoroutine(hitDirection, hitTime));
        }

        protected virtual IEnumerator KnockbackCoroutine(Vector2 hitDirection, float hitTime, bool dead = false)
        { 
           Agent.updatePosition = false;
           IsMoving = false;
           Rb.AddForce(hitDirection * Constants.KnockbackForce, ForceMode2D.Impulse);
           yield return new WaitForSeconds(hitTime);
           if (!dead)
           {
               Agent.updatePosition = true;
               IsMoving = true;
           }
           Rb.velocity = Vector2.zero;
        }

        public void Die(Vector2 hitDirection)
        {
            Agent.isStopped = true;
            StartCoroutine(KnockbackCoroutine(hitDirection,  .25f, true));
        }
    }
}
