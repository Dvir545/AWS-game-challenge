using UnityEngine;

namespace Utils.Data
{
    public class PrefabData: Singleton<PrefabData>
    {
        [Header("Tower Floors")] 
        [SerializeField] private GameObject woodFloorPrefab;
        [SerializeField] private GameObject stoneFloorPrefab;
        [SerializeField] private GameObject steelFloorPrefab;
        [SerializeField] private GameObject goldFloorPrefab;
        [SerializeField] private GameObject diamondFloorPrefab;

        public GameObject GetTowerFloorPrefab(TowerMaterial material)
        {
            switch (material)
            {
                case TowerMaterial.Wood:
                    return woodFloorPrefab;
                case TowerMaterial.Stone:
                    return stoneFloorPrefab;
                case TowerMaterial.Steel:
                    return steelFloorPrefab;
                case TowerMaterial.Gold:
                    return goldFloorPrefab;
                case TowerMaterial.Diamond:
                    return diamondFloorPrefab;
                default:
                    return null;
            }
        }
    
        [Header("Enemies")]
        [SerializeField] private GameObject slimePrefab;
        [SerializeField] private GameObject skeletonPrefab;
        [SerializeField] private GameObject goblinPrefab;
        [SerializeField] private GameObject chickenPrefab;
        [SerializeField] private GameObject orcPrefab;
        [SerializeField] private GameObject demonPrefab;
        
        public GameObject GetEnemyPrefab(Enemy enemy)
        {
            switch (enemy)
            {
                case Enemy.Slime:
                    return slimePrefab;
                case Enemy.Skeleton:
                    return skeletonPrefab;
                case Enemy.Goblin:
                    return goblinPrefab;
                case Enemy.Chicken:
                    return chickenPrefab;
                case Enemy.Orc:
                    return orcPrefab;
                case Enemy.Demon:
                    return demonPrefab;
                default:
                    return null;
            }
        }
    }
}