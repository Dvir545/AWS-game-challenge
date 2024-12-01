using System.Collections.Generic;
using Enemies;
using UnityEngine;

namespace Towers
{
    public class TowerFloorAttackZoneBehaviour : MonoBehaviour
    {
        private int _range;
        private int _damage;
        private float _secondsToAttack;
        private float _attackTimer;
        
        private List<EnemyHealthManager> _enemiesInRange = new List<EnemyHealthManager>();
        private bool IsAttacking => _enemiesInRange.Count > 0;
        
        private TowerFloorAnimationManager _towerFloorAnimationManager;
        
        private void Awake()
        {
            _towerFloorAnimationManager = GetComponentInParent<TowerFloorAnimationManager>();
        }
        
        public void Init(int range, int damage, float secondsToAttack)
        {
            _range = range;
            _damage = damage;
            _secondsToAttack = secondsToAttack;
            GetComponent<CircleCollider2D>().radius = _range;
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
                    foreach (var enemy in _enemiesInRange)
                    {
                        enemy.TakeDamage(_damage);
                    }
                }
            }
            else if (_attackTimer > 0)
                _attackTimer = 0;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Enemy"))
            {
                _enemiesInRange.Add(other.GetComponent<EnemyHealthManager>());
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
