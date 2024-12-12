using UnityEngine;

namespace Utils
{
    public static class Constants
    {
        public const int NumTools = 3;
        public const int MaxUpgradeLevel = 3;
        public const int StartHealth = 6;
        public const int StartingCash = 100;
        public const float BaseSecondsPerHeal = 2;  // per heart
        public const int BasePlayerDamage = 1;
        public const int BaseEnemyDamage = 1;
        public const int KnockbackForce = 7;
        public const float TimeToEatCrop = 3f;
        public const int MaxTowerLevels = 3;

        public const string StartNPCAPIURL = "startnpc";
        public const string MidNPCAPIURL = "storenpc";
            

        public static readonly Color CashColor = new Color(1, 0.93f, 0, 1);
        public static readonly Color EnemyDamageColor = new Color(1, 1, 1, 1);
        public static readonly Color PlayerDamageColor = new Color(.7f, .25f, 0, 1);
    }
}