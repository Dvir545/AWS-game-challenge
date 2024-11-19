using System.Collections;
using UnityEngine;
using Utils;

namespace World.Enemies
{
    public class EnemyHealthManager: MonoBehaviour
    {
        [SerializeField] private int maxHealth;
        private int _curHealth;
        
        [SerializeField] private Animator animator;
        private static readonly int AnimationGotHit = Animator.StringToHash("hit");
        private static readonly int AnimationDeath = Animator.StringToHash("die");
        
        private Rigidbody2D _rb;
        private EnemyHitAnimBehaviour _enemyHitAnimBehaviour;
        
        private void Start()
        {
            _curHealth = maxHealth;
            _rb = GetComponent<Rigidbody2D>();
            _enemyHitAnimBehaviour = animator.GetBehaviour<EnemyHitAnimBehaviour>();
            _enemyHitAnimBehaviour.Init(animator);
        }
        
        public void TakeDamage(int damage, Vector2 hitDirection)
        {
            _curHealth -= damage;
            if (_curHealth > 0)
            {
                StartCoroutine(HitCoroutine());
            }
            else
            {
                StartCoroutine(DieCoroutine());
            }
        }

        private IEnumerator HitCoroutine()
        {
            throw new System.NotImplementedException();
        }

        private IEnumerator DieCoroutine()
        {
            animator.SetBool(AnimationGotHit, true);
            yield return new WaitForSeconds(1.5f);
            animator.SetBool(AnimationDeath, true);
            yield return new WaitForSeconds(1.5f);
            Destroy(gameObject);
        }
    }
}