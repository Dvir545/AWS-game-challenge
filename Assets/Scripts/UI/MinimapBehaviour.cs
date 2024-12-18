using UnityEngine;

namespace UI
{
    public class MinimapBehaviour : MonoBehaviour
    {
        private Transform _map;
        private Transform _mapOpenButton;

        private RectTransform _ref1UI;
        private RectTransform _ref2UI;
        private RectTransform _playerUI;
        [SerializeField] private Transform ref1;
        [SerializeField] private Transform ref2;
        [SerializeField] private Transform player;

        private void Awake()
        {
            _map = transform.GetChild(0);
            _mapOpenButton = transform.GetChild(1);
        
            _ref1UI = _map.GetChild(0).GetComponent<RectTransform>();
            _ref2UI = _map.GetChild(1).GetComponent<RectTransform>();
            _playerUI = _map.GetChild(2).GetComponent<RectTransform>();
        }
        
        private void Update()
        {
            // Get the world space positions
            Vector2 worldRef1 = ref1.position;
            Vector2 worldRef2 = ref2.position;
            Vector2 worldPlayer = player.position;

            // Calculate the scale factor between world space and minimap space
            float worldDistance = Vector2.Distance(worldRef1, worldRef2);
            float minimapDistance = Vector2.Distance(_ref1UI.anchoredPosition, _ref2UI.anchoredPosition);
            float scaleFactor = minimapDistance / worldDistance;

            // Calculate the relative position of the player from ref1
            Vector2 relativePos = worldPlayer - worldRef1;

            // Convert the relative position to minimap space
            var anchoredPosition = _ref1UI.anchoredPosition;
            Vector2 minimapPosition = new Vector2(
                anchoredPosition.x + (relativePos.x * scaleFactor),
                anchoredPosition.y + (relativePos.y * scaleFactor)
            );

            // Update the player UI position on the minimap
            _playerUI.anchoredPosition = minimapPosition;
        }

    
        public void OpenMap()
        {
            _map.gameObject.SetActive(true);
            _mapOpenButton.gameObject.SetActive(false);
        }
    
        public void CloseMap()
        {
            _map.gameObject.SetActive(false);
            _mapOpenButton.gameObject.SetActive(true);
        }
    }
}
