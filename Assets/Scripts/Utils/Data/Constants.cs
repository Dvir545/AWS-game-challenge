using System.Collections.Generic;
using UnityEngine;
using Utils.Data;

namespace Utils
{
    public static class Constants
    {
        public const int NumTools = 3;
        public const int MaxUpgradeLevel = 3;
        public const int StartHealth = 6;
        public const float BaseSecondsPerHeal = 2;  // per heart
        public const float BaseAttackDuration = 2.5f; // seconds
        public const float BaseStaminaProgressIncPerSingleAttack = 0.12f;
        public const float BaseActCooldownDuration = 3;
        public const int BasePlayerDamage = 1;
        public const int BaseEnemyDamage = 1;
        public const int BaseKnockbackForce = 7;
        public const float TimeToEatCrop = 3f;
        public const int MaxTowerLevels = 3;
        public const int TowerCount = 9;
        public const int MaxCropsInStore = 99;
        public const int MaxMaterialsInStore = 99;
        public const int MaxCrops = 99;
        public const int MaxMaterials = 99;
        
        public const int StoreWheatAmountPerCycle = 5;
        public const int StoreCarrotAmountPerCycle = 5;
        public const int StoreTomatoAmountPerCycle = 5;
        public const int StoreCornAmountPerCycle = 3;
        public const int StorePumpkinAmountPerCycle = 1;

        public const int FirstDayDurationInSeconds = 180;
        public const int DaySecondsReductionPerCycle = 10;
        public const int MinDayDurationInSeconds = 40;
        public const float FirstNightDurationInSeconds = 10;
        public const float NightSecondsReductionPerCycle = .5f;
        public const float MinNightDurationInSeconds = 5;
        public const int MinEnemySpawnDistance = 3;
        public const int ChangeLightDurationInSeconds = 5;
        
        public const float DayLightIntensity = 1;
        public const float NightLightIntensity = 0.05f;


        public static readonly Color CashColor = new Color(1, 0.93f, 0, 1);
        public static readonly Color EnemyDamageColor = new Color(1, 1, 1, 1);
        public static readonly Color PlayerDamageColor = new Color(.7f, .25f, 0, 1);
        
        // game start values
        public const int StartHealthUpgradeLevel = 0;
        public const int StartRegenUpgradeLevel = 0;
        public const int StartSpeedUpgradeLevel = 0;
        public const int StartStaminaUpgradeLevel = 0;
        public const int StartKnockbackUpgradeLevel = 0;
        public const int StartSwordLevel = 0;
        public const int StartHoeLevel = 0;
        public const int StartHammerLevel = 0;
        public const int StartCash = 0;
        public const int StartDay = 0;
        public static readonly int[] StartCrops = {3, 0, 0, 0, 0};
        public static readonly int[] StartMaterials = {1, 0, 0, 0, 0};
        public static readonly int[] StartCropsInStore = {5, 0, 0, 0, 0};
        public static readonly int[] StartMaterialsInStore = {3, 0, 0, 0, 0};
        public static readonly int[] FirstDayEnemies = { 0, 0, 0, 0, 0, 0 };
        public static readonly int[] FirstNightEnemies = { 5, 0, 0, 0, 0, 0 };
        public static readonly List<TowerLevelInfo>[] StartTowers = new List<TowerLevelInfo>[]
        {
            new List<TowerLevelInfo>(), new List<TowerLevelInfo>(), new List<TowerLevelInfo>(), 
            new List<TowerLevelInfo>(), new List<TowerLevelInfo>(), new List<TowerLevelInfo>(), 
            new List<TowerLevelInfo>(), new List<TowerLevelInfo>(), new List<TowerLevelInfo>(), 
        };

    }
}