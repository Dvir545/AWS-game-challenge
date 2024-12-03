using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Enemies.Slime
{
    public class SlimeAnimationManager: EnemyAnimationManager
    {
        [Serializable]
        public struct SlimeAnimations
        {
            public AnimationClip idle;
            public AnimationClip jumpstart;
            public AnimationClip jump;
            public AnimationClip jumpend;
            public AnimationClip hit;
            public AnimationClip die;

            public SlimeAnimations(AnimationClip idle, AnimationClip jumpstart, AnimationClip jump, AnimationClip jumpend, AnimationClip hit, AnimationClip die)
            {
                this.idle = idle;
                this.jumpstart = jumpstart;
                this.jump = jump;
                this.jumpend = jumpend;
                this.hit = hit;
                this.die = die;
            }
        }
        
        [SerializeField] private SlimeAnimations[] slimeAnimations;
        
        private static readonly int AnimationJumpStart = Animator.StringToHash("jumpstart");
        private static readonly int AnimationJumpSpeed = Animator.StringToHash("jumpspeed");
        [SerializeField] private AnimationClip jumpAnimation;
        private float _jumpDuration;
        private Coroutine _jumpStartCR;

        protected override void Awake()
        {
            base.Awake();
            var slimeAnimator = animators[0];
            var animatorOverrideController = new AnimatorOverrideController(slimeAnimator.runtimeAnimatorController);
            int randomIndex = Random.Range(0, slimeAnimations.Length);
            var anims = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            anims.Add(new KeyValuePair<AnimationClip, AnimationClip>(slimeAnimator.runtimeAnimatorController.animationClips[0], slimeAnimations[randomIndex].jumpstart));
            anims.Add(new KeyValuePair<AnimationClip, AnimationClip>(slimeAnimator.runtimeAnimatorController.animationClips[1], slimeAnimations[randomIndex].idle));
            anims.Add(new KeyValuePair<AnimationClip, AnimationClip>(slimeAnimator.runtimeAnimatorController.animationClips[2], slimeAnimations[randomIndex].jump));
            anims.Add(new KeyValuePair<AnimationClip, AnimationClip>(slimeAnimator.runtimeAnimatorController.animationClips[3], slimeAnimations[randomIndex].hit));
            anims.Add(new KeyValuePair<AnimationClip, AnimationClip>(slimeAnimator.runtimeAnimatorController.animationClips[4], slimeAnimations[randomIndex].die));
            anims.Add(new KeyValuePair<AnimationClip, AnimationClip>(slimeAnimator.runtimeAnimatorController.animationClips[5], slimeAnimations[randomIndex].jumpend));
            animatorOverrideController.ApplyOverrides(anims);
            slimeAnimator.runtimeAnimatorController = animatorOverrideController;
        }
        
        private void Start()
        {
            _jumpDuration = jumpAnimation.length;
        }
        
        public void JumpStart(float jumpStartDuration)
        {
            if (_jumpStartCR != null)
                StopCoroutine(_jumpStartCR);
            _jumpStartCR = StartCoroutine(JumpStartCR(jumpStartDuration));
        }

        public IEnumerator JumpStartCR(float jumpStartDuration)
        {
            foreach (var animator in animators)
            {
                animator.SetBool(AnimationJumpStart, true);
            }
            yield return new WaitForSeconds(jumpStartDuration);
            foreach (var animator in animators)
            {
                animator.SetBool(AnimationJumpStart, false);
            }
        }

        public void SetJumpDuration(float duration)
        {
            foreach (var animator in animators)
            {
                animator.SetFloat(AnimationJumpSpeed, _jumpDuration / duration);
            }
        }

    }
}