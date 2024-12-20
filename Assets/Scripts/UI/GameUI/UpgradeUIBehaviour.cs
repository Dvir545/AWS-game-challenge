using System;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.Data;

namespace UI.GameUI
{
    public class UpgradeUIBehaviour: MonoBehaviour
    {
        private Image image;
        [SerializeField] private Upgrade upgradeType;
        
        private void Awake()
        {
            Init();
        }
        
        private void Start()
        {
            EventManager.Instance.StartListening(EventManager.AbilityUpgraded, ChangeIcon);
        }

        public void Init()
        {
            ChangeIcon((Upgrade.Speed, GameData.Instance.speedUpgradeLevel));
            ChangeIcon((Upgrade.Regen, GameData.Instance.regenUpgradeLevel));        
        }

        private void ChangeIcon(object arg0)
        {
            if (image == null)
            {
                image = GetComponent<Image>();
            }
            if (arg0 is (Upgrade upgrade, int level))
            {
                if (upgrade == upgradeType)
                {
                    image.sprite = SpriteData.Instance.GetUpgradeSprite(upgradeType, level, outline: false);
                }
            }
        }
    }
}