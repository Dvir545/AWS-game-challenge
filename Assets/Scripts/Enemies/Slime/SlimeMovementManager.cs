using System.Collections;
using UnityEngine;
using UnityEngine.AI;
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
        private Vector2 _nextDirection;
        private Vector2 _preJumpPosition = Vector2.zero;
        private Vector2 _postJumpPosition = Vector2.zero;

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
                var nextDirection = ((Vector2)(Agent.nextPosition - transform.position)).normalized;
                if (Vector2.Angle(nextDirection, _nextDirection) > 2)
                {
                    _nextDirection = nextDirection;
                }
                else
                {
                    _nextDirection = Random.insideUnitCircle.normalized;
                }
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
                float forceMagnitude = jumpDistance / jumpDuration;
                Vector2 direction;
                var distanceFromTarget = Vector2.Distance(transform.position, Agent.destination);
                var isStuck = Mathf.Abs(_postJumpPosition.x - _preJumpPosition.x) < 0.5f &&
                              Mathf.Abs(_postJumpPosition.y - _preJumpPosition.y) < 0.5f;
                if (distanceFromTarget > 10 || isStuck)
                    direction = _nextDirection;
                else
                    direction = ((Vector2)(Agent.destination - transform.position)).normalized;
                Vector2 jumpForce = direction * forceMagnitude;
                IsMoving = true;
                _enemyAnimationManager.SetJumpDuration(jumpDuration);
                _enemySoundManager.PlayJumpSound();
                _preJumpPosition = transform.position;
                Rb.AddForce(jumpForce, ForceMode2D.Impulse);
                yield return new WaitForSeconds(jumpDuration);
                if (_jumpCooldown > 0)  // because slime was hit
                    continue;
                _postJumpPosition = transform.position;
                _enemySoundManager.PlayLandSound();
                Agent.Warp(transform.position);
                Rb.velocity = Vector2.zero;
                _jumpCooldown = Random.Range(jumpMinCooldown, jumpMaxCooldown);
                yield return new WaitForSeconds(0.2f);  // to let agent advance a bit
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
        
        public override FacingDirection GetFacingDirection()
        {
            Vector2 direction = Agent.destination - transform.position;
            if (direction.x > 0)
                return FacingDirection.Left;
            if (direction.x < 0)
                return FacingDirection.Right;
            return CurDirection;
        }
    }
}
