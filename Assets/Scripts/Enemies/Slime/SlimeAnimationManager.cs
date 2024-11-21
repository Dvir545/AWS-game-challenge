using Unity.Mathematics;
using UnityEngine;

namespace Enemies.Slime
{
    public class SlimeAnimationManager: EnemyAnimationManager
    {
        private static readonly int AnimationJumpStart = Animator.StringToHash("jumpstart");
        private static readonly int AnimationJumpSpeed = Animator.StringToHash("jumpspeed");
        [SerializeField] private AnimationClip jumpStartAnimation;
        private float _jumpStartDuration;
        
        private void Start()
        {
            _jumpStartDuration = jumpStartAnimation.length;
        }

        public void JumpStart()
        {
            foreach (var animator in animators)
            {
                animator.SetTrigger(AnimationJumpStart);
            }
        }

        public void SetJumpDuration(float duration)
        {
            foreach (var animator in animators)
            {
                animator.SetFloat(AnimationJumpSpeed, _jumpStartDuration / duration);
            }
        }

    }
}