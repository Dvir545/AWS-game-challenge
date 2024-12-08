using Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Utils;

namespace Clickables
{
    public class ActButtonBehavior : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private GameObject iconObject;
        [FormerlySerializedAs("playerAction")] [SerializeField] private PlayerActionManager playerActionManager;
        [SerializeField] private float pressedOffset = 2f;
        
        private Button _button;
        private Vector3 _originalIconPosition;
        private bool _isPressed = false;
        private Sprite _normalSprite;
        private Sprite _pressedSprite;
        private Image _buttonImage;

        private void Start()
        {
            _button = GetComponent<Button>();
            _buttonImage = GetComponent<Image>();
            
            // Cache the button sprites
            _normalSprite = _buttonImage.sprite; // Changed this line
            _pressedSprite = _button.spriteState.pressedSprite;

            if (iconObject != null)
            {
                _originalIconPosition = iconObject.transform.localPosition;
            }
            
            EventManager.Instance.StartListening(EventManager.TowerBuilt, ForciblyReleaseButton);
            EventManager.Instance.StartListening(EventManager.CropHarvested, ForciblyReleaseButton);
        }

        private void ForciblyReleaseButton(object arg0)
        {
            ReleaseButton();
        }

        private void Update()
        {
            if (Input.GetButtonDown("Attack"))
            {
                PressButton();
                _buttonImage.sprite = _pressedSprite;
            }
            else if (Input.GetButtonUp("Attack"))
            {
                ReleaseButton(); }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            PressButton();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            ReleaseButton();
        }

        private void PressButton()
        {
            if (_button.interactable && !_isPressed)
            {
                _isPressed = true;
                Vector3 pressedPosition = _originalIconPosition + new Vector3(0, -pressedOffset, 0);
                iconObject.transform.localPosition = pressedPosition;
                _button.onClick.Invoke();
                playerActionManager.StartActing();
            }
        }

        private void ReleaseButton()
        {
            if (_isPressed)
            {
                _isPressed = false;
                iconObject.transform.localPosition = _originalIconPosition;
                playerActionManager.StopActing();
                _buttonImage.sprite = _normalSprite;
            }
        }
    }
}
