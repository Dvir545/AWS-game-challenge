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

        public Sprite GetUpgradeSprite(Upgrade upgradeType, int level, bool outline=true) => upgradeType switch
        {
            Upgrade.Health => outline? healthSpriteOutline : healthSprite,
            Upgrade.Regen => outline? regenSpritesOutline[level] : regenSprites[level],
            Upgrade.Speed => outline? speedSpritesOutline[level] : speedSprites[level],
            _ => throw new ArgumentOutOfRangeException(nameof(upgradeType), upgradeType, null)
        };
    }
}