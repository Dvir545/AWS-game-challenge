using System;
using System.Collections;
using JetBrains.Annotations;
using Player;
using UnityEngine;
using UnityEngine.AI;
using Utils;
using Utils.Data;
using Random = UnityEngine.Random;

namespace Enemies
{
    public class EnemyMovementManager : MonoBehaviour
    {
        private GameObject[] _targets;
        [SerializeField] private float secondsToUpdateTarget = 5f;
        private PlayerData playerData;
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
        public float SpeedMultiplier { get; private set; } = 1;
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
            SpeedMultiplier = Random.Range(0.8f, 1.2f);
            AgentSetSpeed = AgentOriginalSpeed * EnemyData.GetSpeed(_enemyType) * SpeedMultiplier;
            Agent.speed = AgentSetSpeed;
            Agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
            
            _targets = GameObject.FindGameObjectsWithTag("Player");
            playerData = FindObjectOfType<PlayerData>();
        
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

        protected Transform GetClosestTarget()
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

            return closestTarget;
        }
    
        protected virtual void FindClosestTarget()
        {
            CurrentTarget = GetClosestTarget();
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
           Rb.AddForce(hitDirection * (Constants.BaseKnockbackForce * playerData.KnockbackMultiplier * EnemyData.GetKnockbackForceMultiplier(_enemyType)), ForceMode2D.Impulse);
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

        public virtual void Reset()
        {
            Agent.isStopped = false;
            Agent.SetDestination(CurrentTargetPosition);
            Agent.speed = AgentSetSpeed;
            Agent.updatePosition = true;
            Agent.Warp(transform.position);
            IsMoving = true;
        }

        public Vector3 GetCurrentTargetPosition()
        {
            return CurrentTargetPosition;
        }
    }
}
