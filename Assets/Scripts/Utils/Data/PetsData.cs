using UnityEngine;

namespace Utils.Data
{
    readonly struct Slime
    {
        private readonly int _price;
        private readonly Color _color;

        public Slime(int price, Color color)
        {
            _price = price;
            _color = color;
        }
        
        public int GetPrice() => _price;
        public Color GetColor() => _color;
    }

    public static class PetsData
    {
        private static Slime[] _slimes =
        {
            new(9999, new Color(1, 1, 1)),
            new(9999, new Color(1f, 0.47f, 0.47f)),
            new(9999, new Color(1f, 1f, 0f)),
            new(9999, new Color(0.32f, 1f, 0.8f)),
            new(9999, new Color(0.12f, 1f, 0.15f))
        };

        public static int GetCount()
        {
            return _slimes.Length;
        }
        
        
        public static int GetPrice(Pet pet, int index)
        {
            switch (pet)
            {
                case Pet.Slime:
                    return _slimes[index].GetPrice();
                default:
                    return 0;
            }
        }
        
        public static Color GetColor(Pet pet, int index)
        {
            switch (pet)
            {
                case Pet.Slime:
                    return _slimes[index].GetColor();
                default:
                    return Color.white;
            }
        }
    }
}