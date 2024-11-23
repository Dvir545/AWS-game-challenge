using UnityEngine;
using UnityEngine.AI;
using World;

namespace Enemies.Chicken
{
    public class ChickenMovementManager: EnemyMovementManager
    {
        private FarmingManager _farmingManager;
        private ChickenEatingManager _chickenEatingManager;
        private EnemyHealthManager _chickenHealthManager;
        private bool _foundCrop;
        private Vector3 _lastCropPosition;
        
        protected override void Awake()
        {
            _farmingManager = FindObjectOfType<FarmingManager>();
            _chickenEatingManager = GetComponent<ChickenEatingManager>();
            _chickenHealthManager = GetComponent<EnemyHealthManager>();
            base.Awake();
            Agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        }

        protected override void Update()
        {
            if (Targeted)
            {
                CurrentTargetPosition = CurrentTarget.position;
                if (_foundCrop)
                    CurrentTargetPosition += new Vector3(0.5f, 0.5f, 0);
                Agent.SetDestination(CurrentTargetPosition);
            }
            else
            {
                Agent.SetDestination(_lastCropPosition);
            }

            IsMoving = Agent.velocity.magnitude != 0 && !_chickenHealthManager.IsDead;

            // if (IsMoving && Rb.velocity.magnitude == 0 && Agent.remainingDistance < 0.4f)
            // {
            //     IsMoving = false;
            //     Agent.updatePosition = false;
            // } else if (!IsMoving && Rb.velocity.magnitude == 0 && Agent.remainingDistance > 0.4f)
            // {
            //     IsMoving = true;
            //     Agent.updatePosition = true;
            // }
        }

        protected override void FindClosestTarget()
        {
            Vector3Int? closestCrop = null;
            float closestDistance = float.MaxValue;

            foreach (Vector3Int crop in _farmingManager.Farms.Keys)
            {
                Vector3 cropCenter = _farmingManager.Farms[crop].GetCropSpriteRenderer().transform.position 
                                     + new Vector3(0.5f, 0.5f, 0);
                float distance = Vector3.Distance(transform.position, cropCenter);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestCrop = crop;
                    _foundCrop = true;
                    _lastCropPosition = cropCenter;
                }
            }

            if (closestCrop != null)
            {
                Agent.isStopped = false;
                CurrentTarget = _farmingManager.Farms[closestCrop.Value].GetCropSpriteRenderer().transform;
            }
            else if (!_foundCrop)
                base.FindClosestTarget();
        }

        public override void Die(Vector2 hitDirection)
        {
            Agent.isStopped = true;
            Rb.velocity = Vector2.zero;
        }
    }
}