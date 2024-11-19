using UnityEngine;
using Utils;

namespace Enemies
{
    public class EnemyAnimationManager : MonoBehaviour
    {
        private  static readonly int AnimationWalking = Animator.StringToHash("walking");
        private  static readonly int AnimationFacing = Animator.StringToHash("facing");

        private EnemyFollowPlayer _enemyFollowPlayer;
        [SerializeField] private Animator[] animators;
        [SerializeField] private SpriteRenderer[] spriteRenderers;
        
        // Start is called before the first frame update
        void Start()
        {
            foreach (var animator in animators)
            {
                animator.SetBool(AnimationWalking, true);
            }
            _enemyFollowPlayer = GetComponent<EnemyFollowPlayer>();
        }

        // Update is called once per frame
        void Update()
        {
            SetFacingDirection();
        }
        
        private void SetFacingDirection()
        {
            CharacterFacingDirection facingDirection = _enemyFollowPlayer.GetFacingDirection();
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
    }
}
