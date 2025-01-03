using System;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.Data;
using World;

namespace Stores
{
    public class ToolBuyer : MonoBehaviour, IBuyable
    {
        [SerializeField] private int toolNumber;
        [SerializeField] private PlayerData playerData;
        [SerializeField] private HeldTool tool;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private TextMeshProUGUI levelText;
        private Image _iconImage;
        private int _curLevel;
        private int _curPrice;

        private void Awake()
        {
            _iconImage = transform.GetChild(0).GetComponent<Image>();
        }

        public void Init()
        {
            if (_iconImage == null)
                _iconImage = transform.GetChild(0).GetComponent<Image>();
            UpdateLevel();
        }

        public void BuyUpgrade()
        {
            bool bought = false;
            if (_curLevel > Constants.MaxUpgradeLevel)
            {
                Debug.Log("Max upgrade level reached");
            }
            else if (GameData.Instance.cash < ToolsData.GetPrice(tool, _curLevel))
            {
                Debug.Log("Not enough cash");
            }
            else
            {
                playerData.SpendCash(_curPrice);
                playerData.UpgradeTool(tool);
                UpdateLevel();
                bought = true;
            }
            if (!bought)
                SoundManager.Instance.CantPurchase();
        }

        private void UpdateLevel()
        {
            Sprite sprite;
            _curLevel = playerData.GetToolLevel(tool) + 1;
            if (_curLevel > Constants.MaxUpgradeLevel)
            {
                levelText.text = "Max";
                priceText.text = "LVL";
                sprite = SpriteData.Instance.GetToolSprite(tool, Constants.MaxUpgradeLevel);
            }
            else
            {
                _curPrice = ToolsData.GetPrice(tool, _curLevel);
                priceText.text = _curPrice + " $";
                levelText.text = "LVL " + _curLevel;
                sprite = SpriteData.Instance.GetToolSprite(tool, _curLevel);
            }
            _iconImage.sprite = sprite;
        }
        
        public void BuyItem()
        {
            BuyUpgrade();
        }
        
        public int GetItemNumber()
        {
            return toolNumber;
        }
    }
}
