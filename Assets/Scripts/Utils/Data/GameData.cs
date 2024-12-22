using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Serialization.Json;
using UnityEngine;

namespace Utils.Data
{
    [Serializable]
    public class TowerLevelInfo
    {
        public int material;
        public float progress;
        public float health;
        
        public TowerLevelInfo(int material, float progress, float health)
        {
            this.material = material;
            this.progress = progress;
            this.health = health;
        }
        public TowerLevelInfo() { }
    }
    
    [Serializable]
    public class PlantedCropInfo
    {
        public int cropType;
        public float growthProgress;
        public float destroyProgress;
        
        public PlantedCropInfo(int cropType, float growth, float destroyProgress)
        {
            this.cropType = cropType;
            this.growthProgress = growth;
            this.destroyProgress = destroyProgress;
        }
        public PlantedCropInfo() { }
    }
    
    class SerializableGameData
    {
        public int HealthUpgradeLevel;
        public int RegenUpgradeLevel;
        public int SpeedUpgradeLevel;
        public int StaminaUpgradeLevel;
        public int KnockbackUpgradeLevel;
        public int SwordLevel;
        public int HoeLevel;
        public int HammerLevel;
        public int Cash;
        public int Day;
        public int CurHealth;
        public float SecondsSinceGameStarted;
        public int[] InventoryCrops;
        public int[] InventoryMaterials;
        public int[] CropsInStore;
        public int[] MaterialsInStore;
        public int[] ThisDayEnemies;
        public int[] ThisNightEnemies;
        public List<TowerLevelInfo>[] Towers;
        public Dictionary<Vector2Int, PlantedCropInfo> PlantedCrops;
    }
    
    public class GameData: Singleton<GameData>
    {
        public int healthUpgradeLevel;
        public int regenUpgradeLevel;
        public int speedUpgradeLevel;
        public int staminaUpgradeLevel;
        public int knockbackUpgradeLevel;
        public int swordLevel;
        public int hoeLevel;
        public int hammerLevel;
        public int cash;
        public int day;
        public int curHealth;
        public float secondsSinceGameStarted;
        public int[] crops;
        public int[] materials;
        public int[] cropsInStore;
        public int[] materialsInStore;
        public int[] thisDayEnemies;
        public int[] thisNightEnemies;
        public List<TowerLevelInfo>[] towers;
        public Dictionary<Vector2Int, PlantedCropInfo> plantedCrops;

        public GameData()
        {
            NewGame();
        }

        public void NewGame()
        {
            healthUpgradeLevel = Constants.StartHealthUpgradeLevel;
            regenUpgradeLevel = Constants.StartRegenUpgradeLevel;
            speedUpgradeLevel = Constants.StartSpeedUpgradeLevel;
            staminaUpgradeLevel = Constants.StartStaminaUpgradeLevel;
            knockbackUpgradeLevel = Constants.StartKnockbackUpgradeLevel;
            swordLevel = Constants.StartSwordLevel;
            hoeLevel = Constants.StartHoeLevel;
            hammerLevel = Constants.StartHammerLevel;
            cash = Constants.StartCash;
            day = Constants.StartDay;
            curHealth = Constants.StartHealth;
            secondsSinceGameStarted = 0;
            crops = Constants.StartCrops.ToArray();
            materials = Constants.StartMaterials.ToArray();
            cropsInStore = Constants.StartCropsInStore.ToArray();
            materialsInStore = Constants.StartMaterialsInStore.ToArray();
            thisDayEnemies = Constants.FirstDayEnemies.ToArray();
            thisNightEnemies = Constants.FirstNightEnemies.ToArray();
            towers = Constants.StartTowers.Select(towerList => new List<TowerLevelInfo>(towerList)).ToArray();
            plantedCrops = new Dictionary<Vector2Int, PlantedCropInfo>();
        }
        
        public void LoadFromJson(string json)
        {
            var data = JsonSerialization.FromJson<SerializableGameData>(json);
            healthUpgradeLevel = data.HealthUpgradeLevel;
            regenUpgradeLevel = data.RegenUpgradeLevel;
            speedUpgradeLevel = data.SpeedUpgradeLevel;
            staminaUpgradeLevel = data.StaminaUpgradeLevel;
            swordLevel = data.SwordLevel;
            hoeLevel = data.HoeLevel;
            hammerLevel = data.HammerLevel;
            cash = data.Cash;
            day = data.Day;
            curHealth = data.CurHealth;
            secondsSinceGameStarted = data.SecondsSinceGameStarted;
            crops = data.InventoryCrops.ToArray();
            materials = data.InventoryMaterials.ToArray();
            cropsInStore = data.CropsInStore.ToArray();
            materialsInStore = data.MaterialsInStore.ToArray();
            thisDayEnemies = data.ThisDayEnemies.ToArray();
            thisNightEnemies = data.ThisNightEnemies.ToArray();
            towers = data.Towers.Select(towerList => new List<TowerLevelInfo>(towerList)).ToArray();
            plantedCrops = data.PlantedCrops;
        }
        
        public void SaveToJson()
        {
            var serializableData = new SerializableGameData
            {
                HealthUpgradeLevel = healthUpgradeLevel,
                RegenUpgradeLevel = regenUpgradeLevel,
                SpeedUpgradeLevel = speedUpgradeLevel,
                StaminaUpgradeLevel = staminaUpgradeLevel,
                SwordLevel = swordLevel,
                HoeLevel = hoeLevel,
                HammerLevel = hammerLevel,
                Cash = cash,
                Day = day,
                CurHealth = curHealth,
                SecondsSinceGameStarted = secondsSinceGameStarted,
                InventoryCrops = crops,
                InventoryMaterials = materials,
                CropsInStore = cropsInStore,
                MaterialsInStore = materialsInStore,
                ThisDayEnemies = thisDayEnemies,
                ThisNightEnemies = thisNightEnemies,
                Towers = towers,
                PlantedCrops = plantedCrops
            };
            var json = JsonSerialization.ToJson(serializableData);
            Debug.Log(json);
            // DVIR - upload json to aws
        }
    }
}