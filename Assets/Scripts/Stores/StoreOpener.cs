using UnityEngine;
using Utils;

namespace Stores
{
    public class StoreOpener : MonoBehaviour
    {
        [SerializeField] private StoreType storeType;
        private Collider2D _myPlayer;
        
        private bool _isPlayerInTrigger;
        void Awake()
        {
            _myPlayer = GameObject.FindGameObjectWithTag("Player").GetComponent<Collider2D>();
        }
    
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_isPlayerInTrigger && other == _myPlayer)
            {
                GeneralStoreManager.Instance.OpenStore(storeType);
                _isPlayerInTrigger = true;
            }
        }
    
        private void OnTriggerExit2D(Collider2D other)
        {
            if (_isPlayerInTrigger && other == _myPlayer)
            {
                GeneralStoreManager.Instance.CloseStore();
                _isPlayerInTrigger = false;
            }
        }
    }
}
