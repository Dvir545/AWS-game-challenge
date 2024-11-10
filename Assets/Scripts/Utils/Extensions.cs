using UnityEngine;

namespace Utils
{
    public static class HeldToolExtensions
    {
        public static Sprite GetToolSprite(this HeldTool tool, Sprite[] icons)
        {
            return tool switch
            {
                HeldTool.Sword => icons[0],
                HeldTool.Hoe => icons[1],
                HeldTool.Hammer => icons[2],
                _ => icons[0] // Default to first sprite if somehow invalid
            };
        }
    }
}