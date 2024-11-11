using UnityEngine;

namespace Stores
{
    public class GeneralStoreManager : MonoBehaviour
    {
        [SerializeField] private GameObject windowCanvas;
        [SerializeField] private GameObject darkOverlay;
        
        private void Start()
        {
            if (darkOverlay != null)
                darkOverlay.SetActive(false);
        }

        public void OpenStore()
        {
            if (darkOverlay != null)
                darkOverlay.SetActive(true);
            windowCanvas.SetActive(true);
        }
        
        public void CloseStore()
        {
            if (darkOverlay != null)
                darkOverlay.SetActive(false);
            windowCanvas.SetActive(false);
        }
    }
}