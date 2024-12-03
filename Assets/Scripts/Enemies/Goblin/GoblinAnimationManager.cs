using UnityEngine;

namespace Enemies.Goblin
{
    public class GoblinAnimationManager: EnemyAnimationManager
    {
        private static readonly int AnimationShoot = Animator.StringToHash("shoot");
        
        [SerializeField] private AnimationClip shootAnimation;
        
        public void Shoot()
        {
            foreach (var animator in animators)
            {
                animator.SetTrigger(AnimationShoot);
            }
        }
        
        public float GetShootAnimationLength()
        {
            return shootAnimation.length;
        }
    }
}