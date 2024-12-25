using System;
using System.Collections.Generic;
using Player;
using UnityEngine;
using UnityEngine.Pool;
using Utils;
using Utils.Data;

namespace World
{
    public class HealingMatBehaviour : MonoBehaviour
    {
        [SerializeField] private GameObject healEffectPrefab;
        private Dictionary<GameObject, GameObject> _playerHealEffects = new Dictionary<GameObject, GameObject>();
        [SerializeField] private PlayerData playerData;
        private Collider2D _collider;
        
        public void Init()
        {
            if (_collider == null)
            {
                _playerHealEffects = new Dictionary<GameObject, GameObject>();
                _collider = GetComponent<Collider2D>();
            }
            _collider.enabled = true;
            EventManager.Instance.StartListening(EventManager.NightStarted, StopHeal);
            EventManager.Instance.StartListening(EventManager.NightEnded, StartHeal);
        }

        private void StartHeal(object arg0)
        {
            _collider.enabled = true;
        }

        private void StopHeal(object arg0)
        {
            foreach (var healEffect in _playerHealEffects.Values)
            {
                if (healEffect != null)
                {
                    Destroy(healEffect);
                }
            }
            foreach (var player in _playerHealEffects.Keys)
            {
                player.GetComponent<PlayerHealthManager>().StopHeal();
            }
            _playerHealEffects.Clear();   
            _collider.enabled = false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // if (DayNightManager.Instance.NightTime) return;
            if (other.CompareTag("Player") && !_playerHealEffects.ContainsKey(other.gameObject))
            {
                var healEffect = Instantiate(healEffectPrefab, other.transform.position, Quaternion.identity);
                healEffect.SetActive(true);
                healEffect.transform.SetParent(other.transform);
                _playerHealEffects.Add(other.gameObject, healEffect);
                other.gameObject.GetComponent<PlayerHealthManager>().StartHeal();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                if (_playerHealEffects.TryGetValue(other.gameObject, out GameObject healEffect))
                {
                    Destroy(healEffect);
                    _playerHealEffects.Remove(other.gameObject);
                }
                other.gameObject.GetComponent<PlayerHealthManager>().StopHeal();
            }
        }

        private void OnDestroy()
        {
            // Clean up any remaining heal effects when the healing mat is destroyed
            foreach (var healEffect in _playerHealEffects.Values)
            {
                if (healEffect != null)
                {
                    Destroy(healEffect);
                }
            }
            foreach (var player in _playerHealEffects.Keys)
            {
                player.GetComponent<PlayerHealthManager>().StopHeal();
            }
            _playerHealEffects.Clear();
        }
    }
}
