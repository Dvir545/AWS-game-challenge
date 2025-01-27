﻿using System.Collections;
using Player;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.Data;
using World;

namespace Enemies
{
    public class EnemyHealthManager: MonoBehaviour
    {
        [SerializeField] public Enemy enemyType;
        [SerializeField] private int _maxHealthOverride = 0;
        public int MaxHealth { get; private set; }
        public int CurHealth { get; private set; }
        public bool IsDead => CurHealth <= 0;
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
        private const float TimeBetweenHits = 0.5f;  // used when staying in collider
        private float _timeSinceLastHit = 0;

        private void Awake()
        {
            MaxHealth = _maxHealthOverride == 0 ? EnemyData.GetMaxHealth(enemyType) : _maxHealthOverride;
            CurHealth = MaxHealth;
            _enemyAnimationManager = GetComponent<EnemyAnimationManager>();
            _enemyMovementManager = GetComponent<EnemyMovementManager>();
            _enemySoundManager = GetComponent<EnemySoundManager>();
            _effectsManager = FindObjectOfType<EffectsManager>();
            _playerData = FindObjectOfType<PlayerData>();
            healthBar.SetActive(false);
            healthBarFiller.fillAmount = 1;
        }
        
        public virtual void  TakeDamage(int damage, Vector2? hitDirection = null, bool tower = false)
        {
            if (CurHealth <= 0) return;
            if (!_canGetHit && !tower) return;
            StartCoroutine(TakeDamageCoroutine(damage, hitDirection, tower));
        }
        
        private IEnumerator TakeDamageCoroutine(int damage, Vector2? hitDirection = null, bool tower = false)
        {
            if (tower)
                damage = Mathf.Max(1, Mathf.FloorToInt(damage * EnemyData.GetTowerDamageMultiplier(enemyType)));
            else 
                _canGetHit = false;
            CurHealth -= damage;
            UpdateHealthBar();
            _enemySoundManager.PlayHitSound();
            _effectsManager.FloatingTextEffect(transform.position, 2, .5f, damage.ToString(), Constants.EnemyDamageColor);
            if (CurHealth > 0)
            {
                if (!(enemyType == Enemy.Orc && tower))  // orcs destroy towers so we do not want hit anim from towers
                    _enemyAnimationManager.GotHit();
                if (hitDirection != null)
                    _enemyMovementManager.Knockback((Vector2)hitDirection, HitTime);
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
            healthBarFiller.fillAmount = (float)CurHealth / MaxHealth;
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

        private IEnumerator Die(Vector2? hitDirection = null)
        {
            _enemyMovementManager.Die(hitDirection);
            _enemyAnimationManager.Die();
            var soundLength = _enemySoundManager.PlayDeathSound();
            var waitTime = Mathf.Max(1.5f, soundLength);
            DayNightManager.Instance.EnemyDied(waitTime);
            var body = transform.GetChild(0);
            EventManager.Instance.TriggerEvent(EventManager.EnemyKilled, body.transform);
            yield return new WaitForSeconds(waitTime);
            int cashDrop = EnemyData.GetCashDrop(enemyType);
            _effectsManager.FloatingTextEffect(transform.position, 1, 1, 
                cashDrop.ToString() + "$", Constants.CashColor);
            _playerData.AddCash(cashDrop);
            EnemyPool.Instance.ReleaseEnemy(gameObject, enemyType);
        }
        
        private void HitPlayer(Transform player)
        {
            var hitDirection = (player.position - transform.position).normalized;
            player.GetComponent<PlayerHealthManager>().GotHit(enemyType, hitDirection);
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (CurHealth <= 0) return;
            if (other.CompareTag("Player"))
            {
                HitPlayer(other.transform);
            }
        }
        
        private void OnTriggerStay2D(Collider2D other)
        {
            if (CurHealth <= 0) return;
            if (other.CompareTag("Player"))
            {
                _timeSinceLastHit += Time.deltaTime;
                if (_timeSinceLastHit >= TimeBetweenHits)
                {
                    HitPlayer(other.transform);
                    _timeSinceLastHit = 0;
                }
            }
        }

        public void Reset()
        {
            CurHealth = MaxHealth;
            UpdateHealthBar();
            _canGetHit = true;
        }
    }
}