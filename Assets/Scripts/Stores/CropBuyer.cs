using Crops;
using Player;
using TMPro;
using UnityEngine;
using Utils;
using Utils.Data;
using World;

namespace Stores
{
    public class CropBuyer : MonoBehaviour, IBuyable
    {
        [SerializeField] private CropManager cropManager;
        [SerializeField] private PlayerData playerData;
        [SerializeField] private Crop crop;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private TextMeshProUGUI amount;
        private int Amount
        {
            get => GameData.Instance.cropsInStore[(int)crop];
            set => GameData.Instance.cropsInStore[(int)crop] = value;
        }

        public void Init()
        {
            amount.text = Amount.ToString();
            priceText.text = CropsData.Instance.GetPrice(crop) + " $";
        }

        public void UpdateAmountText(int newAmount)
        {
            amount.text = newAmount.ToString();
        }

        public void BuyCrop()
        {
            bool bought = false;
            if (GameData.Instance.cash < CropsData.Instance.GetPrice(crop))
            {
                Debug.Log("Not enough cash");
            } else if (Amount <= 0)
            {
                Debug.Log("No more crops");
            }
            else if (Amount >= Constants.MaxCrops)
            {
                Debug.Log("Too many crops");
            }
            else
            {
                playerData.SpendCash(CropsData.Instance.GetPrice(crop));
                cropManager.AddCrop(crop);
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
            BuyCrop();
        }
    }
}
