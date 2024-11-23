using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Enemies.Chicken
{
    public class ChickenAnimationManager: EnemyAnimationManager
    {
        [Serializable]
        public struct ChickenAnimations
        {
            public AnimationClip idle;
            public AnimationClip run;
            public AnimationClip eat;
            public AnimationClip hit;
            public AnimationClip die;

            public ChickenAnimations(AnimationClip idle, AnimationClip run, AnimationClip eat, AnimationClip hit, AnimationClip die)
            {
                this.idle = idle;
                this.run = run;
                this.eat = eat;
                this.hit = hit;
                this.die = die;
            }
        }
        
        [SerializeField] private ChickenAnimations[] chickenAnimations;
        private AnimatorOverrideController _animatorOverrideController;
        private static readonly int AnimationEating = Animator.StringToHash("eating");
        
        private ChickenEatingManager _chickenEatingManager;
        
        protected override void Awake()
        {
            base.Awake();
            _chickenEatingManager = GetComponent<ChickenEatingManager>();
            var chickenAnimator = animators[0];
            _animatorOverrideController = new AnimatorOverrideController(chickenAnimator.runtimeAnimatorController);
            int randomIndex = Random.Range(0, chickenAnimations.Length);
            var anims = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            anims.Add(new KeyValuePair<AnimationClip, AnimationClip>(chickenAnimator.runtimeAnimatorController.animationClips[0], chickenAnimations[randomIndex].run));
            anims.Add(new KeyValuePair<AnimationClip, AnimationClip>(chickenAnimator.runtimeAnimatorController.animationClips[1], chickenAnimations[randomIndex].eat));
            anims.Add(new KeyValuePair<AnimationClip, AnimationClip>(chickenAnimator.runtimeAnimatorController.animationClips[2], chickenAnimations[randomIndex].hit));
            anims.Add(new KeyValuePair<AnimationClip, AnimationClip>(chickenAnimator.runtimeAnimatorController.animationClips[3], chickenAnimations[randomIndex].die));
            anims.Add(new KeyValuePair<AnimationClip, AnimationClip>(chickenAnimator.runtimeAnimatorController.animationClips[4], chickenAnimations[randomIndex].idle));
            _animatorOverrideController.ApplyOverrides(anims);
            chickenAnimator.runtimeAnimatorController = _animatorOverrideController;
        }

        protected override void Update()
        {
            base.Update();
            HandleEating();
        }
        
        private void HandleEating()
        {
            if (_chickenEatingManager.IsEating)
            {
                foreach (var animator in animators)
                {
                    animator.SetBool(AnimationEating, true);
                }
            }
            else
            {
                foreach (var animator in animators)
                {
                    animator.SetBool(AnimationEating, false);
                }
            }
        }
    }
}