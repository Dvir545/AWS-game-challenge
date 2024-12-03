using UnityEngine;
using Utils;

namespace Player
{
    public class PlayerAttackManager: MonoBehaviour
    {
        [SerializeField] private PlayerMovement playerMovement;
        private FacingDirection _facing;
        
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
                case FacingDirection.Down:
                    downAttackCollider.enabled = true;
                    upAttackCollider.enabled = false;
                    rightAttackCollider.enabled = false;
                    break;
                case FacingDirection.Up:
                    downAttackCollider.enabled = false;
                    upAttackCollider.enabled = true;
                    rightAttackCollider.enabled = false;
                    break;
                default:
                    downAttackCollider.enabled = false;
                    upAttackCollider.enabled = false;
                    rightAttackCollider.enabled = true;
                    var transform1 = rightAttackCollider.transform;
                    var position = transform1.localPosition;
                    var scale = transform1.localScale;
                    if (playerMovement.GetFacingDirection() == FacingDirection.Right)
                    {
                        position = new Vector2(-Mathf.Abs(position.x), position.y);
                        scale = new Vector2(-Mathf.Abs(scale.x), scale.y);
                    }
                    if (playerMovement.GetFacingDirection() == FacingDirection.Left)
                    {
                        position = new Vector2(Mathf.Abs(position.x), position.y);
                        scale = new Vector2(Mathf.Abs(scale.x), scale.y);
                    }
                    transform1.localPosition = position;
                    transform1.localScale = scale;
                    break;
            }
            _facing = playerMovement.GetFacingDirection();
        }
        
    }
}