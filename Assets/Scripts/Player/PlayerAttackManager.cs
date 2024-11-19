using System;
using NUnit.Framework;
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
        public bool IsAttacking => upAttackCollider.enabled || downAttackCollider.enabled || rightAttackCollider.enabled;
        [SerializeField] private Animator playerAnimator;
        private PlayerAttackBehaviour[] _playerAttackBehaviors;

        private void Start()
        {
            upAttackCollider.enabled = false;
            downAttackCollider.enabled = false;
            rightAttackCollider.enabled = false;
            _playerAttackBehaviors = playerAnimator.GetBehaviours<PlayerAttackBehaviour>();
            foreach (var playerAttackBehavior in _playerAttackBehaviors)
            {
                playerAttackBehavior.Init(this);
            }
        }

        private void Update()
        {
            if (IsAttacking)
            {
                // check for direction change
                if (_facing != playerMovement.GetFacingDirection())
                {
                    StopAttack();
                    StartAttack();
                }
            }
        }

        public void StartAttack()
        {
            switch (playerMovement.GetFacingDirection())
            {
                case CharacterFacingDirection.Up:
                    upAttackCollider.enabled = true;
                    break;
                case CharacterFacingDirection.Down:
                    downAttackCollider.enabled = true;
                    break;
                default:
                    rightAttackCollider.enabled = true;
                    break;
            }
            _facing = playerMovement.GetFacingDirection();
        }
        
        public void StopAttack()
        {
            upAttackCollider.enabled = false;
            downAttackCollider.enabled = false;
            rightAttackCollider.enabled = false;
        }
        
    }
}