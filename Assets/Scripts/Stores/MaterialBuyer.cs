using Player;
using TMPro;
using Towers;
using UnityEngine;
using Utils;
using Utils.Data;

namespace Stores
{
    public class MaterialBuyer : MonoBehaviour
    {
        [SerializeField] private MaterialManager materialManager;
        [SerializeField] private PlayerData playerData;
        [SerializeField] private TowerMaterial material;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private TextMeshProUGUI amount;
        private int _amount = 10;
        
        private void Start()
        {
            amount.text = _amount.ToString();
            priceText.text = TowersData.Instance.GetTowerData(material).Price + " $";
        }

        public void BuyMaterial()
        {
            if (playerData.CurCash < TowersData.Instance.GetTowerData(material).Price)
            {
                Debug.Log("Not enough cash");
            } else if (_amount <= 0)
            {
                Debug.Log("No more materials");
            }
            else
            {
                playerData.SpendCash(TowersData.Instance.GetTowerData(material).Price);
                materialManager.AddMaterial(material);
                _amount--;
                amount.text = _amount.ToString();
            }
        }
    }
}
