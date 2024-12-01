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
    }
}