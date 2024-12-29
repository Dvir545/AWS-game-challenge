using System.Collections.Generic;
using UnityEngine;
using Utils;
using Utils.Data;
using World;

namespace Stores
{
    public class GeneralStoreManager : Singleton<GeneralStoreManager>
    {
        [SerializeField] private GameObject cropsStore;
        [SerializeField] private GameObject materialsStore;
        [SerializeField] private GameObject toolsStore;
        [SerializeField] private GameObject upgradesStore;
        [SerializeField] private GameObject petsStore;
        private int _darkOverlayOrder = 0;
        private int _windowCanvasOrder = 1;
        private Dictionary<StoreType, GameObject> _storesDarkOverlays;
        private Dictionary<StoreType, GameObject> _storesWindowCanvases;
        private StoreType? _activeStore;
        public bool IsStoreOpen => _activeStore != null;
        [SerializeField] private CropBuyer[] cropItems;
        [SerializeField] private MaterialBuyer[] materialItems;
        [SerializeField] private ToolBuyer[] toolItems;
        [SerializeField] private UpgradeBuyer[] upgradeItems;
        [SerializeField] private PetBuyer[] petItems;
        private IBuyable[] _activeItems;
        
        private void Awake()
        {
            _storesDarkOverlays = new Dictionary<StoreType, GameObject>
            {
                [StoreType.Crops] = cropsStore.transform.GetChild(_darkOverlayOrder).gameObject,
                [StoreType.Materials] = materialsStore.transform.GetChild(_darkOverlayOrder).gameObject,
                [StoreType.Tools] = toolsStore.transform.GetChild(_darkOverlayOrder).gameObject,
                [StoreType.Upgrades] = upgradesStore.transform.GetChild(_darkOverlayOrder).gameObject,
                [StoreType.Pets] = petsStore.transform.GetChild(_darkOverlayOrder).gameObject
            };
            _storesWindowCanvases = new Dictionary<StoreType, GameObject>
            {
                [StoreType.Crops] = cropsStore.transform.GetChild(_windowCanvasOrder).gameObject,
                [StoreType.Materials] = materialsStore.transform.GetChild(_windowCanvasOrder).gameObject,
                [StoreType.Tools] = toolsStore.transform.GetChild(_windowCanvasOrder).gameObject,
                [StoreType.Upgrades] = upgradesStore.transform.GetChild(_windowCanvasOrder).gameObject,
                [StoreType.Pets] = petsStore.transform.GetChild(_windowCanvasOrder).gameObject
            };
        }

        private void Start()
        {
            foreach (var darkOverlay in _storesDarkOverlays.Values)
            {
                darkOverlay.SetActive(false);
            }

            foreach (var windowCanvas in _storesWindowCanvases.Values)
            {
                windowCanvas.SetActive(false);
            }
            EventManager.Instance.StartListening(EventManager.DayEnded, CloseStore);
        }

        private void CloseStore(object arg0)
        {
            CloseStore();
        }

        public void OpenStore(StoreType storeType)
        {
            if (!DayNightManager.Instance.DayTime) return;
            _storesDarkOverlays[storeType].SetActive(true);
            _storesWindowCanvases[storeType].SetActive(true);
            _activeStore = storeType;
            _activeItems = storeType switch
            {
                StoreType.Crops => cropItems,
                StoreType.Materials => materialItems,
                StoreType.Tools => toolItems,
                StoreType.Upgrades => upgradeItems,
                StoreType.Pets => petItems,
                _ => null
            };
        }
        
        public void CloseStore()
        {
            if (_activeStore == null) return;
            StoreType activeStore = (StoreType)_activeStore;
            _storesDarkOverlays[activeStore].SetActive(false);
            _storesWindowCanvases[activeStore].SetActive(false);
            _activeStore = null;
            _activeItems = null;
        }

        public void UpdateStock(int[] newCrops, int[] newMaterials)
        {
            for (int i = 0; i < newCrops.Length; i++)
            {
                GameData.Instance.cropsInStore[i] = Mathf.Min(GameData.Instance.cropsInStore[i] + newCrops[i], Constants.MaxCropsInStore);
            }
            for (int i = 0; i < newMaterials.Length; i++)
            {
                GameData.Instance.materialsInStore[i] = Mathf.Min(GameData.Instance.materialsInStore[i] + newMaterials[i], Constants.MaxMaterialsInStore);
            }
        }
        
        public void BuyItem(int itemNumber)
        {
            if (_activeItems == null) return;
            if (itemNumber < 0 || itemNumber >= _activeItems.Length) return;
            _activeItems[itemNumber].BuyItem();
        }
    }
}