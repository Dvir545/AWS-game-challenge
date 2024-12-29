using Crops;
using Player;
using Towers;
using UI.GameUI;
using UnityEngine;
using Utils;
using Utils.Data;

namespace World
{
    public class GameEnder: Singleton<GameEnder>
    {
        [SerializeField] private TowerBuildManager towerBuildManager;
        [SerializeField] private FarmingManager farmingManager;
        [SerializeField] private PlayerData playerData;
        [SerializeField] private Transform player;
        [SerializeField] private PetsManager petsManager;

        public void EndGame(bool died)
        {
            towerBuildManager.ResetTowers();
            farmingManager.ResetCrops();
            playerData.Reset();
            MinimapBehaviour.Instance.Reset();
            player.GetComponent<PlayerMovement>().Reset();
            player.GetComponent<PlayerActionManager>().Reset();
            player.GetComponent<PlayerHealthManager>().Reset();
            player.GetComponent<PlayerSoundManager>().Reset();
            petsManager.Reset();
            
            GameStarter.Instance.Init(died: died);
        }
    }
}