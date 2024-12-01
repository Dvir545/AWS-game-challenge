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


    }
}