using System;
using System.Collections;
using UnityEngine;
using Utils;

namespace Enemies.Goblin
{
    public class GoblinAttackManager : MonoBehaviour
    {
        [SerializeField] private float secondsBetweenAttacks = 1.5f;
        [SerializeField] private Transform shootPointDown;
        [SerializeField] private Transform shootPointUp;
        [SerializeField] private Transform shootPointRight;
        private GoblinAnimationManager _animationManager;
        private GoblinSoundManager _soundManager;
        private EnemyMovementManager _movementManager;
        private EnemyHealthManager _healthManager;

        private Coroutine _attackCoroutine;

        private void Awake()
        {
            _animationManager = GetComponent<GoblinAnimationManager>();
            _soundManager = GetComponent<GoblinSoundManager>();
            _movementManager = GetComponent<EnemyMovementManager>();
            _healthManager = GetComponent<EnemyHealthManager>();
        }

        private void Update()
        {
            if (_healthManager.IsDead)
            {
                StopAttacking();
            }
        }

        private void Attack(Vector2 target)
        {
            var facing = _movementManager.GetFacingDirection();
            Transform shootPoint = facing switch
            {
                FacingDirection.Down => shootPointDown,
                FacingDirection.Up => shootPointUp,
                FacingDirection.Right => shootPointRight,
                FacingDirection.Left => shootPointRight,
                _ => shootPointDown
            };
            Vector2 position = shootPoint.position;
            ArrowPool.Instance.SpawnArrow(position, target - position, facing, transform);
        }

        private IEnumerator AttackCoroutine(Transform target)
        {
            while (true)
            {
                yield return new WaitForSeconds(secondsBetweenAttacks);
                _animationManager.Shoot();
                _soundManager.PlayShootSound();
                yield return new WaitForSeconds(_animationManager.GetShootAnimationLength()/2f);  // wait for animation to reach the shoot point
                Attack(target.position + new Vector3(0, 0.5f, 0));
            }
        }
        
        public void StartAttacking(Transform target)
        {
            if (_attackCoroutine != null)
                StopCoroutine(_attackCoroutine);
            _attackCoroutine = StartCoroutine(AttackCoroutine(target));
        }
        
        public void StopAttacking()
        {
            if (_attackCoroutine != null)
            {
                StopCoroutine(_attackCoroutine);
                _attackCoroutine = null;
            }
        }
    }
}