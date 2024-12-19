using Player;
using UnityEngine;
using Utils;

namespace UI.GameUI
{
    public class HealEffectBehaviour: MonoBehaviour
    {
        private ParticleSystem _ps;
        private PlayerData _playerData;

        private void Awake()
        {
            _playerData = FindObjectOfType<PlayerData>();
            _ps = GetComponent<ParticleSystem>();
            ParticleSystem.MainModule ma = _ps.main;
            ma.startColor = _playerData.GetUpgradeColor(Upgrade.Regen);
        }
    }
}