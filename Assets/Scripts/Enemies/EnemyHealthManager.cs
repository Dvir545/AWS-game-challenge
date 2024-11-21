using System.Collections;
using Player;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.Data;

namespace Enemies
{
    public class EnemyHealthManager: MonoBehaviour
    {
        [SerializeField] private Enemy enemyType;
        private int _maxHealth;
        private int _curHealth;
        public bool IsDead => _curHealth <= 0;
        private EnemyAnimationManager _enemyAnimationManager;
        private EnemyMovementManager _enemyMovementManager;
        private EnemySoundManager _enemySoundManager;
        private EffectsManager _effectsManager;
        private PlayerData _playerData;
        [SerializeField] private GameObject healthBar;
        [SerializeField] private Image healthBarFiller;
        private bool _canGetHit = true;
        private const float HitTime = 0.25f;
        private Coroutine _currentHealthBarCoroutine;

        private void Awake()
        {
            _maxHealth = EnemyData.GetMaxHealth(enemyType);
            _curHealth = _maxHealth;
            _enemyAnimationManager = GetComponent<EnemyAnimationManager>();
            _enemyMovementManager = GetComponent<EnemyMovementManager>();
            _enemySoundManager = GetComponent<EnemySoundManager>();
            _effectsManager = FindObjectOfType<EffectsManager>();
            _playerData = FindObjectOfType<PlayerData>();
            healthBar.SetActive(false);
            healthBarFiller.fillAmount = 1;
        }
        
        public void TakeDamage(int damage, Vector2 hitDirection)
        {
            if (_curHealth <= 0) return;
            if (!_canGetHit) return;
            StartCoroutine(TakeDamageCoroutine(damage, hitDirection));
        }
        
        private IEnumerator TakeDamageCoroutine(int damage, Vector2 hitDirection)
        {
            _canGetHit = false;
            _curHealth -= damage;
            UpdateHealthBar();
            _enemySoundManager.PlayHitSound();
            _effectsManager.FloatingTextEffect(transform.position, 2, .5f, damage.ToString(), Constants.EnemyDamageColor);
            if (_curHealth > 0)
            {
                _enemyAnimationManager.GotHit();
                _enemyMovementManager.Knockback(hitDirection, HitTime);
                yield return new WaitForSeconds(HitTime);
                _canGetHit = true;
            }
            else
            {
                StartCoroutine(Die(hitDirection));
            }
        }

        private IEnumerator HealthBarCoroutine()
        {
            healthBarFiller.fillAmount = (float)_curHealth / _maxHealth;
            healthBar.SetActive(true);
            yield return new WaitForSeconds(1.5f);
            healthBar.SetActive(false);
        }

        public void UpdateHealthBar()
        {
            if (_currentHealthBarCoroutine != null)
            {
                StopCoroutine(_currentHealthBarCoroutine);
            }
            _currentHealthBarCoroutine = StartCoroutine(HealthBarCoroutine());
        }

        private IEnumerator Die(Vector2 hitDirection)
        {
            _enemyMovementManager.Die(hitDirection);
            _enemyAnimationManager.Die();
            _enemySoundManager.PlayDeathSound();
            yield return new WaitForSeconds(1.5f);
            int cashDrop = EnemyData.GetCashDrop(enemyType);
            _effectsManager.FloatingTextEffect(transform.position, 1, 1, 
                cashDrop.ToString() + "$", Constants.CashColor);
            _playerData.AddCash(cashDrop);
            Destroy(gameObject);
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_curHealth <= 0) return;
            if (other.CompareTag("Player"))
            {
                var hitDirection = (other.transform.position - transform.position).normalized;
                other.gameObject.GetComponent<PlayerHealthManager>().GotHit(enemyType, hitDirection);
            }
        }
    }
}