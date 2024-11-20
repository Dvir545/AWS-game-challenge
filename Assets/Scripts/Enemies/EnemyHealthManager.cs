using System.Collections;
using Player;
using UnityEngine;
using Utils;
using Utils.Data;

namespace Enemies
{
    public class EnemyHealthManager: MonoBehaviour
    {
        [SerializeField] private Enemy enemyType;
        private int _curHealth;
        private EnemyAnimationManager _enemyAnimationManager;
        private EnemyMovementManager _enemyMovementManager;
        [SerializeField] private EffectsManager effectsManager;
        [SerializeField] private PlayerData playerData;
        private bool _canGetHit = true;
        private const float HitTime = 0.25f;


        private void Start()
        {
            _curHealth = EnemyData.GetMaxHealth(enemyType);
            _enemyAnimationManager = GetComponent<EnemyAnimationManager>();
            _enemyMovementManager = GetComponent<EnemyMovementManager>();
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
            if (_curHealth > 0)
            {
                _enemyAnimationManager.GotHit();
                _enemyMovementManager.Knockback(hitDirection, HitTime);
                effectsManager.FloatingTextEffect(transform.position, 2, .5f, damage.ToString(), Constants.EnemyDamageColor);
                yield return new WaitForSeconds(HitTime);
                _canGetHit = true;
            }
            else
            {
                StartCoroutine(Die(hitDirection));
            }
        }

        private IEnumerator Die(Vector2 hitDirection)
        {
            _enemyMovementManager.Die(hitDirection);
            _enemyAnimationManager.Die();
            yield return new WaitForSeconds(1.5f);
            int cashDrop = EnemyData.GetCashDrop(enemyType);
            effectsManager.FloatingTextEffect(transform.position, 1, 1, 
                cashDrop.ToString() + "$", Constants.CashColor);
            playerData.AddCash(cashDrop);
            Destroy(gameObject);
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                var hitDirection = (other.transform.position - transform.position).normalized;
                other.gameObject.GetComponent<PlayerHealthManager>().GotHit(enemyType, hitDirection);
            }
        }
    }
}