using System;
using UnityEngine;

namespace Utils
{
    public enum CharacterFacingDirection
    {
        Down = 0,
        Up = 1,
        Left = 2,
        Right = 3
    }

    public enum TowerShootingDirection
    {
        Down = 0,
        Left = 1,
        Right = 2
    }

    public enum HeldTool
    {
        Sword = 0,
        Hoe = 1,
        Hammer = 2
    }

    public enum Crop
    {
        Wheat = 0,
        Carrot = 1,
        Tomato = 2,
        Corn = 3,
        Pumpkin = 4
    }

    public enum Enemy
    {
        Slime = 0,
        Skeleton = 1,
        Chicken = 2,
        Orc = 3,
    }

    public enum TowerMaterial
    {
        Wood = 0,
        Stone = 1,
        Steel = 2,
        Gold = 3,
        Diamond = 4
    }
}