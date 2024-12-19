using Crops;
using Enemies;
using Towers;
using UnityEngine;
using Utils;

namespace World
{
    public class GameEnder: Singleton<GameEnder>
    {
        [SerializeField] private TowerBuildManager towerBuildManager;
        [SerializeField] private FarmingManager farmingManager;
        
        public void EndGame()
        {
            // reset all towers
            towerBuildManager.ResetTowers();
            
            // remove all crops
            farmingManager.ResetCrops();
            
            GameStarter.Instance.Init();
        }
    }
}