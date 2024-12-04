using UnityEngine;

namespace Enemies.Goblin
{
    public class GoblinMovementManager : EnemyMovementManager
    {
        [SerializeField] private float defaultDistanceToAttack = 3f;
        [SerializeField] private float maxDistanceToAttack = 8f;
        
        private GoblinAttackManager _attackManager;
        
        protected override void Awake()
        {
            _attackManager = GetComponent<GoblinAttackManager>();
            base.Awake();
        }
        
        protected override void Update()
        {
            base.Update();
            if (Targeted)
            {
                float distance = Vector2.Distance(transform.position, CurrentTargetPosition);
                if (distance <= defaultDistanceToAttack && IsMoving)
                {
                    Agent.updatePosition = false;
                    IsMoving = false;
                    _attackManager.StartAttacking(CurrentTarget);
                }
                else if (distance > maxDistanceToAttack && !IsMoving)
                {
                    Agent.Warp(transform.position);
                    Agent.updatePosition = true;
                    IsMoving = true;
                    _attackManager.StopAttacking();
                }
            }
        }
        
        public override void Knockback(Vector2 hitDirection, float hitTime)
        {
            _attackManager.StopAttacking();
            base.Knockback(hitDirection, hitTime);
        }
    }
}
