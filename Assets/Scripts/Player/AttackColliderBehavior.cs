using Enemies;
using UnityEngine;
using Utils;

namespace Player
{
    public class AttackColliderBehavior : MonoBehaviour
    {
        [SerializeField] private PlayerData playerData;
        private float _timeSinceLastHit;
        private const float TimeBetweenHits = 0.2f;  // used when staying in collider

        private void HitEnemy(Transform enemy)
        {
            int damage = Constants.BasePlayerDamage * playerData.GetDamageMultiplier;
            enemy.GetComponent<EnemyHealthManager>().TakeDamage(damage, (enemy.position - transform.position).normalized);
            EventManager.Instance.TriggerEvent(EventManager.PlayerHitEnemy, null);
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Enemy"))
            {
                HitEnemy(other.transform);
            }
        }
        
        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.CompareTag("Enemy"))
            {
                _timeSinceLastHit += Time.deltaTime;
                if (_timeSinceLastHit >= TimeBetweenHits)
                {
                    HitEnemy(other.transform);
                    _timeSinceLastHit = 0;
                }
            }
        }
    }
}
