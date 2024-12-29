using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Serialization.Json;
using UnityEngine;
using Utils;

namespace Utils.Data
{
    [Serializable]
    public class Vector2IntSerializable
    {
        public int x;
        public int y;

        public Vector2IntSerializable(Vector2Int vector)
        {
            x = vector.x;
            y = vector.y;
        }

        public Vector2Int ToVector2Int()
        {
            return new Vector2Int(x, y);
        }
    }

    [Serializable]
    public class PlantedCropsData
    {
        public int cropType;
        public float growthProgress;
        public float destroyProgress;
    }

    [Serializable]
    public class PlantedCropKeyValue
    {
        public Vector2IntSerializable Key;
        public PlantedCropInfo Value;
    }

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
    
    [Serializable]
    public class PetInfo
    {
        public int petType;
        public int petIndex;
        
        public PetInfo(int petType, int petIndex)
        {
            this.petType = petType;
            this.petIndex = petIndex;
        }
        public PetInfo() { }
    }
    
    [Serializable]
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
        public List<PetInfo> Pets;
    }
    
    public class GameData : Singleton<GameData>
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
        public List<PetInfo> pets;

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
            pets = new List<PetInfo>();
        }
        
        public void LoadFromGameState(GameState state)
        {
            if (state == null)
            {
                Debug.LogError("Attempted to load null game state!");
                return;
            }

            try
            {
                healthUpgradeLevel = state.healthUpgradeLevel;
                regenUpgradeLevel = state.regenUpgradeLevel;
                speedUpgradeLevel = state.speedUpgradeLevel;
                staminaUpgradeLevel = state.staminaUpgradeLevel;
                knockbackUpgradeLevel = state.knockbackUpgradeLevel;
                swordLevel = state.swordLevel;
                hoeLevel = state.hoeLevel;
                hammerLevel = state.hammerLevel;
                cash = state.cash;
                day = state.day;
                curHealth = state.curHealth;
                secondsSinceGameStarted = state.secondsSinceGameStarted;
                
                // Copy arrays with null checks
                crops = state.crops?.ToArray() ?? Constants.StartCrops.ToArray();
                materials = state.materials?.ToArray() ?? Constants.StartMaterials.ToArray();
                cropsInStore = state.cropsInStore?.ToArray() ?? Constants.StartCropsInStore.ToArray();
                materialsInStore = state.materialsInStore?.ToArray() ?? Constants.StartMaterialsInStore.ToArray();
                thisDayEnemies = state.thisDayEnemies?.ToArray() ?? Constants.FirstDayEnemies.ToArray();
                thisNightEnemies = state.thisNightEnemies?.ToArray() ?? Constants.FirstNightEnemies.ToArray();

                // Initialize towers array
                towers = new List<TowerLevelInfo>[Constants.TowerCount];
                for (int i = 0; i < Constants.TowerCount; i++)
                {
                    towers[i] = new List<TowerLevelInfo>();
                    if (state.towers != null && i < state.towers.Length && state.towers[i] != null)
                    {
                        foreach (var towerInfo in state.towers[i].towers)
                        {
                            if (towerInfo != null)
                            {
                                towers[i].Add(new TowerLevelInfo(
                                    towerInfo.material,
                                    towerInfo.progress,
                                    towerInfo.health
                                ));
                            }
                        }
                    }
                }

                // Initialize and populate planted crops
                plantedCrops = new Dictionary<Vector2Int, PlantedCropInfo>();
                if (state.plantedCrops != null)
                {
                    foreach (var cropKeyValue in state.plantedCrops)
                    {
                        var position = cropKeyValue.Key.ToVector2Int();
                        plantedCrops[position] = new PlantedCropInfo(
                            cropKeyValue.Value.cropType,
                            cropKeyValue.Value.growthProgress,
                            cropKeyValue.Value.destroyProgress
                        );
                    }
                }
                
                // Initialize pets
                pets = new List<PetInfo>();
                if (state.pets != null)
                {
                    foreach (var petInfo in state.pets)
                    {
                        pets.Add(new PetInfo(petInfo.petType, petInfo.petIndex));
                    }
                }

                Debug.Log($"Successfully loaded game state. Day: {day}, Health: {curHealth}, Cash: {cash}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading game state: {e.Message}\n{e.StackTrace}");
                NewGame(); // Fallback to new game if loading fails
            }
        }
        
        public void SaveToJson()
        {
            var serializableData = new SerializableGameData
            {
                HealthUpgradeLevel = healthUpgradeLevel,
                RegenUpgradeLevel = regenUpgradeLevel,
                SpeedUpgradeLevel = speedUpgradeLevel,
                StaminaUpgradeLevel = staminaUpgradeLevel,
                KnockbackUpgradeLevel = knockbackUpgradeLevel,
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
                PlantedCrops = plantedCrops,
                Pets = pets
            };
            
            var json = JsonSerialization.ToJson(serializableData);
            Debug.Log($"Game state serialized: {json}");
        }
    }

    [Serializable]
    public class GameState
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
        public TowerArrayWrapper[] towers;
        public List<PlantedCropKeyValue> plantedCrops;
        public List<PetInfo> pets;
    }

    [Serializable]
    public class TowerArrayWrapper
    {
        public List<TowerLevelInfo> towers = new List<TowerLevelInfo>();
    }
}
