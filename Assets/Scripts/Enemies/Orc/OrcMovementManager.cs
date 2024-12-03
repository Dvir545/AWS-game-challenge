﻿using System;
using Towers;
using UnityEngine;

namespace Enemies.Orc
{
    public class OrcMovementManager: EnemyMovementManager
    {
        private TowerBuildManager _towerBuildManager;
        private bool _foundTower;
        [SerializeField] private float attackDistance;
        private OrcAttackManager _orcAttackManager;
        private float accelerationDistance = 3f; // Distance threshold for acceleration
        private float accelerationMultiplier = 1.5f; // How much faster the agent moves when close
        
        protected override void Awake()
        {
            _towerBuildManager = FindObjectOfType<TowerBuildManager>();
            _orcAttackManager = GetComponent<OrcAttackManager>();
            base.Awake();
        }

        protected override void Update()
        {
            base.Update();
            if (CurrentTarget != null)
            {
                float distanceToTarget = Vector2.Distance(transform.position, CurrentTargetPosition);
                if (_foundTower)
                {
                    if (distanceToTarget <= attackDistance)
                    {
                        Agent.isStopped = true;
                        IsMoving = false;
                        if (!_orcAttackManager.IsAttacking())
                            _orcAttackManager.StartAttacking(CurrentTarget);
                    }
                    else
                    {
                        Agent.isStopped = false;
                        IsMoving = true;
                        if (Math.Abs(Agent.speed - AgentSetSpeed) > 0.01f)
                            Agent.speed = AgentSetSpeed;
                        _orcAttackManager.SetAttacking(false);
                    }
                }
                else
                {
                    Agent.isStopped = false;
                    IsMoving = true;
                    Agent.speed = distanceToTarget <= accelerationDistance ? AgentSetSpeed * accelerationMultiplier : AgentSetSpeed;
                }
            }
        }

        protected override void FindClosestTarget()
        {
            Transform closestTower = null;
            float closestDistance = float.MaxValue;
            var foundTower = false;
            foreach (Transform towerTransform in _towerBuildManager.GetBuiltTowerTransforms())
            {
                float distance = Vector2.Distance(transform.position, towerTransform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTower = towerTransform;
                    foundTower = true;
                }
            }
            if (closestTower != null)
            {
                CurrentTarget = closestTower;
            }
            else if (!foundTower)
                base.FindClosestTarget();
            _foundTower = foundTower;
        }
    }
}