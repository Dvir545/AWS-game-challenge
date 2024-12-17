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
            public AnimationClip jump;
            public AnimationClip hit;
            public AnimationClip die;

            public SlimeAnimations(AnimationClip idle, AnimationClip jump, AnimationClip hit, AnimationClip die)
            {
                this.idle = idle;
                this.jump = jump;
                this.hit = hit;
                this.die = die;
            }
        }
        
        [SerializeField] private SlimeAnimations[] slimeAnimations;
        
        protected override void Awake()
        {
            base.Awake();
            var slimeAnimator = animators[0];
            var animatorOverrideController = new AnimatorOverrideController(slimeAnimator.runtimeAnimatorController);
            int randomIndex = Random.Range(0, slimeAnimations.Length);
            var anims = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            anims.Add(new KeyValuePair<AnimationClip, AnimationClip>(slimeAnimator.runtimeAnimatorController.animationClips[0], slimeAnimations[randomIndex].idle));
            anims.Add(new KeyValuePair<AnimationClip, AnimationClip>(slimeAnimator.runtimeAnimatorController.animationClips[1], slimeAnimations[randomIndex].jump));
            anims.Add(new KeyValuePair<AnimationClip, AnimationClip>(slimeAnimator.runtimeAnimatorController.animationClips[2], slimeAnimations[randomIndex].hit));
            anims.Add(new KeyValuePair<AnimationClip, AnimationClip>(slimeAnimator.runtimeAnimatorController.animationClips[3], slimeAnimations[randomIndex].die));
            animatorOverrideController.ApplyOverrides(anims);
            slimeAnimator.runtimeAnimatorController = animatorOverrideController;
        }

    }
}