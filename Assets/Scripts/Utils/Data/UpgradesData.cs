using TMPro;
using UnityEngine;

namespace Utils.Data
{
    readonly struct Health
    {
        private readonly int _price;
        
        public Health(int price)
        {
            _price = price;
        }
        public int GetPrice() => _price;
    }
    
    readonly struct Regen
    {
        private readonly float _regenSpeedMultiplier;
        private readonly int _price;
        private readonly Color _color;

        public Regen(float regenSpeedMultiplier, int price, Color color)
        {
            _regenSpeedMultiplier = regenSpeedMultiplier;
            _price = price;
            _color = color;
        }
        
        public float GetRegenSpeedMultiplier() => _regenSpeedMultiplier;
        public int GetPrice() => _price;

        public Color GetColor() => _color;
    }

    readonly struct Speed
    {
        private readonly float _speedMultiplier;
        private readonly int _price;

        public Speed(float speedMultiplier, int price)
        {
            _speedMultiplier = speedMultiplier;
            _price = price;
        }

        public float GetSpeedMultiplier() => _speedMultiplier;
        public int GetPrice() => _price;
    }
    
    readonly struct Stamina
    {
        private readonly float _staminaMultiplier;
        private readonly int _price;

        public Stamina(float staminaMultiplier, int price)
        {
            _staminaMultiplier = staminaMultiplier;
            _price = price;
        }

        public float GetStaminaMultiplier() => _staminaMultiplier;
        public int GetPrice() => _price;
    }

    public static class UpgradesData
    {
        private static Health[] _healths =
        {
            new(0),
            new(200),
            new(1000),
            new(3000)
            
        };
        private static Regen[] _regens =
        {
            new(1, 0, new Color(0.98f, 0.76f, 0.51f)),
            new(1.5f, 200, new Color(0f, 1f, 0f)),
            new(2f, 1000, new Color(1f, 0f, 0f)),
            new(3f, 3000, new Color(1f, 0f, 0.72f))
        };
        private static Speed[] _speeds =
        {
            new(1, 0),
            new(1.1f, 200),
            new(1.3f, 1000),
            new(1.5f, 3000)
        };

        private static Stamina[] _staminas =
        {
            new(1, 0),
            new(1.5f, 200),
            new(2f, 1000),
            new(3f, 1000)
        };
        
        public static float GetRegenSpeedMultiplier(int index)
        {
            return _regens[index].GetRegenSpeedMultiplier();
        }
        public static float GetSpeedMultiplier(int index)
        {
            return _speeds[index].GetSpeedMultiplier();
        }
        
        public static float GetStaminaMultiplier(int index)
        {
            return _staminas[index].GetStaminaMultiplier();
        }
        public static int GetPrice(Upgrade upgradeType, int level)
        {
            switch (upgradeType)
            {
                case Upgrade.Health:
                    return _healths[level].GetPrice();
                case Upgrade.Regen:
                    return _regens[level].GetPrice();
                case Upgrade.Speed:
                    return _speeds[level].GetPrice();
                case Upgrade.Stamina:
                    return _staminas[level].GetPrice();
            }

            return 0;
        }

        public static Color GetColor(Upgrade upgradeType, int level)
        {
            switch (upgradeType)
            {
                case Upgrade.Regen:
                    return _regens[level].GetColor();
            }

            return Color.white;
        }
    }
}