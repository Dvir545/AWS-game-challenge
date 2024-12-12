using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.Data;
using Vector3 = UnityEngine.Vector3;

namespace Player
{
    public class PlayerHealthManager : MonoBehaviour
    {
        [SerializeField] private GameObject heartUIPrefab;
        [SerializeField] private GameObject heartsUIParent;
    
        [SerializeField] private Sprite fullHeartSprite;
        [SerializeField] private Sprite halfHeartSprite;
        [SerializeField] private Sprite emptyHeartSprite;

        [SerializeField] private PlayerData playerData;
        [SerializeField] private EffectsManager effectsManager;

        private List<GameObject> _heartPrefabs = new();
        private List<Image> _heartImages = new();
        private bool _canGetHit = true;
        private Coroutine _healCoroutine;
        private const int XOffsetBetweenHearts = 128;
        private const float HitTime = 0.25f;

        private void Start()
        {
            AddUIHealth(playerData.MaxHealth);
            UpdateUIHealth(playerData.CurHealth);
            EventManager.Instance.StartListening(EventManager.MaxHealthIncreased, AddUIHealth);
            EventManager.Instance.StartListening(EventManager.HealthChanged, UpdateUIHealth);
        }

        private void AddUIHealth(object arg0)
        {
            if (arg0 is int maxHealth)
            {
                int nHearts = maxHealth / 2;
                int nNew = nHearts - _heartPrefabs.Count;
                for (int i = 0; i < nNew; i++)
                {
                    GameObject newPrefab = Instantiate(heartUIPrefab,
                        new Vector3(XOffsetBetweenHearts * _heartPrefabs.Count, 0, 0), Quaternion.identity);
                    // todo why its spawned at bottom of screen?
                    newPrefab.transform.SetParent(heartsUIParent.transform, worldPositionStays:false);
                    _heartPrefabs.Add(newPrefab);
                    _heartImages.Add(newPrefab.GetComponent<Image>());
                }
                UpdateUIHealth(playerData.CurHealth);
            }
        }

        private void UpdateUIHealth(object arg0)
        {
            if (arg0 is int curHealth)
            {
                int nFull = Mathf.FloorToInt(curHealth / 2);
                int nHalf = curHealth % 2;
                for (int i = 0; i < _heartPrefabs.Count; i++)
                {
                    Sprite sprite;
                    if (nFull > 0)
                    {
                        sprite = fullHeartSprite;
                        nFull--;
                    } else if (nHalf > 0)
                    {
                        sprite = halfHeartSprite;
                        nHalf--;
                    }
                    else
                    {
                        sprite = emptyHeartSprite;
                    }
                    _heartImages[i].sprite = sprite;
                }
            }
        }

        public void GotHit(Enemy enemyType, Vector3 hitDirection)
        {
            if (!_canGetHit) return;
            StartCoroutine(GotHitCoroutine(enemyType, hitDirection));
        }

        private IEnumerator GotHitCoroutine(Enemy enemyType, Vector3 hitDirection)
        {
            _canGetHit = false;
            int damage = Constants.BaseEnemyDamage * EnemyData.GetDamageMultiplier(enemyType);
            playerData.SubtractHealth(damage);
            if (playerData.CurHealth <= 0)
            {
                EventManager.Instance.TriggerEvent(EventManager.PlayerDied, null);
                yield break;
            }

            var pushForceMultiplier = EnemyData.GetPushForceMultiplier(enemyType);
            EventManager.Instance.TriggerEvent(EventManager.PlayerGotHit, (HitTime, hitDirection, pushForceMultiplier));
            effectsManager.FloatingTextEffect(transform.position, 2, .5f, damage.ToString(), Constants.PlayerDamageColor, 1.5f);
            
            yield return new WaitForSeconds(HitTime);
            _canGetHit = true;
        }
        
        public void StartHeal()
        {
            _healCoroutine = StartCoroutine(HealCoroutine());
        }
        
        public void StopHeal()
        {
            StopCoroutine(_healCoroutine);
        }

        private IEnumerator HealCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(Constants.BaseSecondsPerHeal / playerData.RegenSpeedMultiplier);
                playerData.IncHealth();
            }
        }
    }
}