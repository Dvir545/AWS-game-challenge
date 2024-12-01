using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;
using Utils.Data;

namespace Towers
{
    public class TowerFloorAnimationManager: MonoBehaviour
    {
        [SerializeField] private Animator _centerAnimator;
        [SerializeField] private Animator _leftAnimator;
        [SerializeField] private Animator _rightAnimator;
        [SerializeField] private AnimationClip _shootAnimation;  // to get the duration of the shoot animation
        
        private static readonly int AnimationTowerType = Animator.StringToHash("tower_type");
        private static readonly int AnimationShoot = Animator.StringToHash("shoot");
        private static readonly int AnimationStop = Animator.StringToHash("stop");

        private SpriteRenderer _centerSpriteRenderer;
        private SpriteRenderer _leftSpriteRenderer;
        private SpriteRenderer _rightSpriteRenderer;

        private float _shootAnimationDuration;
        public float SecondsToAttackAnimation;
        public bool IsShooting;

        // private TowerSprites _sprites;

        private void Awake()
        {
            _centerSpriteRenderer = _centerAnimator.transform.GetComponent<SpriteRenderer>();
            _leftSpriteRenderer = _leftAnimator.transform.GetComponent<SpriteRenderer>();
            _rightSpriteRenderer = _rightAnimator.transform.GetComponent<SpriteRenderer>();
            _shootAnimationDuration = _shootAnimation.length;
        }

        public void Init(TowerMaterial material, int sortingOrder, float secondsToAttack)
        {
            _centerAnimator.SetInteger(AnimationTowerType, (int) material);
            _leftAnimator.SetInteger(AnimationTowerType, (int) material);
            _rightAnimator.SetInteger(AnimationTowerType, (int) material);
            // _sprites = TowersData.Instance.GetTowerData(material).GetSprites();
            _centerSpriteRenderer.sortingOrder = sortingOrder;
            _leftSpriteRenderer.sortingOrder = sortingOrder;
            _rightSpriteRenderer.sortingOrder = sortingOrder;
            SecondsToAttackAnimation = secondsToAttack - .8f*_shootAnimationDuration;
        }

        public void StartTower()
        {
            _centerSpriteRenderer.enabled = true;
            _leftSpriteRenderer.enabled = true;
            _rightSpriteRenderer.enabled = true;
        }

        public void Shoot(TowerShootingDirection direction)
        {
            switch (direction)
            {
                case TowerShootingDirection.Down:
                    _centerAnimator.SetTrigger(AnimationShoot);
                    break;
                case TowerShootingDirection.Left:
                    _leftAnimator.SetTrigger(AnimationShoot);
                    break;
                case TowerShootingDirection.Right:
                    _rightAnimator.SetTrigger(AnimationShoot);
                    break;
            }
        }

        public void UpdateShootingAnimation(bool down, bool left, bool right)
        {
            if (down)
                StartCoroutine(ShootCR(TowerShootingDirection.Down));
            if (left)
                StartCoroutine(ShootCR(TowerShootingDirection.Left));
            if (right)
                StartCoroutine(ShootCR(TowerShootingDirection.Right));
        }
        
        private IEnumerator ShootCR(TowerShootingDirection direction)
        {
            if (IsShooting) yield break;
            IsShooting = true;
            Shoot(direction);
            yield return new WaitForSeconds(_shootAnimationDuration);
            IsShooting = false;
        }
    }
}