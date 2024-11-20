using UnityEngine;
using Utils;

namespace Player
{
    public class PlayerAnimationManager: MonoBehaviour
    {
        [SerializeField] private Animator[] animators;
        [SerializeField] private SpriteRenderer[] spriteRenderers;  // for flipping
        private static readonly int AnimationMoving = Animator.StringToHash("moving");
        private static readonly int AnimationFacing = Animator.StringToHash("facing");
        private static readonly int AnimationActing = Animator.StringToHash("acting");
        private static readonly int AnimationActType = Animator.StringToHash("act_type");
        private static readonly int AnimationGotHit = Animator.StringToHash("hit");
        private static readonly int AnimationDeath = Animator.StringToHash("die");

        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private PlayerAction playerAction;
        [SerializeField] private PlayerData playerData;
        
        private void Update()
        {
            bool isMoving = playerMovement.IsMoving;
            int facing = (int)playerMovement.GetFacingDirection();
            bool isActing =playerAction.IsActing;
            int actType = (int)playerData.GetCurTool();
            foreach (var animator in animators)
            {
                UpdateAnimator(animator, isMoving, facing, isActing, actType);
            }
        }

        private void UpdateAnimator(Animator animator, bool isMoving, int facing, bool isActing, int actType)
        {
            animator.SetBool(AnimationMoving, isMoving);
            animator.SetInteger(AnimationFacing, facing);
            animator.SetBool(AnimationActing, isActing);
            animator.SetInteger(AnimationActType, actType);
            if (facing == (int)CharacterFacingDirection.Right)
            {
                foreach (var spriteRenderer in spriteRenderers)
                {
                    spriteRenderer.flipX = true;
                }
            } else if (facing == (int)CharacterFacingDirection.Left)
            {
                foreach (var spriteRenderer in spriteRenderers)
                {
                    spriteRenderer.flipX = false;
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
    }
}