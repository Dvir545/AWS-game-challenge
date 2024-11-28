using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utils.Data
{
    struct CropData
    {
        public Crop Crop { get; private set; }
        public int Price { get; private set; }
        public int GrowthTime { get; private set; }
        private int _sellPrice;

        public CropData(Crop crop, int price, int sellPrice, int growthTime)
        {
            Crop = crop;
            Price = price;
            _sellPrice = sellPrice;
            GrowthTime = growthTime;
        }

        public int GetSellPrice()
        {
            return (int)(_sellPrice * UnityEngine.Random.Range(0.8f, 1.2f));  // for fun
        }
        
        public Sprite GetSprite(float growth) // growth is between 0 and 1
        {
            return SpriteData.Instance.GetCropSprite(Crop, growth);
        }
    }

    public class CropsData : Singleton<CropsData>
    {

        private CropData[] _cropsData = {
            new CropData(Crop.Wheat, 1, 4, 3),  // 0.4$ profit per second
            new CropData(Crop.Carrot, 5, 20, 5),  // 1$ profit per second
            new CropData(Crop.Tomato, 30, 51, 3),  // 3$ profit per second
            new CropData(Crop.Corn, 100, 178, 6),  // 5$ profit per second
            new CropData(Crop.Pumpkin, 500, 700, 10)  // 10$ profit per second
        };
        private Dictionary<Crop, CropData> _cropsDataDict = new Dictionary<Crop, CropData>();
        protected void Awake()
        {
            foreach (CropData cropData in _cropsData)
            {
                _cropsDataDict.Add(cropData.Crop, cropData);
            }
        }
        
        public int GetPrice(Crop crop)
        {
            return _cropsDataDict[crop].Price;
        }
        
        public int GetSellPrice(Crop crop)
        {
            return _cropsDataDict[crop].GetSellPrice();
        }
        
        public int GetGrowthTime(Crop crop)
        {
            return _cropsDataDict[crop].GrowthTime;
        }

        public Sprite GetSprite(Crop crop, float growth)
        {
            return _cropsDataDict[crop].GetSprite(growth);
        }
    }
}