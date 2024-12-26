using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.Data;
using World;
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
        private PlayerSoundManager _playerSoundManager;

        private List<GameObject> _heartPrefabs = new();
        private List<Image> _heartImages = new();
        private float _heartScale;
        private bool _canGetHit = true;
        private Coroutine _healCoroutine;
        private bool _init;
        private const float XStartOffset = -296;//-360 + 64;
        private const int XOffsetBetweenHearts = 128;
        private const float HitTime = .25f;
        private const float HitMercyTime = .5f;
        
        public bool IsDead => GameData.Instance.curHealth <= 0;

        private void Start()
        {
            _playerSoundManager = GetComponent<PlayerSoundManager>();
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
                        new Vector3(XStartOffset + XOffsetBetweenHearts * _heartPrefabs.Count, 0, 0), Quaternion.identity);
                    newPrefab.transform.SetParent(heartsUIParent.transform, worldPositionStays:false);
                    _heartPrefabs.Add(newPrefab);
                    _heartImages.Add(newPrefab.GetComponent<Image>());
                }
                UpdateUIHealth(GameData.Instance.curHealth);
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

                    if (_heartImages[i].sprite != sprite)
                    {
                        _heartImages[i].sprite = sprite;
                        _heartPrefabs[i].transform.DOScale(_heartScale*1.3f, 0.2f).OnComplete(() => _heartPrefabs[i].transform.DOScale(_heartScale, 0.2f));
                        break;
                    }
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
            if (GameData.Instance.curHealth <= 0)
            {
                EventManager.Instance.TriggerEvent(EventManager.PlayerDied, null);
                GameStatistics.Instance.UpdateStatistics((int)enemyType);
                yield break;
            }

            var pushForceMultiplier = EnemyData.GetPushForceMultiplier(enemyType);
            EventManager.Instance.TriggerEvent(EventManager.PlayerGotHit, (HitTime, hitDirection, pushForceMultiplier));
            EffectsManager.Instance.FloatingTextEffect(transform.position, 2, .5f, damage.ToString(), Constants.PlayerDamageColor, 1.5f);
            
            var hitMercyTime = enemyType == Enemy.EvilBall ? HitTime : HitMercyTime;
            yield return new WaitForSeconds(hitMercyTime);
            _canGetHit = true;
        }
        
        public void StartHeal()
        {
            _healCoroutine = StartCoroutine(HealCoroutine());
        }
        
        public void StopHeal()
        {
            if (_healCoroutine != null)
                StopCoroutine(_healCoroutine);
        }

        private IEnumerator HealCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(Constants.BaseSecondsPerHeal / playerData.RegenSpeedMultiplier);
                if (GameData.Instance.curHealth < playerData.MaxHealth)
                {
                    playerData.IncHealth();
                    SoundManager.Instance.Healed();
                }
            }
        }

        public void Reset()
        {
            foreach (var heart in _heartPrefabs)
            {
                Destroy(heart);
            }
            _heartPrefabs.Clear();
            _heartImages.Clear();
        }

        public void Init()
        {
            _init = true;
            if (_heartScale == 0)
            {
                _heartScale = heartUIPrefab.transform.localScale.x;
            }
            AddUIHealth(playerData.MaxHealth);
            UpdateUIHealth(GameData.Instance.curHealth);
            _canGetHit = true;
            _init = false;
        }
    }
}