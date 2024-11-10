namespace Utils
{
    struct Sword
    {
        private  float _animationSpeedMultiplier;
        private float _damageMultiplier;
        
        public  Sword(float animationSpeedMultiplier, float damageMultiplier)
        {
            _animationSpeedMultiplier = animationSpeedMultiplier;
            _damageMultiplier = damageMultiplier;
        }
        
        public float GetAnimationSpeedMultiplier() => _animationSpeedMultiplier;
        public float GetDamageMultiplier() => _damageMultiplier;
    }
    
    struct Hoe
    {
        private float _animationSpeedMultiplier;
        private float _progressSpeedMultiplier;
        
        public Hoe(float animationSpeedMultiplier, float progressSpeedMultiplier)
        {
            _animationSpeedMultiplier = animationSpeedMultiplier;
            _progressSpeedMultiplier = progressSpeedMultiplier;
        }
        
        public float GetAnimationSpeedMultiplier() => _animationSpeedMultiplier;
        public float GetProgressSpeedMultiplier() => _progressSpeedMultiplier;
    }

    struct Hammer
    {
        private float _animationSpeedMultiplier;
        private float _progressSpeedMultiplier;

        public Hammer(float animationSpeedMultiplier, float damageMultiplier)
        {
            _animationSpeedMultiplier = animationSpeedMultiplier;
            _progressSpeedMultiplier = damageMultiplier;
        }

        public float GetAnimationSpeedMultiplier() => _animationSpeedMultiplier;
        public float GetProgressMultiplier() => _progressSpeedMultiplier;
    }

    public static class Tools
    {
        private static Sword[] _swords =
        {
            new(1f, 1f), // normal sword
        };
        private static Hoe[] _hoes =
        {
            new(1f, 1f)  // normal hoe
        };
        private static Hammer[] _hammers =
        {
            new(1f, 1f)  // normal hammer
        };
        
        public static float GetAnimationSpeedMultiplier(int index, HeldTool tool)
        {
            switch (tool)
            {
                case HeldTool.Sword:
                    return _swords[index].GetAnimationSpeedMultiplier();
                case HeldTool.Hoe:
                    return _hoes[index].GetAnimationSpeedMultiplier();
                case HeldTool.Hammer:
                    return _hammers[index].GetAnimationSpeedMultiplier();
            }
            return 1;
        }
        
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
        
        public static float GetDamageMultiplier(int index, HeldTool tool)
        {
            switch (tool)
            {
                case HeldTool.Sword:
                    return _swords[index].GetDamageMultiplier();
            }

            return 0;
        }
    }
}