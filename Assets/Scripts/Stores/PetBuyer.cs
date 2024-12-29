using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.Data;
using World;

namespace Stores
{
    public class PetBuyer : MonoBehaviour, IBuyable
    {
        [SerializeField] private int petNumber;
        [SerializeField] private PlayerData playerData;
        [SerializeField] private Pet pet;
        [SerializeField] private TextMeshProUGUI priceText;
        private Image _iconImage;
        private int _curPrice;
        private int _curIndex;

        private void Awake()
        {
            _iconImage = transform.GetChild(0).GetComponent<Image>();
        }

        public void Init()
        {
            if (_iconImage == null)
                _iconImage = transform.GetChild(0).GetComponent<Image>();
            _curIndex = GameData.Instance.pets.Count % PetsData.GetCount();
            _curPrice = PetsData.GetPrice(pet, _curIndex);
            UpdateIndex(GameData.Instance.pets.Count);
        }

        public void BuyPet()
        {
            bool bought = false;
            if (GameData.Instance.cash < _curPrice)
            {
                Debug.Log("Not enough cash");
            }
            else
            {
                playerData.SpendCash(_curPrice);
                playerData.AddPet(pet, _curIndex);
                UpdateIndex(_curIndex + 1);
                bought = true;
            }
            if (!bought)
                SoundManager.Instance.CantPurchase();
        }

        private void UpdateIndex(int index)
        {
            _curIndex = index % PetsData.GetCount();
            _curPrice = PetsData.GetPrice(pet, _curIndex);
            priceText.text = _curPrice + " $";
            _iconImage.color = PetsData.GetColor(pet, _curIndex);
        }
        
        public void BuyItem()
        {
            BuyPet();
        }
        
        public int GetItemNumber()
        {
            return petNumber;
        }
    }
}
