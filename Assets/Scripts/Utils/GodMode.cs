using Player;
using UnityEngine;
using Utils.Data;

namespace Utils
{
    public class GodMode : MonoBehaviour
    {
        [SerializeField] private PlayerData playerData;


        public void InfiniteCash()
        {
            playerData.AddCash(99999 - GameData.Instance.cash);
        }
    }
}
