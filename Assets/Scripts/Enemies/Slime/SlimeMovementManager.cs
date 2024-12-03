using System.Collections;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

namespace Enemies.Slime
{
    public class SlimeMovementManager : EnemyMovementManager
    {
        [SerializeField] private float jumpMinDistance;
        [SerializeField] private float jumpMaxDistance;
        [SerializeField] private float jumpSpeed;
        [SerializeField] private float jumpMinCooldown;
        [SerializeField] private float jumpMaxCooldown;
        [SerializeField] private float jumpStartDuration;
        private float _jumpCooldown;
        private SlimeAnimationManager _enemyAnimationManager;
        private SlimeSoundManager _enemySoundManager;
        private EnemyHealthManager _enemyHealthManager;
        private Coroutine _jumpCoroutine;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            StopUpdatingPath();
            Agent.updatePosition = false;
            Agent.velocity = Vector3.zero;
            IsMoving = false;
            _enemyAnimationManager = GetComponent<SlimeAnimationManager>();
            _enemySoundManager = GetComponent<SlimeSoundManager>();
            _enemyHealthManager = GetComponent<EnemyHealthManager>();
            _jumpCoroutine = StartCoroutine(Jump());
        }

        private IEnumerator Jump()
        {
            while (!_enemyHealthManager.IsDead)
            {
                while (_jumpCooldown > 0)
                {
                    _jumpCooldown -= Time.deltaTime;
                    yield return null;
                }
                // start jumping
                FindClosestTarget();
                _enemyAnimationManager.JumpStart(jumpStartDuration);
                yield return new WaitForSeconds(jumpStartDuration);
                if (_jumpCooldown > 0)   // because slime was hit
                    continue;
                float jumpDistance = Random.Range(jumpMinDistance, jumpMaxDistance);
                float jumpDuration = jumpDistance / jumpSpeed;
                Vector2 direction = (Agent.destination - transform.position).normalized;
                float forceMagnitude = jumpDistance / jumpDuration;
                Vector2 jumpForce = direction * forceMagnitude;
                IsMoving = true;
                _enemyAnimationManager.SetJumpDuration(jumpDuration);
                _enemySoundManager.PlayJumpSound();
                Rb.AddForce(jumpForce, ForceMode2D.Impulse);
                yield return new WaitForSeconds(jumpDuration);
                if (_jumpCooldown > 0)  // because slime was hit
                    continue;
                _enemySoundManager.PlayLandSound();
                Rb.velocity = Vector2.zero;
                _jumpCooldown = Random.Range(jumpMinCooldown, jumpMaxCooldown);
            }
        }

        protected override IEnumerator KnockbackCoroutine(Vector2 hitDirection, float hitTime, bool dead = false)
        {
            IsMoving = false;
            _jumpCooldown = 0.01f;
            Rb.AddForce(hitDirection * Constants.KnockbackForce, ForceMode2D.Impulse);
            yield return new WaitForSeconds(hitTime);
            Rb.velocity = Vector2.zero;
            StopCoroutine(_jumpCoroutine);
            _jumpCoroutine = StartCoroutine(Jump());
        }
        
        public override CharacterFacingDirection GetFacingDirection()
        {
            Vector2 direction = Agent.destination - transform.position;
            if (direction.x > 0)
                return CharacterFacingDirection.Left;
            if (direction.x < 0)
                return CharacterFacingDirection.Right;
            return CurDirection;
        }
    }
}
