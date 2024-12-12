using System;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.Data;

namespace UI
{
    public class UpgradeUIBehaviour: MonoBehaviour
    {
        private Image image;
        [SerializeField] private Upgrade upgradeType;
        
        private void Awake()
        {
            image = GetComponent<Image>();
        }
        
        private void Start()
        {
            EventManager.Instance.StartListening(EventManager.AbilityUpgraded, ChangeIcon);
        }

        private void ChangeIcon(object arg0)
        {
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