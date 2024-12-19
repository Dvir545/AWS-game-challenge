using Crops;
using Player;
using TMPro;
using UnityEngine;
using Utils;
using Utils.Data;

namespace Stores
{
    public class CropBuyer : MonoBehaviour
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

        private void Start()
        {
            amount.text = Amount.ToString();
            priceText.text = CropsData.Instance.GetPrice(crop) + " $";
            EventManager.Instance.StartListening(EventManager.DayStarted, UpdateAmountText);
        }

        private void UpdateAmountText(object arg0)
        {
            amount.text = Amount.ToString();
        }

        public void BuyCrop()
        {
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
            }
        }
    }
}
