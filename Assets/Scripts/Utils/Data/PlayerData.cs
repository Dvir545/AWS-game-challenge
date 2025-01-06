using Player;
using UnityEngine;
using World;

namespace Utils.Data
{
    public class PlayerData: MonoBehaviour
    {
        private HeldTool _curTool = HeldTool.Sword;

        public float RegenSpeedMultiplier { get; private set; } 
        public float SpeedMultiplier { get; private set; }
        public float StaminaMultiplier { get; private set; }
        public float KnockbackMultiplier { get; private set; }

        private void Start()
        {
            RegenSpeedMultiplier = UpgradesData.GetSpeedMultiplier(GameData.Instance.regenUpgradeLevel);
            SpeedMultiplier = UpgradesData.GetSpeedMultiplier(GameData.Instance.speedUpgradeLevel);
            StaminaMultiplier = UpgradesData.GetStaminaMultiplier(GameData.Instance.staminaUpgradeLevel);
            KnockbackMultiplier = UpgradesData.GetKnockbackMultiplier(GameData.Instance.knockbackUpgradeLevel);
        }
        

        public void SwitchTool(bool next)
        {
            var idx = next? (int)_curTool + 1 : (int)_curTool - 1;
            _curTool = (HeldTool)MathUtils.Mod(idx, Constants.NumTools);
        }
        private int GetCurToolLevel() => _curTool switch
        {
            HeldTool.Sword => GameData.Instance.swordLevel,
            HeldTool.Hoe => GameData.Instance.hoeLevel,
            HeldTool.Hammer => GameData.Instance.hammerLevel,
            _ => 0
        };
        public int GetToolLevel(HeldTool tool) => tool switch
            {
                HeldTool.Sword => GameData.Instance.swordLevel,
                HeldTool.Hoe => GameData.Instance.hoeLevel,
                HeldTool.Hammer => GameData.Instance.hammerLevel,
                _ => 0
            };
        public HeldTool GetCurTool() => _curTool;

        public void UpgradeTool(HeldTool tool)
        {
            switch (tool)
            {
                case HeldTool.Sword:
                    GameData.Instance.swordLevel++;
                    break;
                case HeldTool.Hoe:
                    GameData.Instance.hoeLevel++;
                    break;
                case HeldTool.Hammer:
                    GameData.Instance.hammerLevel++;
                    break;
            }
            EventManager.Instance.TriggerEvent(EventManager.ActiveToolChanged, null);
        }
        public float GetProgressSpeedMultiplier => ToolsData.GetProgressSpeedMultiplier(GetCurToolLevel(), _curTool);
        public int GetDamageMultiplier => ToolsData.GetDamageMultiplier(GetCurToolLevel(), _curTool);

        public int MaxHealth => Constants.StartHealth + GameData.Instance.healthUpgradeLevel * 2;

        public void IncMaxHealth()
        {
            GameData.Instance.curHealth += 2;
            EventManager.Instance.TriggerEvent(EventManager.MaxHealthIncreased, MaxHealth);
        }
        
        public void AddHealth(int amount)
        {
            if (GameData.Instance.curHealth >= MaxHealth) return;
            GameData.Instance.curHealth += amount;
            EventManager.Instance.TriggerEvent(EventManager.HealthChanged, GameData.Instance.curHealth);
        }

        public void IncHealth()
        {
            AddHealth(1);
        }

        public void SubtractHealth(int amount)
        {
            GameData.Instance.curHealth -= amount;
            EventManager.Instance.TriggerEvent(EventManager.HealthChanged, GameData.Instance.curHealth);
        }
        
        public void AddCash(int amount)
        {
            GameData.Instance.cash += amount;
            SoundManager.Instance.GotMoney();
            EventManager.Instance.TriggerEvent(EventManager.CashChanged, GameData.Instance.cash);
        }
        public void SpendCash(int amount)
        {
            GameData.Instance.cash -= amount;
            SoundManager.Instance.Purchase();
            EventManager.Instance.TriggerEvent(EventManager.CashChanged, GameData.Instance.cash);
        }
        
        public int GetNumCrops(Crop cropType) => GameData.Instance.crops[(int)cropType];
        public int GetNumCropTypes() => GameData.Instance.crops.Length;
        public void AddCrop(Crop cropType)
        {
            GameData.Instance.crops[(int)cropType]++;
        }
        public void RemoveCrop(Crop cropType)
        {
            GameData.Instance.crops[(int)cropType]--;
        }
        
        public int GetNumMaterials(TowerMaterial material) => GameData.Instance.materials[(int)material];
        public int GetNumMaterialTypes() => GameData.Instance.materials.Length;
        public void AddMaterial(TowerMaterial material)
        {
            GameData.Instance.materials[(int)material]++;
        }
        public void RemoveMaterial(TowerMaterial material)
        {
            GameData.Instance.materials[(int)material]--;
        }

        public int GetUpgradeLevel(Upgrade upgradeType) => upgradeType switch
        {
            Upgrade.Health => GameData.Instance.healthUpgradeLevel,
            Upgrade.Regen => GameData.Instance.regenUpgradeLevel,
            Upgrade.Speed => GameData.Instance.speedUpgradeLevel,
            Upgrade.Stamina => GameData.Instance.staminaUpgradeLevel,
            Upgrade.Knockback => GameData.Instance.knockbackUpgradeLevel,
            _ => 0
        };

        public void UpgradeUpgrade(Upgrade upgradeType)
        {
            switch (upgradeType)
            {
                case Upgrade.Health: 
                    GameData.Instance.healthUpgradeLevel++;
                    IncMaxHealth();
                    break;
                case Upgrade.Regen: 
                    GameData.Instance.regenUpgradeLevel++;
                    RegenSpeedMultiplier = UpgradesData.GetRegenSpeedMultiplier(GameData.Instance.regenUpgradeLevel);
                    break;
                case Upgrade.Speed: 
                    GameData.Instance.speedUpgradeLevel++;
                    SpeedMultiplier = UpgradesData.GetSpeedMultiplier(GameData.Instance.speedUpgradeLevel);
                    break;
                case Upgrade.Stamina:
                    GameData.Instance.staminaUpgradeLevel++;
                    StaminaMultiplier = UpgradesData.GetStaminaMultiplier(GameData.Instance.staminaUpgradeLevel);
                    break;
                case Upgrade.Knockback:
                    GameData.Instance.knockbackUpgradeLevel++;
                    KnockbackMultiplier = UpgradesData.GetKnockbackMultiplier(GameData.Instance.knockbackUpgradeLevel);
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
        public void AddPet(Pet pet, int index)
        {
            GameData.Instance.pets.Add(new PetInfo((int)pet, index));
            PetsManager.Instance.AddPet(pet, index);
        }

        public void Reset()
        {
            _curTool = HeldTool.Sword;
        }

    }
}