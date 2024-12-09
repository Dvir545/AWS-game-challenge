using UnityEngine;

namespace Stores
{
    public class StoreOpener : MonoBehaviour
    {
       private GeneralStoreManager _generalStoreManager;
        private Collider2D _myPlayer;
        
        private bool _isPlayerInTrigger;
        void Awake()
        {
            _generalStoreManager = FindObjectOfType<GeneralStoreManager>();
            _myPlayer = GameObject.FindGameObjectWithTag("Player").GetComponent<Collider2D>();
        }
    
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_isPlayerInTrigger && other == _myPlayer)
            {
                _generalStoreManager.OpenStore();
                _isPlayerInTrigger = true;
            }
        }
    
        private void OnTriggerExit2D(Collider2D other)
        {
            if (_isPlayerInTrigger && other == _myPlayer)
            {
                _generalStoreManager.CloseStore();
                _isPlayerInTrigger = false;
            }
        }
    }
}
