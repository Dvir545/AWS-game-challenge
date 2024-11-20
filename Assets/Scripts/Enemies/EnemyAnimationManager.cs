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

        private EnemyMovementManager _enemyMovementManager;
        private EnemyHealthManager _enemyHealthManager;
        [SerializeField] private Animator[] animators;
        [SerializeField] private SpriteRenderer[] spriteRenderers;
        
        void Start()
        {
            _enemyMovementManager = GetComponent<EnemyMovementManager>();
            _enemyHealthManager = GetComponent<EnemyHealthManager>();
        }

        void Update()
        {
            HandleWalking();
            SetFacingDirection();
        }

        private void HandleWalking()
        {
            if (_enemyMovementManager.IsMoving)
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

        private void SetFacingDirection()
        {
            CharacterFacingDirection facingDirection = _enemyMovementManager.GetFacingDirection();
            if (facingDirection == CharacterFacingDirection.Right)
            {
                foreach (var spriteRenderer in spriteRenderers)
                {
                    spriteRenderer.flipX = true;
                }
            } else if (facingDirection == CharacterFacingDirection.Left)
            {
                foreach (var spriteRenderer in spriteRenderers)
                {
                    spriteRenderer.flipX = false;
                }
            }
            ChangeDirection(facingDirection);
        }
        
        private void ChangeDirection(CharacterFacingDirection direction)
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
