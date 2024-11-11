using UnityEngine;

namespace Stores
{
    public class GeneralStoreManager : MonoBehaviour
    {
        [SerializeField] private GameObject windowCanvas;

        public void OpenStore()
        {
            windowCanvas.SetActive(true);
        }
        
        public  void CloseStore()
        {
            windowCanvas.SetActive(false);
        }
    }
}