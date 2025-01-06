using UnityEngine;

namespace Utils.Data
{
    public struct TowerData
    {
        public TowerMaterial TowerMaterial { get; private set; }
        public int Price { get; private set; }
        public int Damage { get; private set; }
        public int Range { get; private set; }
        public int MaxTargets { get; private set; }
        public float SecondsToAttack { get; private set; }
        public int SecondsToBuild { get; private set; }
        public int SecondsToDestroy { get; private set; }
        
        public TowerData(TowerMaterial tower, int price, int damage, int range, int maxTargets, float secondsToAttack, int secondsToBuild, int secondsToDestroy)
        {
            TowerMaterial = tower;
            Price = price;
            Damage = damage;
            Range = range;
            MaxTargets = maxTargets;
            SecondsToAttack = secondsToAttack;
            SecondsToBuild = secondsToBuild;
            SecondsToDestroy = secondsToDestroy;
        }

        public GameObject GetFloorPrefab()
        {
            return PrefabData.Instance.GetTowerFloorPrefab(TowerMaterial);
        }
    }
    public class TowersData: Singleton<TowersData>
    {
        private TowerData[] _towersData = {
            new(TowerMaterial.Wood, 20, 1, 5, 2, 2.5f, 10, 13),
            new(TowerMaterial.Stone, 50, 2, 5, 3, 3, 15, 20),
            new(TowerMaterial.Steel, 100, 3, 5, 4, 1.5f, 20, 27),
            new(TowerMaterial.Gold, 250, 4, 6, 5, 1f, 25, 27),
            new(TowerMaterial.Diamond, 600, 5, 7, 6, 1f, 30, 35)
        };
        
        public TowerData GetTowerData(TowerMaterial tower)
        {
            return _towersData[(int)tower];
        }
    }
}