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
        private Collider2D _collider2D;
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

        protected virtual void Awake()
        {
            EnemyHealthManager = GetComponent<EnemyHealthManager>();
            _enemyType = EnemyHealthManager.enemyType;
            Rb = GetComponent<Rigidbody2D>();
            _collider2D = GetComponent<Collider2D>();
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


        protected IEnumerator UpdatePath()
        {
            while (true)
            {
                FindClosestTarget();
                yield return new WaitForSeconds(secondsToUpdateTarget);
            }
        }
        
        private void OnDisable()
        {
            if (UpdatePathCR != null)
            {
                StopCoroutine(UpdatePathCR);
                UpdatePathCR = null;
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
           _collider2D.enabled = true;
           Rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
           var force = Constants.BaseKnockbackForce *EnemyData.GetKnockbackForceMultiplier(_enemyType);
           if (force > 0)
               force *= playerData.KnockbackMultiplier;
           Rb.AddForce(hitDirection * force, ForceMode2D.Impulse);
           yield return new WaitForSeconds(hitTime);
           if (!dead)
           {
               Agent.Warp(transform.position);
               Agent.updatePosition = true;
               IsMoving = true;
           }
           if (!_collider2D.isTrigger) 
               _collider2D.enabled = false;
           Rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
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
            UpdatePathCR = StartCoroutine(UpdatePath());
            Agent.isStopped = false;
            if (CurrentTarget != null)
            {
                CurrentTargetPosition = CurrentTarget.position;
                Agent.SetDestination(CurrentTargetPosition);
            }
            Agent.speed = AgentSetSpeed;
            Agent.updatePosition = true;
            Agent.Warp(transform.position);
            IsMoving = true;
            if (!_collider2D.isTrigger)
                _collider2D.enabled = false;
        }

        public Vector3 GetCurrentTargetPosition()
        {
            return CurrentTargetPosition;
        }
    }
}
