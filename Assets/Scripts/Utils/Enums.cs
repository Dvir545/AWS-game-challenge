using UnityEngine;

namespace Utils
{
    public enum PlayerFacingDirection
    {
        Down = 0,
        Up = 1,
        Left = 2,
        Right = 3
    }

    public enum HeldTool
    {
        Sword = 0,
        Hoe = 1,
        Hammer = 2
    }
    

    public static class Constants
    {
        public static int NumTools = 3;
    }
}