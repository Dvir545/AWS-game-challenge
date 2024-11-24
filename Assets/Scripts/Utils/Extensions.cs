using System.Collections;
using UnityEngine;
using UnityEngine.AI;

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
        public static Vector3Int[] GetAdjacentTiles(this Vector3Int tile, CharacterFacingDirection facingDirection)
        {
            switch (facingDirection)
            {
                case CharacterFacingDirection.Down:
                    return new[]
                    {
                        tile + new Vector3Int(0, -1, 0),
                        tile + new Vector3Int(1, -1, 0),
                        tile + new Vector3Int(-1, -1, 0)
                    };
                case CharacterFacingDirection.Up:
                    return new[]
                    {
                        tile + new Vector3Int(0, 1, 0),
                        tile + new Vector3Int(1, 1, 0),
                        tile + new Vector3Int(-1, 1, 0)
                    };
                case  CharacterFacingDirection.Left:
                    return new[]
                    {
                        tile + new Vector3Int(1, 0, 0),
                        tile + new Vector3Int(1, 1, 0),
                        tile + new Vector3Int(1, -1, 0)
                    };
                case CharacterFacingDirection.Right:
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

    public static class Vector2Extensions
    {
        public static CharacterFacingDirection GetFacingDirection(this Vector2 direction)
        {
            float angle = Vector2.SignedAngle(Vector2.up, direction);
            if (angle is > -45 and < 45)
            {
                return CharacterFacingDirection.Up;
            }
            else if (angle is >= 45 and <= 135)
            {
                return CharacterFacingDirection.Right;
            }
            else if (angle is > 135 or < -135)
            {
                return CharacterFacingDirection.Down;
            }
            else
            {
               return CharacterFacingDirection.Left;
            }
        }
    }
}