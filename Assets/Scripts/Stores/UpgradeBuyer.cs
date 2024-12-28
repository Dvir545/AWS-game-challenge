using Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.Data;
using World;

namespace Stores
{
    public class UpgradeBuyer : MonoBehaviour, IBuyable
    {
        [SerializeField] private int upgradeNumber;
        [SerializeField] private PlayerData playerData;
        [SerializeField] private Upgrade upgradeType;
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
            else if (GameData.Instance.cash < UpgradesData.GetPrice(upgradeType, _curLevel))
            {
                Debug.Log("Not enough cash");
            }
            else
            {
                playerData.SpendCash(_curPrice);
                playerData.UpgradeUpgrade(upgradeType);
                UpdateLevel();
                bought = true;
            }
            if (!bought)
                SoundManager.Instance.CantPurchase();
        }

        private void UpdateLevel()
        {
            Sprite sprite;
            _curLevel = playerData.GetUpgradeLevel(upgradeType) + 1;
            if (_curLevel > Constants.MaxUpgradeLevel)
            {
                levelText.text = "Max";
                priceText.text = "LVL";
                sprite = SpriteData.Instance.GetUpgradeSprite(upgradeType, Constants.MaxUpgradeLevel);
            }
            else
            {
                _curPrice = UpgradesData.GetPrice(upgradeType, _curLevel);
                priceText.text = _curPrice + " $";
                levelText.text = "LVL " + _curLevel;
                sprite = SpriteData.Instance.GetUpgradeSprite(upgradeType, _curLevel);
            }
            _iconImage.sprite = sprite;
        }
        
        public void BuyItem()
        {
            BuyUpgrade();
        }
        
        public int GetItemNumber()
        {
            return upgradeNumber;
        }
    }
}
