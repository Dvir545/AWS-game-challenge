using System.Collections.Generic;
using Player;
using UnityEngine;
using UnityEngine.Pool;
using Utils;

namespace World
{
    public class HealingMatBehaviour : MonoBehaviour
    {
        [SerializeField] private GameObject healEffectPrefab;
        private Dictionary<GameObject, GameObject> _playerHealEffects = new Dictionary<GameObject, GameObject>();
        [SerializeField] private PlayerData playerData;
        
        void Start()
        {
            _playerHealEffects = new Dictionary<GameObject, GameObject>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
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
            _playerHealEffects.Clear();
        }

    }
}
