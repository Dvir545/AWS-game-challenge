using System.Collections.Generic;
using UnityEngine;

namespace Utils.Data
{
    struct CropData
    {
        public Crop Crop { get; private set; }
        public int Price { get; private set; }
        public int PlantTime { get; private set; }
        public float DestroyTime { get; private set; }
        private int _sellPrice;

        public CropData(Crop crop, int price, int sellPrice, int plantTime, float destroyTime)
        {
            Crop = crop;
            Price = price;
            _sellPrice = sellPrice;
            PlantTime = plantTime;
            DestroyTime = destroyTime;
        }

        public int GetSellPrice()
        {
            return (int)(_sellPrice * Random.Range(0.8f, 1.2f));  // for fun
        }
    }

    public class CropsData : Singleton<CropsData>
    {

        private CropData[] _cropsData = {
            new CropData(Crop.Wheat, 1, 4, 3, 3f),  // 0.4$ profit per second
            new CropData(Crop.Carrot, 5, 20, 5, 4f),  // 1$ profit per second
            new CropData(Crop.Tomato, 30, 51, 3, 5f),  // 3$ profit per second
            new CropData(Crop.Corn, 100, 178, 6, 6f),  // 5$ profit per second
            new CropData(Crop.Pumpkin, 500, 700, 10, 7f)  // 10$ profit per second
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
        
        public int GetPlantTime(Crop crop)
        {
            return _cropsDataDict[crop].PlantTime;
        }
        
        public float GetDestroyTime(Crop crop)
        {
            return _cropsDataDict[crop].DestroyTime;
        }
    }
}