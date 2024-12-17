using System.Linq;
using Unity.Serialization.Json;

namespace Utils.Data
{
    class SerializableGameData
    {
        public int healthUpgradeLevel;
        public int regenUpgradeLevel;
        public int speedUpgradeLevel;
        public int swordLevel;
        public int hoeLevel;
        public int hammerLevel;
        public int cash;
        public int day;
        public int curHealth;
        public int[] crops;
        public int[] materials;
        public int[] cropsInStore;
        public int[] materialsInStore;
        public int[] thisDayEnemies;
        public int[] thisNightEnemies;
        public int[][] towerLevels;
    }
    
    public class GameData: Singleton<GameData>
    {
        public int healthUpgradeLevel;
        public int regenUpgradeLevel;
        public int speedUpgradeLevel;
        public int swordLevel;
        public int hoeLevel;
        public int hammerLevel;
        public int cash;
        public int day;
        public int curHealth;
        public int[] crops;
        public int[] materials;
        public int[] cropsInStore;
        public int[] materialsInStore;
        public int[] thisDayEnemies;
        public int[] thisNightEnemies;
        public int[][] towerLevels;
        
        public GameData()
        {
            NewGame();
        }

        private void NewGame()
        {
            healthUpgradeLevel = Constants.StartHealthUpgradeLevel;
            regenUpgradeLevel = Constants.StartRegenUpgradeLevel;
            speedUpgradeLevel = Constants.StartSpeedUpgradeLevel;
            swordLevel = Constants.StartSwordLevel;
            hoeLevel = Constants.StartHoeLevel;
            hammerLevel = Constants.StartHammerLevel;
            cash = Constants.StartCash;
            day = Constants.StartDay;
            curHealth = Constants.StartHealth;
            crops = Constants.StartCrops.ToArray();
            materials = Constants.StartMaterials.ToArray();
            cropsInStore = Constants.StartCropsInStore.ToArray();
            materialsInStore = Constants.StartMaterialsInStore.ToArray();
            thisDayEnemies = Constants.FirstDayEnemies.ToArray();
            thisNightEnemies = Constants.FirstNightEnemies.ToArray();
            towerLevels = new int[Constants.TowerCount][];
            for (int i = 0; i < Constants.TowerCount; i++)
            {
                towerLevels[i] = new [] {-1, -1, -1};
            }
        }
        
        public void LoadFromJson(string json)
        {
            var data = JsonSerialization.FromJson<SerializableGameData>(json);
            healthUpgradeLevel = data.healthUpgradeLevel;
            regenUpgradeLevel = data.regenUpgradeLevel;
            speedUpgradeLevel = data.speedUpgradeLevel;
            swordLevel = data.swordLevel;
            hoeLevel = data.hoeLevel;
            hammerLevel = data.hammerLevel;
            cash = data.cash;
            day = data.day;
            curHealth = data.curHealth;
            crops = data.crops;
            materials = data.materials;
            cropsInStore = data.cropsInStore;
            materialsInStore = data.materialsInStore;
            thisDayEnemies = data.thisDayEnemies;
            thisNightEnemies = data.thisNightEnemies;
            towerLevels = data.towerLevels;
        }
        
        public void SaveToJson()
        {
            var serializableData = new SerializableGameData
            {
                healthUpgradeLevel = healthUpgradeLevel,
                regenUpgradeLevel = regenUpgradeLevel,
                speedUpgradeLevel = speedUpgradeLevel,
                swordLevel = swordLevel,
                hoeLevel = hoeLevel,
                hammerLevel = hammerLevel,
                cash = cash,
                day = day,
                curHealth = curHealth,
                crops = crops,
                materials = materials,
                cropsInStore = cropsInStore,
                materialsInStore = materialsInStore,
                thisDayEnemies = thisDayEnemies,
                thisNightEnemies = thisNightEnemies,
                towerLevels = towerLevels
            };
            var json = JsonSerialization.ToJson(serializableData);
            // DVIR - upload json to aws
        }
    }
}