using UnityEngine;
using Utils;
using Utils.Data;

namespace Player
{
    public class PlayerData: MonoBehaviour
    {
        private HeldTool _curTool = HeldTool.Sword;
        private int _curSword = 0;
        private int _curHoe = 0;
        private int _curHammer = 0;

        private int[] _numCrops = {1, 1, 1, 1, 1};
        private int[] _numMaterials = {1, 1, 1, 1, 1};
        
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
        public int GetDamageMultiplier => ToolsData.GetDamageMultiplier(GetTypeOfCurTool(), _curTool);

        public int MaxHealth { get; private set; } = Constants.StartHealth;

        public void IncMaxHealth()
        {
            MaxHealth++;
            CurHealth += 2;
            EventManager.Instance.TriggerEvent(EventManager.MaxHealthIncreased, MaxHealth);
        }
        
        public int CurHealth { get; private set; } = Constants.StartHealth;

        public void AddHealth(int amount)
        {
            CurHealth += amount;
            EventManager.Instance.TriggerEvent(EventManager.HealthChanged, CurHealth);
        }

        public void IncHealth()
        {
            AddHealth(1);
        }

        public void SubtractHealth(int amount)
        {
            CurHealth -= amount;
            EventManager.Instance.TriggerEvent(EventManager.HealthChanged, CurHealth);
        }

        public void DecHealth()
        {
            SubtractHealth(1);
        }

        public int CurCash { get; private set; } = Constants.StartingCash;

        public void AddCash(int amount)
        {
            CurCash += amount;
            EventManager.Instance.TriggerEvent(EventManager.CashChanged, CurCash);
        }
        public void SpendCash(int amount)
        {
            CurCash -= amount;
            EventManager.Instance.TriggerEvent(EventManager.CashChanged, CurCash);
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
        
        public int GetNumMaterials(TowerMaterial material) => _numMaterials[(int)material];
        public int GetNumMaterialTypes() => _numMaterials.Length;
        public void AddMaterial(TowerMaterial material)
        {
            _numMaterials[(int)material]++;
        }
        public void RemoveMaterial(TowerMaterial material)
        {
            _numMaterials[(int)material]--;
        }
    }
}