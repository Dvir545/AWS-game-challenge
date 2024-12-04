using System;
using UnityEngine;
using Utils;

namespace Enemies.Demon
{
    public class DemonAnimationManager: EnemyAnimationManager
    {
        private  static readonly int AnimationAttacking = Animator.StringToHash("attack");
        [SerializeField] private GameObject ballsManager;
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                foreach (var animator in animators)
                {
                    animator.SetTrigger(AnimationAttacking);
                }
            }
        }
        
        // public override void SetFacingDirection()
        // {
        //     FacingDirection facingDirection = EnemyMovementManager.GetFacingDirection();
        //     if (facingDirection == FacingDirection.Right)
        //     {
        //         transform.localScale = new Vector3(-1, 1, 1);
        //         ballsManager.transform.localScale = new Vector3(-1, 1, 1);
        //     } else if (facingDirection == FacingDirection.Left)
        //     {
        //         transform.localScale = new Vector3(1, 1, 1);
        //         ballsManager.transform.localScale = new Vector3(1, 1, 1);
        //     }
        //     ChangeDirection(facingDirection);
        // }
    }
}