using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utils.Data
{
    struct CropData
    {
        private int _price;
        private int _sellPrice;
        private int _growthTime;

        public CropData(int price, int sellPrice, int growthTime)
        {
            _price = price;
            _sellPrice = sellPrice;
            _growthTime = growthTime;
        }

        public int GetPrice()
        {
            return _price;
        }

        public int GetSellPrice()
        {
            return (int)(_sellPrice * UnityEngine.Random.Range(0.8f, 1.2f));  // for fun
        }

        public int GetGrowthTime()
        {
            return _growthTime;
        }

        public float GetProfitPerSecond()
        {
            return (_sellPrice - _price) / _growthTime;
        }
    }

    public class CropsData : Singleton<CropsData>
    {
        private Dictionary<Crop, CropData> _cropsData = new () {
            {Crop.Wheat, new CropData(1, 4, 3)},  // 0.4$ profit per second
            {Crop.Carrot, new CropData(5, 20, 5)},  // 1$ profit per second
            {Crop.Tomato, new CropData(30, 51, 3)},  // 3$ profit per second
            {Crop.Corn, new CropData(100, 178, 6)},  // 5$ profit per second
            { Crop.Pumpkin, new CropData(500, 700, 10)}  // 10$ profit per second
        };
        
        public int GetPrice(Crop crop)
        {
            return _cropsData[crop].GetPrice();
        }
        
        public int GetSellPrice(Crop crop)
        {
            return _cropsData[crop].GetSellPrice();
        }
        
        public int GetGrowthTime(Crop crop)
        {
            return _cropsData[crop].GetGrowthTime();
        }
    }
}