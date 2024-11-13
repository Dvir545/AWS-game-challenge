using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Utils;
using Vector3 = UnityEngine.Vector3;

namespace Player
{
    public class HealthManager : MonoBehaviour
    {
        [SerializeField] private GameObject heartUIPrefab;
        [SerializeField] private GameObject heartsUIParent;
    
        [SerializeField] private Sprite fullHeartSprite;
        [SerializeField] private Sprite halfHeartSprite;
        [SerializeField] private Sprite emptyHeartSprite;

        [SerializeField] private PlayerData playerData;

        private List<GameObject> _heartPrefabs = new();
        private List<Image> _heartImages = new();
        private const int XOffsetBetweenHearts = 100;

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
    }
}
