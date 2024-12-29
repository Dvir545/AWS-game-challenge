using Player;
using TMPro;
using Towers;
using UnityEngine;
using Utils;
using Utils.Data;
using World;

namespace Stores
{
    public class MaterialBuyer : MonoBehaviour, IBuyable
    {
        [SerializeField] private int materialNumber;
        [SerializeField] private MaterialManager materialManager;
        [SerializeField] private PlayerData playerData;
        [SerializeField] private TowerMaterial material;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private TextMeshProUGUI amount;
        private int Amount
        {
            get => GameData.Instance.materialsInStore[(int)material];
            set => GameData.Instance.materialsInStore[(int)material] = value;
        }

        private void Start()
        {
            amount.text = Amount.ToString();
            priceText.text = TowersData.Instance.GetTowerData(material).Price + " $";
        }
        
        public void UpdateAmountText(int newAmount)
        {
            amount.text = newAmount.ToString();
        }

        public void BuyMaterial()
        {
            bool bought = false;
            if (GameData.Instance.cash < TowersData.Instance.GetTowerData(material).Price)
            {
                Debug.Log("Not enough cash");
            } else if (Amount <= 0)
            {
                Debug.Log("No more materials");
            }
            else if (Amount >= Constants.MaxMaterials)
            {
                Debug.Log("Too many materials");
            }
            else
            {
                playerData.SpendCash(TowersData.Instance.GetTowerData(material).Price);
                materialManager.AddMaterial(material);
                Amount--;
                amount.text = Amount.ToString();
                bought = true;
            }
            if (!bought)
            {
                SoundManager.Instance.CantPurchase();
            }
        }
        
        public void BuyItem()
        {
            BuyMaterial();
        }
        
        public int GetItemNumber()
        {
            return materialNumber;
        }
    }
}
