using Enemies;
using UnityEngine;
using Utils;

namespace Player
{
    public class AttackColliderBehavior : MonoBehaviour
    {
        [SerializeField] private PlayerData playerData; 
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Enemy"))
            {
                int damage = Constants.BasePlayerDamage * playerData.GetDamageMultiplier;
                other.GetComponent<EnemyHealthManager>().TakeDamage(damage, (other.transform.position - transform.position).normalized);
                EventManager.Instance.TriggerEvent(EventManager.PlayerHitEnemy, null);
            }
        }
    }
}
