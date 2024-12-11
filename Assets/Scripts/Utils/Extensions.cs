using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Utils
{
    public static class TileExtensions
    {
        public static Vector3Int[] GetAdjacentTiles(this Vector3Int tile, FacingDirection facingDirection)
        {
            switch (facingDirection)
            {
                case FacingDirection.Down:
                    return new[]
                    {
                        tile + new Vector3Int(0, -1, 0),
                        tile + new Vector3Int(1, -1, 0),
                        tile + new Vector3Int(-1, -1, 0)
                    };
                case FacingDirection.Up:
                    return new[]
                    {
                        tile + new Vector3Int(0, 1, 0),
                        tile + new Vector3Int(1, 1, 0),
                        tile + new Vector3Int(-1, 1, 0)
                    };
                case  FacingDirection.Left:
                    return new[]
                    {
                        tile + new Vector3Int(1, 0, 0),
                        tile + new Vector3Int(1, 1, 0),
                        tile + new Vector3Int(1, -1, 0)
                    };
                case FacingDirection.Right:
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
        public static FacingDirection GetFacingDirection(this Vector2 direction)
        {
            float angle = Vector2.SignedAngle(Vector2.up, direction);
            if (angle is > -45 and < 45)
            {
                return FacingDirection.Up;
            }
            else if (angle is >= 45 and <= 135)
            {
                return FacingDirection.Right;
            }
            else if (angle is > 135 or < -135)
            {
                return FacingDirection.Down;
            }
            else
            {
               return FacingDirection.Left;
            }
        }
    }
}