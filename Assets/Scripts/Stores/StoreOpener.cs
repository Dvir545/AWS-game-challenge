using UnityEngine;

namespace Stores
{
    public class StoreOpener : MonoBehaviour
    {
        [SerializeField] private GeneralStoreManager generalStoreManager;
        [SerializeField] private Collider2D myPlayer;
        private Collider2D  _collider;
        
        private bool _isPlayerInTrigger;
        void Start()
        {
            _collider = GetComponent<Collider2D>();
        }
    
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_isPlayerInTrigger && other == myPlayer)
            {
                generalStoreManager.OpenStore();
                _isPlayerInTrigger = true;
            }
        }
    
        private void OnTriggerExit2D(Collider2D other)
        {
            if (_isPlayerInTrigger && other == myPlayer)
            {
                generalStoreManager.CloseStore();
                _isPlayerInTrigger = false;
            }
        }
    }
}
