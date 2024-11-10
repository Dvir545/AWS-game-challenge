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

    public static class TileExtensions
    {
        public static Vector3Int[] GetAdjacentTiles(this Vector3Int tile, PlayerFacingDirection facingDirection)
        {
            switch (facingDirection)
            {
                case PlayerFacingDirection.Down:
                    return new[]
                    {
                        tile + new Vector3Int(0, -1, 0),
                        tile + new Vector3Int(1, -1, 0),
                        tile + new Vector3Int(-1, -1, 0)
                    };
                case PlayerFacingDirection.Up:
                    return new[]
                    {
                        tile + new Vector3Int(0, 1, 0),
                        tile + new Vector3Int(1, 1, 0),
                        tile + new Vector3Int(-1, 1, 0)
                    };
                case  PlayerFacingDirection.Left:
                    return new[]
                    {
                        tile + new Vector3Int(1, 0, 0),
                        tile + new Vector3Int(1, 1, 0),
                        tile + new Vector3Int(1, -1, 0)
                    };
                case PlayerFacingDirection.Right:
                    return new[]
                    {
                        tile + new Vector3Int(-1, 0, 0),
                        tile + new Vector3Int(-1, 1, 0),
                        tile + new Vector3Int(-1, -1, 0)
                    };
                default:
                    return null;
            }
        }
    }
}