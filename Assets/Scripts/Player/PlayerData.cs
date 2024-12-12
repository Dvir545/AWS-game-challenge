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

        private int _curHealthUpgradeLevel = 0;
        private int _curRegenUpgradeLevel = 0;
        private int _curSpeedUpgradeLevel = 0;
        public float RegenSpeedMultiplier { get; private set; } = 1;
        public float SpeedMultiplier { get; private set; } = 1;

        private int[] _numCrops = {1, 1, 1, 1, 1};
        private int[] _numMaterials = {1, 1, 1, 1, 1};
        
        public void SwitchTool()
        {
            _curTool = (HeldTool)(((int)_curTool + 1) % Constants.NumTools);
        }
        private int GetCurToolLevel() => _curTool switch
        {
            HeldTool.Sword => _curSword,
            HeldTool.Hoe => _curHoe,
            HeldTool.Hammer => _curHammer,
            _ => 0
        };
        public int GetToolLevel(HeldTool tool) => tool switch
            {
                HeldTool.Sword => _curSword,
                HeldTool.Hoe => _curHoe,
                HeldTool.Hammer => _curHammer,
                _ => 0
            };
        public HeldTool GetCurTool() => _curTool;

        public void UpgradeTool(HeldTool tool)
        {
            switch (tool)
            {
                case HeldTool.Sword:
                    _curSword++;
                    break;
                case HeldTool.Hoe:
                    _curHoe++;
                    break;
                case HeldTool.Hammer:
                    _curHammer++;
                    break;
            }
            EventManager.Instance.TriggerEvent(EventManager.ActiveToolChanged, null);
        }
        public float GetProgressSpeedMultiplier => ToolsData.GetProgressSpeedMultiplier(GetCurToolLevel(), _curTool);
        public int GetDamageMultiplier => ToolsData.GetDamageMultiplier(GetCurToolLevel(), _curTool);

        public int MaxHealth { get; private set; } = Constants.StartHealth;

        public void IncMaxHealth()
        {
            MaxHealth += 2;
            CurHealth += 2;
            EventManager.Instance.TriggerEvent(EventManager.MaxHealthIncreased, MaxHealth);
        }
        
        public int CurHealth { get; private set; } = Constants.StartHealth;

        public void AddHealth(int amount)
        {
            if (CurHealth >= MaxHealth) return;
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

        public int GetUpgradeLevel(Upgrade upgradeType) => upgradeType switch
        {
            Upgrade.Health => _curHealthUpgradeLevel,
            Upgrade.Regen => _curRegenUpgradeLevel,
            Upgrade.Speed => _curSpeedUpgradeLevel,
            _ => 0
        };

        public void UpgradeUpgrade(Upgrade upgradeType)
        {
            switch (upgradeType)
            {
                case Upgrade.Health: 
                    _curHealthUpgradeLevel++;
                    IncMaxHealth();
                    break;
                case Upgrade.Regen: 
                    _curRegenUpgradeLevel++;
                    RegenSpeedMultiplier = UpgradesData.GetRegenSpeedMultiplier(_curRegenUpgradeLevel);
                    break;
                case Upgrade.Speed: 
                    _curSpeedUpgradeLevel++;
                    SpeedMultiplier = UpgradesData.GetSpeedMultiplier(_curSpeedUpgradeLevel);
                    break;
            }
            EventManager.Instance.TriggerEvent(EventManager.AbilityUpgraded, (upgradeType, GetUpgradeLevel(upgradeType)));
        }

        public Color GetCurToolColor()
        {
            var curTool = GetCurTool();
            return ToolsData.GetColor(curTool, GetToolLevel(curTool));
        }

        public Color GetUpgradeColor(Upgrade upgradeType)
        {
            switch (upgradeType)
            {
                case Upgrade.Regen:
                    return UpgradesData.GetColor(upgradeType, GetUpgradeLevel(upgradeType));
                default:
                    return Color.white;
            }
        }
    }
}