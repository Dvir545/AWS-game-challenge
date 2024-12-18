﻿using System.Collections;
using Crops;
using UnityEngine;
using UnityEngine.AI;
using Utils;

namespace Enemies.Chicken
{
    public class ChickenMovementManager: EnemyMovementManager
    {
        private FarmingManager _farmingManager;
        private EnemyAnimationManager _enemyAnimationManager;
        private bool _foundCrop;
        [SerializeField] private float roamRadius = 5f;
        [SerializeField] private float roamSecondsToSwitchTarget = 3f;
        [SerializeField] private float roamingSpeed = 1f;
        private Coroutine _roamCR;

        protected override void Awake()
        {
            _farmingManager = FindObjectOfType<FarmingManager>();
            _enemyAnimationManager = GetComponent<EnemyAnimationManager>();
            base.Awake();
        }

        protected override void Update()
        {
            if (EnemyHealthManager.IsDead)
            {
                if (_roamCR != null)
                {
                    StopCoroutine(_roamCR);//RoamingAgent.Instance.StopRoam(Agent);
                    _roamCR = null;
                }
                return;
            }
            if (Targeted)
            {
                if (_roamCR != null)
                {
                    StopCoroutine(_roamCR);//RoamingAgent.Instance.StopRoam(Agent);
                    _roamCR = null;
                    Agent.speed = AgentSetSpeed;
                }
                CurrentTargetPosition = CurrentTarget.position;
                // if (_foundCrop)
                //     CurrentTargetPosition += new Vector3(0.5f, 0.5f, 0);
                Agent.SetDestination(CurrentTargetPosition);
            }
            else {
                if (_roamCR == null)
                {
                    FindClosestTarget();
                    if (CurrentTarget == null) {
                        Agent.speed = AgentOriginalSpeed * roamingSpeed * Random.Range(0.8f, 1.2f);
                        _roamCR = StartCoroutine(RoamTowardsPlayerCoroutine());//RoamingAgent.Instance.Roam(Agent, roamRadius, roamSecondsToSwitchTarget);
                    }
                }
                _enemyAnimationManager.SetFacingDirection();
            }

            IsMoving = Agent.velocity.magnitude != 0 && !EnemyHealthManager.IsDead;
        }

        protected override void FindClosestTarget()
        {
            Vector2Int? closestCrop = null;
            float closestDistance = float.MaxValue;

            foreach (Vector2Int crop in _farmingManager.Farms.Keys)
            {
                Vector3 cropCenter = _farmingManager.Farms[crop].transform.position 
                                     + new Vector3(0.5f, 0.5f, 0);
                float distance = Vector3.Distance(transform.position, cropCenter);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestCrop = crop;
                    _foundCrop = true;
                }
            }

            if (closestCrop != null)
            {
                Agent.isStopped = false;
                CurrentTarget = _farmingManager.Farms[closestCrop.Value].transform;
            }
        }

        public override void Die(Vector2? hitDirection = null)
        {
            Agent.isStopped = true;
            Rb.velocity = Vector2.zero;
        }
        
        public override FacingDirection GetFacingDirection()
        {
            Vector2 direction = Agent.destination - transform.position;
            if (direction.x > 0)
                return FacingDirection.Left;
            if (direction.x < 0)
                return FacingDirection.Right;
            return CurDirection;
        }
        
        private IEnumerator RoamTowardsPlayerCoroutine()
        {
            var target = GetClosestTarget();
            while (true)
            {
                var position = Agent.transform.position;
                var direction = (target.position - position).normalized;
                Vector3 newPos = position + direction * (roamRadius + Random.Range(-1, 2)); //RandomNavSphere(agent.transform.position, radius, -1);
                Agent.SetDestination(newPos);
                yield return new WaitForSeconds(roamSecondsToSwitchTarget + Random.Range(-1, 2));
            }
        }
    }
}