using System;
using UnityEngine;
using UnityEngine.UI;
using Utils.Data;

namespace Utils
{
    
    public class PlayerData: MonoBehaviour
    {
        private HeldTool _curTool = HeldTool.Sword;
        private int _curSword = 0;
        private int _curHoe = 0;
        private int _curHammer = 0;

        private int _curCash = Constants.StartingCash;
        
        private int[] _numCrops = {1, 1, 1, 1, 1};
        
        public void SwitchTool()
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
        public HeldTool GetCurTool() => _curTool;
        public float GetAnimationSpeedMultiplier => ToolsData.GetAnimationSpeedMultiplier(GetTypeOfCurTool(), _curTool);
        public float GetProgressSpeedMultiplier => ToolsData.GetProgressSpeedMultiplier(GetTypeOfCurTool(), _curTool);
        public float GetDamageMultiplier => ToolsData.GetDamageMultiplier(GetTypeOfCurTool(), _curTool);

        public int GetCurCash => _curCash;

        public int AddCash(int amount)
        {
            _curCash += amount;
            EventManager.Instance.TriggerEvent(EventManager.CashChanged, _curCash);
            return _curCash;
        }
        public int SpendCash(int amount)
        {
            _curCash -= amount;
            EventManager.Instance.TriggerEvent(EventManager.CashChanged, _curCash);
            return _curCash;
        }
        
        public int GetNumCrops(Crop cropType) => _numCrops[(int)cropType];
        public int GetNumCropTypes() => _numCrops.Length;
        public void AddCrop(Crop cropType)
        {
            _numCrops[(int)cropType]++;
        }
        public void RemoveCrop(Crop cropType)
        {
            _numCrops[(int)cropType]--;
        }
    }
}