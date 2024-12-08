using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;
using Utils;
using Utils.Data;

namespace Enemies
{
    public class EnemyMovementManager : MonoBehaviour
    {
        private GameObject[] _targets;
        [SerializeField] private float secondsToUpdateTarget = 5f;
        protected Rigidbody2D Rb;
        protected NavMeshAgent Agent;
        protected Transform CurrentTarget;
        protected Vector3 CurrentTargetPosition = Vector3.zero;
        protected Coroutine UpdatePathCR;
        private Coroutine _kbCR;
        protected EnemyHealthManager EnemyHealthManager;
        private Enemy _enemyType;
        protected float AgentOriginalSpeed;
        protected float AgentSetSpeed;
        protected FacingDirection CurDirection;

        
        public bool IsMoving { get; protected set; } = true;
        public bool Targeted => !Agent.isStopped && CurrentTarget != null;

        // Start is called before the first frame update
        protected virtual void Awake()
        {
            EnemyHealthManager = GetComponent<EnemyHealthManager>();
            _enemyType = EnemyHealthManager.enemyType;
            Rb = GetComponent<Rigidbody2D>();
            Agent = GetComponent<NavMeshAgent>();
            Agent.updateRotation = false;
            Agent.updateUpAxis = false;
            AgentOriginalSpeed = Agent.speed;
            AgentSetSpeed = AgentOriginalSpeed * EnemyData.GetSpeed(_enemyType) * Random.Range(0.8f, 1.2f);
            Agent.speed = AgentSetSpeed;
            
            _targets = GameObject.FindGameObjectsWithTag("Player");
        
            UpdatePathCR = StartCoroutine(UpdatePath());
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
            StopCoroutine(UpdatePathCR);
        }

        protected virtual void Update()
        {
            if (EnemyHealthManager.IsDead)
            {
                if (UpdatePathCR != null)
                {
                    StopCoroutine(UpdatePathCR);
                    UpdatePathCR = null;
                }
                return;
            }
    
            if (Targeted)
            {
                CurrentTargetPosition = CurrentTarget.position;
                Agent.SetDestination(CurrentTargetPosition);
            }
        }
    
        protected virtual void FindClosestTarget()
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
    
        public virtual FacingDirection GetFacingDirection()
        {
            Vector2 direction = Agent.destination - transform.position;
            CurDirection = direction.GetFacingDirection();
            return CurDirection;
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
           Rb.AddForce(hitDirection * (Constants.KnockbackForce * EnemyData.GetKnockbackForceMultiplier(_enemyType)), ForceMode2D.Impulse);
           yield return new WaitForSeconds(hitTime);
           if (!dead)
           {
               Agent.Warp(transform.position);
               Agent.updatePosition = true;
               IsMoving = true;
           }
           Rb.velocity = Vector2.zero;
        }

        public virtual void Die(Vector2? hitDirection = null)
        {
            Agent.isStopped = true;
            if (hitDirection != null)
            {
                StartCoroutine(KnockbackCoroutine((Vector2)hitDirection, .25f, true));
            }
        }

        public Vector3 GetCurrentTargetPosition()
        {
            return CurrentTargetPosition;
        }
    }
}
