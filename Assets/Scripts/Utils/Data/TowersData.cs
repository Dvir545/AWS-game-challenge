using UnityEngine;

namespace Utils.Data
{
    public struct TowerData
    {
        public TowerMaterial TowerMaterial { get; private set; }
        public int Price { get; private set; }
        public int Damage { get; private set; }
        public int Range { get; private set; }
        public float SecondsToAttack { get; private set; }
        public int SecondsToBuild { get; private set; }
        public int SecondsToDestroy { get; private set; }
        
        public TowerData(TowerMaterial tower, int price, int damage, int range, float secondsToAttack, int secondsToBuild, int secondsToDestroy)
        {
            TowerMaterial = tower;
            Price = price;
            Damage = damage;
            Range = range;
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
            new(TowerMaterial.Wood, 20, 1, 5, 2.5f, 10, 5),
            new(TowerMaterial.Stone, 50, 2, 5, 2, 15, 7),
            new(TowerMaterial.Steel, 100, 3, 5, 1.5f, 20, 9),
            new(TowerMaterial.Gold, 250, 4, 6, 1f, 25, 9),
            new(TowerMaterial.Diamond, 500, 5, 7, 1f, 30, 12)
        };
        
        public TowerData GetTowerData(TowerMaterial tower)
        {
            return _towersData[(int)tower];
        }
    }
}