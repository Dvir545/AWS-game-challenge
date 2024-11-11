using System;
using UnityEngine;
using UnityEngine.UI;
using Utils.Data;

namespace Utils
{
    
    public class GameData: Singleton<GameData>
    {
        private HeldTool _curTool = HeldTool.Sword;
        private int _curSword = 0;
        private int _curHoe = 0;
        private int _curHammer = 0;
        
        private void OnSwitchTool()
        {
            _curTool = (HeldTool)(((int)_curTool + 1) % Constants.NumTools);
        }
        
        private int GetTypeOfCurTool() => _curTool switch
        {
            HeldTool.Sword => _curSword,
            HeldTool.Hoe => _curHoe,
            HeldTool.Hammer => _curHammer,
            _ => 0
        };
        
        public static void SwitchTool() => Instance.OnSwitchTool();
        public static HeldTool GetCurTool() => Instance._curTool;
        public static float GetAnimationSpeedMultiplier => ToolsData.GetAnimationSpeedMultiplier(Instance.GetTypeOfCurTool(), Instance._curTool);
        public static float GetProgressSpeedMultiplier => ToolsData.GetProgressSpeedMultiplier(Instance.GetTypeOfCurTool(), Instance._curTool);
        public static float GetDamageMultiplier => ToolsData.GetDamageMultiplier(Instance.GetTypeOfCurTool(), Instance._curTool);
    }
}