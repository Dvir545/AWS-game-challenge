using UnityEngine;
using UnityEngine.UI;

namespace Utils
{
    public class GameData: Singleton<GameData>
    {
        private HeldTool _curTool = HeldTool.Sword;

        private void OnSwitchTool()
        {
            _curTool = (HeldTool)(((int)_curTool + 1) % Constants.NumTools);
        }
        
        public static void SwitchTool() => Instance.OnSwitchTool();
        public static HeldTool GetCurTool() => Instance._curTool;
    }
}