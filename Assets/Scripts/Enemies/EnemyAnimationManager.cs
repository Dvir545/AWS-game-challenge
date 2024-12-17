using UnityEngine;
using Utils;

namespace Enemies
{
    public class EnemyAnimationManager : MonoBehaviour
    {
        private  static readonly int AnimationWalking = Animator.StringToHash("walking");
        private  static readonly int AnimationFacing = Animator.StringToHash("facing");
        private static readonly int AnimationGotHit = Animator.StringToHash("hit");
        private static readonly int AnimationDeath = Animator.StringToHash("die");
        private static readonly int AnimationWalkSpeed = Animator.StringToHash("walkspeed");

        protected EnemyMovementManager EnemyMovementManager;
        [SerializeField] protected Animator[] animators;
        [SerializeField] private SpriteRenderer[] spriteRenderers;
        
        protected virtual void Awake()
        {
            EnemyMovementManager = GetComponent<EnemyMovementManager>();
            foreach (var animator in animators)
            {
                animator.SetFloat(AnimationWalkSpeed, EnemyMovementManager.SpeedMultiplier);
            }
        }

        protected virtual void Update()
        {
            HandleWalking();
            if (EnemyMovementManager.Targeted)
            {
                SetFacingDirection();
            }
        }

        private void HandleWalking()
        {
            if (EnemyMovementManager.IsMoving)
            {
                foreach (var animator in animators)
                {
                    animator.SetBool(AnimationWalking, true);
                }
            }
            else
            {
                foreach (var animator in animators)
                {
                    animator.SetBool(AnimationWalking, false);
                }
            }
        }

        public virtual void SetFacingDirection()
        {
            FacingDirection facingDirection = EnemyMovementManager.GetFacingDirection();
            if (facingDirection == FacingDirection.Right)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            } else if (facingDirection == FacingDirection.Left)
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
            ChangeDirection(facingDirection);
        }
        
        protected void ChangeDirection(FacingDirection direction)
        {
            foreach (var animator in animators)
            {
                if (animator.GetInteger(AnimationFacing) != (int)direction)
                {
                    animator.SetInteger(AnimationFacing, (int)direction);
                }
            }
        }
        
        public void GotHit()
        {
            foreach (var animator in animators)
            {
                animator.SetTrigger(AnimationGotHit);
            }
        }

        public void Die()
        {
            foreach (var animator in animators)
            {
                animator.SetTrigger(AnimationDeath);
            }
        }
        
    }
}
