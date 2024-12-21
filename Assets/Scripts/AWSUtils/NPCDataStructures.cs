using System;
using System.Collections.Generic;

namespace AWSUtils
{
    [Serializable]
    public class NPCResponse
    {
        public string response;
    }

    [Serializable]
    public class StartNPCRequest
    {
        public int totalGamesPlayed;
        public int consecutiveGamesPlayed;
        public string killedLastGameBy;
        public int daysSurvivedLastGame;
        public float secondsPlayedLastGame;
        public int daysSurvivedHighScore;
        public float secondsPlayedHighScore;
    }

    [Serializable]
    public class MidNPCRequest
    {
        public PlayerStatus playerStatus;
    }

    [Serializable]
    public class PlayerStatus
    {
        public int health;
        public int money;
        public Inventory inventory;
        public WorldStatus worldStatus;
        public LastRoundActivity lastRoundActivity;
        public NextRoundEnemies nextRoundEnemies;
        public List<string> previousResponses;
    }

    [Serializable]
    public class Inventory
    {
        public Crops crops;
        public TowerMaterials towerMaterials;
    }

    [Serializable]
    public class Crops
    {
        public int wheat;
        public int carrot;
        public int tomato;
        public int corn;
        public int pumpkin;
    }

    [Serializable]
    public class TowerMaterials
    {
        public int wood;
        public int stone;
        public int iron;
        public int gold;
        public int diamond;
    }

    [Serializable]
    public class WorldStatus
    {
        public int daysSurvived;
        public int availableTowerSpots;
        public int existingTowers;
        public Shops shops;
    }

    [Serializable]
    public class Shops
    {
        public SeedShop seedShopAvailableSeeds;
        public ResourcesShop resourcesShopAvailableMaterials;
        public ToolShop toolShopAvailableUpgradeLevel;
        public UtilityShop utilityShopAvailableUpgradeLevels;
    }

    [Serializable]
    public class SeedShop
    {
        public int wheat;
        public int carrot;
        public int tomato;
        public int corn;
        public int pumpkin;
    }

    [Serializable]
    public class ResourcesShop
    {
        public int wood;
        public int stone;
        public int iron;
        public int gold;
        public int diamond;
    }

    [Serializable]
    public class ToolShop
    {
        public int hoe;
        public int hammer;
        public int sword;
    }

    [Serializable]
    public class UtilityShop
    {
        public int health;
        public int speed;
        public int healthRegen;
    }

    [Serializable]
    public class LastRoundActivity
    {
        public int damageTaken;
        public int cropsPlanted;
        public int cropsHarvested;
        public int cropsDestroyed;
        public int towersBuilt;
        public int towersDestroyed;
    }

    [Serializable]
    public class NextRoundEnemies
    {
        public int slime;
        public int skeleton;
        public int goblinArcher;
        public int chicken;
        public int orc;
        public int demon;
    }
}