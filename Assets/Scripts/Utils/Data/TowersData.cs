using UnityEngine;

namespace Utils.Data
{
    public struct TowerData
    {
        public TowerMaterial TowerMaterial { get; private set; }
        public int Damage { get; private set; }
        public int Range { get; private set; }
        public float SecondsToAttack { get; private set; }
        public int SecondsToBuild { get; private set; }
        public int SecondsToDestroy { get; private set; }
        
        public TowerData(TowerMaterial tower, int damage, int range, float secondsToAttack, int secondsToBuild, int secondsToDestroy)
        {
            TowerMaterial = tower;
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
            new TowerData(TowerMaterial.Wood, 1, 3, 3, 1, 5),
            new TowerData(TowerMaterial.Stone, 2, 4, 2, 1, 7),
            new TowerData(TowerMaterial.Steel, 3, 5, 1.5f, 1, 9),
            new TowerData(TowerMaterial.Gold, 4, 6, 1f, 1, 9),
            new TowerData(TowerMaterial.Diamond, 5, 7, 1f, 1, 12)
            // todo change times to build: 10, 15, 20, 25, 30
        };
        
        public TowerData GetTowerData(TowerMaterial tower)
        {
            return _towersData[(int)tower];
        }
    }
}