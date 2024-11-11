using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utils.Data
{
    struct CropData
    {
        private int _price;
        private float _profit_multiplier;
        private int _growthTime;

        public CropData(int price, float profit_multiplier, int growthTime)
        {
            _price = price;
            _profit_multiplier = profit_multiplier;
            _growthTime = growthTime;
        }

        public int GetPrice()
        {
            return _price;
        }

        public int GetSellPrice()
        {
            float sellPrice = _price * _profit_multiplier;
            sellPrice *= UnityEngine.Random.Range(0.8f, 1.2f); // for fun
            return (int)sellPrice;
        }

        public int GetGrowthTime()
        {
            return _growthTime;
        }

        public float GetProfitPerSecond()
        {
            return (_price * _profit_multiplier - _price) / _growthTime;
        }
    }

    public class CropsData : Singleton<CropsData>
    {
        private Dictionary<Crop, CropData> _cropsData = new () {
            {Crop.Wheat, new CropData(1, 3f, 5)},  // 0.4$ profit per second
            {Crop.Carrot, new CropData(3, 4.33f, 10)},  // 1$ profit per second
            {Crop.Tomato, new CropData(10, 2.5f, 5)},  // 3$ profit per second
            {Crop.Corn, new CropData(50, 2.5f, 15)},  // 5$ profit per second
            { Crop.Pumpkin, new CropData(200, 2f, 20)}  // 10$ profit per second
        };
    }
}