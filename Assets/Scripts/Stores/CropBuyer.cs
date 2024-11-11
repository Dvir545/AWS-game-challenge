using System;
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
        private int _amount = 10;
        
        private void Start()
        {
            amount.text = _amount.ToString();
            priceText.text = CropsData.Instance.GetPrice(crop) + " $";
        }

        public void BuyCrop()
        {
            if (playerData.GetCurCash < CropsData.Instance.GetPrice(crop))
            {
                Debug.Log("Not enough cash");
            } else if (_amount <= 0)
            {
                Debug.Log("No more crops");
            }
            else
            {
                playerData.SpendCash(CropsData.Instance.GetPrice(crop));
                cropManager.AddCrop(crop);
                _amount--;
                amount.text = _amount.ToString();
            }
        }
    }
}
