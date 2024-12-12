using UnityEngine;

namespace Utils.Data
{
    readonly struct Sword
    {
        private readonly int _damageMultiplier;
        private readonly int _price;
        private readonly Color _color;
        
        public  Sword(int damageMultiplier, int price, Color color)
        {
            _damageMultiplier = damageMultiplier;
            _price = price;
            _color = color;
        }
        
        public int GetDamageMultiplier() => _damageMultiplier;
        public int GetPrice() => _price;
        public Color GetColor() => _color;
    }

    readonly struct Hoe
    {
        private readonly float _progressSpeedMultiplier;
        private readonly int _price;
        private readonly Color _color;
        
        public Hoe(float progressSpeedMultiplier, int price, Color color)
        {
            _progressSpeedMultiplier = progressSpeedMultiplier;
            _price = price;
            _color = color;
        }
        
        public float GetProgressSpeedMultiplier() => _progressSpeedMultiplier;
        public int GetPrice() => _price;
        public Color GetColor() => _color;
    }

    readonly struct Hammer
    {
        private readonly float _progressSpeedMultiplier;
        private readonly int _price;
        private readonly Color _color;

        public Hammer(float progressSpeedMultiplier, int price, Color color)
        {
            _progressSpeedMultiplier = progressSpeedMultiplier;
            _price = price;
            _color = color;
        }

        public float GetProgressMultiplier() => _progressSpeedMultiplier;
        public int GetPrice() => _price;
        public Color GetColor() => _color;
    }

    public static class ToolsData
    {
        private static Sword[] _swords =
        {
            new(1, 0, new Color(1, 1, 1)),
            new(2, 100, new Color(0.52f, 1f, 0.48f)),
            new(3, 500, new Color(0.49f, 0.87f, 1f)),
            new(5, 2000, new Color(0.9f, 0.49f, 1f))
        };
        private static Hoe[] _hoes =
        {
            new(1, 0, new Color(1, 1, 1)),
            new(2f, 100, new Color(0.52f, 1f, 0.48f)),
            new(3f, 500, new Color(0.49f, 0.87f, 1f)),
            new(4f, 2000, new Color(0.9f, 0.49f, 1f))
        };
        private static Hammer[] _hammers =
        {
            new(1, 0, new Color(1, 1, 1)),
            new(2f, 100, new Color(0.52f, 1f, 0.48f)),
            new(3f, 500, new Color(0.49f, 0.87f, 1f)),
            new(4f, 2000, new Color(0.9f, 0.49f, 1f))
        };
        
        public static float GetProgressSpeedMultiplier(int index, HeldTool tool)
        {
            switch (tool)
            {
                case HeldTool.Hoe:
                    return _hoes[index].GetProgressSpeedMultiplier();
                case HeldTool.Hammer:
                    return _hammers[index].GetProgressMultiplier();
            }

            return 0;
        }
        
        public static int GetDamageMultiplier(int index, HeldTool tool)
        {
            switch (tool)
            {
                case HeldTool.Sword:
                    return _swords[index].GetDamageMultiplier();
            }
            return 0;
        }
        
        public static int GetPrice(HeldTool tool, int level)
        {
            switch (tool)
            {
                case HeldTool.Sword:
                    return _swords[level].GetPrice();
                case HeldTool.Hoe:
                    return _hoes[level].GetPrice();
                case HeldTool.Hammer:
                    return _hammers[level].GetPrice();
            }

            return 0;
        }
        
        public static Color GetColor(HeldTool tool, int level)
        {
            switch (tool)
            {
                case HeldTool.Sword:
                    return _swords[level].GetColor();
                case HeldTool.Hoe:
                    return _hoes[level].GetColor();
                case HeldTool.Hammer:
                    return _hammers[level].GetColor();
            }

            return Color.white;
        }
    }
}