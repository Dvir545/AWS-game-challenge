using System;
using UnityEngine;

namespace Utils
{
    public enum FacingDirection
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

    public enum Upgrade
    {
        Health = 0,
        Regen = 1,
        Speed = 2,
        Stamina = 3
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
        Goblin = 4,
        Demon = 5,
        EvilBall = 6
    }

    public enum TowerMaterial
    {
        Wood = 0,
        Stone = 1,
        Steel = 2,
        Gold = 3,
        Diamond = 4
    }

    public enum StoreType
    {
        Crops = 0,
        Materials = 1,
        Tools = 2,
        Upgrades = 3
    }

    public enum NPCType
    {
        Start = 0,
        Mid = 1
    }

    public enum DayPhase
    {
        Day = 0,
        DayEnd = 1,
        Night = 2,
        NightEnd = 3
    }
    
    public enum ProgressBarType
    {
        Default = 0,
        Stamina = 1,
        Cooldown = 2,
        Evil = 3
    }
}