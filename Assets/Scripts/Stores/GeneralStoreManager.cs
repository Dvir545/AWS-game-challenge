using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Stores
{
    public class GeneralStoreManager : Singleton<GeneralStoreManager>
    {
        [SerializeField] private GameObject cropsStore;
        [SerializeField] private GameObject materialsStore;
        [SerializeField] private GameObject toolsStore;
        [SerializeField] private GameObject upgradesStore;
        private int _darkOverlayOrder = 0;
        private int _windowCanvasOrder = 1;
        private Dictionary<StoreType, GameObject> _storesDarkOverlays;
        private Dictionary<StoreType, GameObject> _storesWindowCanvases;
        private StoreType? _activeStore;
        
        private void Awake()
        {
            _storesDarkOverlays = new Dictionary<StoreType, GameObject>
            {
                [StoreType.Crops] = cropsStore.transform.GetChild(_darkOverlayOrder).gameObject,
                [StoreType.Materials] = materialsStore.transform.GetChild(_darkOverlayOrder).gameObject,
                [StoreType.Tools] = toolsStore.transform.GetChild(_darkOverlayOrder).gameObject,
                [StoreType.Upgrades] = upgradesStore.transform.GetChild(_darkOverlayOrder).gameObject
            };
            _storesWindowCanvases = new Dictionary<StoreType, GameObject>
            {
                [StoreType.Crops] = cropsStore.transform.GetChild(_windowCanvasOrder).gameObject,
                [StoreType.Materials] = materialsStore.transform.GetChild(_windowCanvasOrder).gameObject,
                [StoreType.Tools] = toolsStore.transform.GetChild(_windowCanvasOrder).gameObject,
                [StoreType.Upgrades] = upgradesStore.transform.GetChild(_windowCanvasOrder).gameObject
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
        }

        public void OpenStore(StoreType storeType)
        {
            _storesDarkOverlays[storeType].SetActive(true);
            _storesWindowCanvases[storeType].SetActive(true);
            _activeStore = storeType;
        }
        
        public void CloseStore()
        {
            if (_activeStore == null) return;
            StoreType activeStore = (StoreType)_activeStore;
            _storesDarkOverlays[activeStore].SetActive(false);
            _storesWindowCanvases[activeStore].SetActive(false);
            _activeStore = null;
        }
    }
}