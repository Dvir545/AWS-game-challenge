namespace Utils.Data
{
    readonly struct Sword
    {
        private readonly int _damageMultiplier;
        private readonly int _price;
        
        public  Sword(int damageMultiplier, int price)
        {
            _damageMultiplier = damageMultiplier;
            _price = price;
        }
        
        public int GetDamageMultiplier() => _damageMultiplier;
        public int GetPrice() => _price;
    }

    readonly struct Hoe
    {
        private readonly float _progressSpeedMultiplier;
        private readonly int _price;
        
        public Hoe(float progressSpeedMultiplier, int price)
        {
            _progressSpeedMultiplier = progressSpeedMultiplier;
            _price = price;
        }
        
        public float GetProgressSpeedMultiplier() => _progressSpeedMultiplier;
        public int GetPrice() => _price;
    }

    readonly struct Hammer
    {
        private readonly float _progressSpeedMultiplier;
        private readonly int _price;

        public Hammer(float progressSpeedMultiplier, int price)
        {
            _progressSpeedMultiplier = progressSpeedMultiplier;
            _price = price;
        }

        public float GetProgressMultiplier() => _progressSpeedMultiplier;
        public int GetPrice() => _price;
    }

    public static class ToolsData
    {
        private static Sword[] _swords =
        {
            new(1, 0),
            new(2, 100),
            new(3, 500),
            new(5, 2000)
            
        };
        private static Hoe[] _hoes =
        {
            new(1, 0),
            new(2f, 100),
            new(3f, 500),
            new(4f, 2000)
        };
        private static Hammer[] _hammers =
        {
            new(1, 0),
            new(2f, 100),
            new(3f, 500),
            new(4f, 2000)
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
    }
}