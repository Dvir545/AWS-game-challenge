using System;
using UnityEngine;

namespace Utils.Data
{
    public struct TowerSprites
    {
        public Sprite Center { get; private set; }
        public Sprite Left { get; private set; }
        public Sprite Right { get; private set; }
    }
    public class SpriteData: Singleton<SpriteData>
    {
        [Header("Crop Sprites")]
        [SerializeField] private Sprite[] wheatSprites;
        [SerializeField] private Sprite[] carrotSprites;
        [SerializeField] private Sprite[] tomatoSprites;
        [SerializeField] private Sprite[] cornSprites;
        [SerializeField] private Sprite[] pumpkinSprites;
        public Sprite GetCropSprite(Crop crop, float growth)
        {
            var sprites = crop switch
            {
                Crop.Wheat => wheatSprites,
                Crop.Carrot => carrotSprites,
                Crop.Tomato => tomatoSprites,
                Crop.Corn => cornSprites,
                Crop.Pumpkin => pumpkinSprites,
                _ => throw new ArgumentOutOfRangeException(nameof(crop), crop, null)
            };
            return sprites[Mathf.Min(sprites.Length - 1, Mathf.FloorToInt(growth * sprites.Length))];
        }
        
        [Header("Tool Sprites")] 
        [SerializeField] private Sprite[] swordSprites;
        [SerializeField] private Sprite[] swordSpritesOutline;
        [SerializeField] private Sprite[] hoeSprites;
        [SerializeField] private Sprite[] hoeSpritesOutline;
        [SerializeField] private Sprite[] hammerSprites;
        [SerializeField] private Sprite[] hammerSpritesOutline;
        public Sprite GetToolSprite(HeldTool tool, int level, bool outline=true) 
        => tool switch
            {
                HeldTool.Sword => outline? swordSpritesOutline[level] : swordSprites[level],
                HeldTool.Hoe => outline? hoeSpritesOutline[level] : hoeSprites[level],
                HeldTool.Hammer => outline? hammerSpritesOutline[level] : hammerSprites[level],
                _ => throw new ArgumentOutOfRangeException(nameof(tool), tool, null)
            };

        [Header("Upgrade Sprites")] 
        [SerializeField] private Sprite healthSprite;

        [SerializeField] private Sprite healthSpriteOutline;
        [SerializeField] private Sprite[] regenSprites;
        [SerializeField] private Sprite[] regenSpritesOutline;
        [SerializeField] private Sprite[] speedSprites;
        [SerializeField] private Sprite[] speedSpritesOutline;
        [SerializeField] private Sprite[] staminaSprites;
        [SerializeField] private Sprite[] staminaSpritesOutline;

        public Sprite GetUpgradeSprite(Upgrade upgradeType, int level, bool outline=true) => upgradeType switch
        {
            Upgrade.Health => outline? healthSpriteOutline : healthSprite,
            Upgrade.Regen => outline? regenSpritesOutline[level] : regenSprites[level],
            Upgrade.Speed => outline? speedSpritesOutline[level] : speedSprites[level],
            Upgrade.Stamina => outline? staminaSpritesOutline[level] : staminaSprites[level],
            _ => throw new ArgumentOutOfRangeException(nameof(upgradeType), upgradeType, null)
        };
        
        [Header("Progress Bar Sprites")]
        [SerializeField] private Sprite[] progressBarDefaultSprites;
        [SerializeField] private Sprite[] progressBarStaminaSprites0;
        [SerializeField] private Sprite[] progressBarCooldownSprites0;
        [SerializeField] private Sprite[] progressBarStaminaSprites1;
        [SerializeField] private Sprite[] progressBarCooldownSprites1;
        [SerializeField] private Sprite[] progressBarStaminaSprites2;
        [SerializeField] private Sprite[] progressBarCooldownSprites2;
        [SerializeField] private Sprite[] progressBarStaminaSprites3;
        [SerializeField] private Sprite[] progressBarCooldownSprites3;
        [SerializeField] private Sprite[] progressBarEvilSprites;

        public Sprite[] GetProgressBarSprites(ProgressBarType type)
        {
            switch (type)
            {
                case ProgressBarType.Default:
                    return progressBarDefaultSprites;
                case ProgressBarType.Stamina:
                    return GameData.Instance.staminaUpgradeLevel switch
                    {
                        0 => progressBarStaminaSprites0,
                        1 => progressBarStaminaSprites1,
                        2 => progressBarStaminaSprites2,
                        3 => progressBarStaminaSprites3,
                        _ => throw new ArgumentOutOfRangeException(nameof(GameData.Instance.staminaUpgradeLevel), GameData.Instance.staminaUpgradeLevel, null)
                    };
                case ProgressBarType.Cooldown:
                    return GameData.Instance.staminaUpgradeLevel switch
                    {
                        0 => progressBarCooldownSprites0,
                        1 => progressBarCooldownSprites1,
                        2 => progressBarCooldownSprites2,
                        3 => progressBarCooldownSprites3,
                        _ => throw new ArgumentOutOfRangeException(nameof(GameData.Instance.staminaUpgradeLevel), GameData.Instance.staminaUpgradeLevel, null)
                    };
                case ProgressBarType.Evil:
                    return progressBarEvilSprites;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}