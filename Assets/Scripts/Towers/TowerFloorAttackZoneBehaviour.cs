using System.Collections.Generic;
using Enemies;
using UnityEngine;
using Utils;
using World;

namespace Towers
{
    public class TowerFloorAttackZoneBehaviour : MonoBehaviour
    {
        private int _range;
        private int _damage;
        private int _maxTargets;
        private float _secondsToAttack;
        private float _attackTimer;
        
        private List<EnemyHealthManager> _enemiesInRange = new List<EnemyHealthManager>();
        private bool IsAttacking => _enemiesInRange.Count > 0;
        
        private TowerFloorAnimationManager _towerFloorAnimationManager;
        
        [SerializeField] private AudioSource cannon;
        [SerializeField] private AudioClip cannonShot;
        
        private void Awake()
        {
            _towerFloorAnimationManager = GetComponentInParent<TowerFloorAnimationManager>();
            EventManager.Instance.StartListening(EventManager.EnemyKilled, EnemyKilled);
        }
        
        public void Init(int range, int damage, int maxTargets,  float secondsToAttack)
        {
            _range = range;
            _damage = damage;
            _maxTargets = maxTargets;
            _secondsToAttack = secondsToAttack;
            GetComponent<CircleCollider2D>().radius = _range;

        }

        private void EnemyKilled(object arg0)
        {
            if (arg0 is Transform enemyBody)
            {
                var enemy = enemyBody.parent.GetComponent<EnemyHealthManager>();
                if (_enemiesInRange.Contains(enemy))
                {
                    _enemiesInRange.Remove(enemy);
                }                
            }
        }

        private void Update()
        {
            if (IsAttacking)
            {
                _attackTimer += Time.deltaTime;
                if (!_towerFloorAnimationManager.IsShooting && _attackTimer >= _towerFloorAnimationManager.SecondsToAttackAnimation)
                {
                    bool down = false, left = false, right = false;
                    foreach (var enemy in _enemiesInRange)
                    {
                        var direction = (enemy.transform.position - transform.position).normalized;
                        var angle = Vector2.SignedAngle(Vector2.up, direction);
                        if (angle is > 135 or < -135)
                            down = true;
                        else if (angle is > 45 and < 135)
                            left = true;
                        else if (angle is < -45 and > -135)
                            right = true;
                    }
                    _towerFloorAnimationManager.UpdateShootingAnimation(down, left, right);
                }
                if (_attackTimer >= _secondsToAttack)
                {
                    _attackTimer = 0;
                    // copy the list to avoid concurrent modification
                    var enemiesToAttack = new List<EnemyHealthManager>(_enemiesInRange);
                    foreach (var enemy in enemiesToAttack)
                    {
                        enemy.TakeDamage(_damage, tower: true);
                    }
                    SoundManager.Instance.PlaySFX(cannon, cannonShot);
                }
            }
            else if (_attackTimer > 0)
                _attackTimer = 0;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.CompareTag("Enemy") && _enemiesInRange.Count < _maxTargets)
            {
                var enemy = other.GetComponent<EnemyHealthManager>();
                if (!_enemiesInRange.Contains(enemy))
                    _enemiesInRange.Add(enemy);
            }
        }
        
        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Enemy"))
            {
                _enemiesInRange.Remove(other.GetComponent<EnemyHealthManager>());
            }
        }
    }
}
