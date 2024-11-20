using UnityEngine;
using Utils;

namespace Player
{
    public class PlayerAttackManager: MonoBehaviour
    {
        [SerializeField] private PlayerMovement playerMovement;
        private CharacterFacingDirection _facing;
        
        [SerializeField] private Collider2D upAttackCollider;
        [SerializeField] private Collider2D downAttackCollider;
        [SerializeField] private Collider2D rightAttackCollider;
        public bool IsAttacking { get; private set; }

        private void Start()
        {
            upAttackCollider.enabled = false;
            downAttackCollider.enabled = false;
            rightAttackCollider.enabled = false;
        }

        private void Update()
        {
            if (IsAttacking && _facing != playerMovement.GetFacingDirection())
            {
                ChangeAttackDirection();
            }
        }

        public void StartAttack()
        {
            IsAttacking = true;
            ChangeAttackDirection();
        }
        
        public void StopAttack()
        {
            IsAttacking = false;
            upAttackCollider.enabled = false;
            downAttackCollider.enabled = false;
            rightAttackCollider.enabled = false;
        }

        private void ChangeAttackDirection()
        {
            switch (playerMovement.GetFacingDirection())
            {
                case CharacterFacingDirection.Down:
                    downAttackCollider.enabled = true;
                    upAttackCollider.enabled = false;
                    rightAttackCollider.enabled = false;
                    break;
                case CharacterFacingDirection.Up:
                    downAttackCollider.enabled = false;
                    upAttackCollider.enabled = true;
                    rightAttackCollider.enabled = false;
                    break;
                default:
                    downAttackCollider.enabled = false;
                    upAttackCollider.enabled = false;
                    rightAttackCollider.enabled = true;
                    break;
            }
            _facing = playerMovement.GetFacingDirection();
        }
        
    }
}