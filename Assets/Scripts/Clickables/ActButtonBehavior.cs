using Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Clickables
{
    public class ActButtonBehavior : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private GameObject iconObject;
        [SerializeField] private PlayerAction playerAction;
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
                ReleaseButton();
                _buttonImage.sprite = _normalSprite;
            }
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
                playerAction.StartActing();
            }
        }

        private void ReleaseButton()
        {
            if (_isPressed)
            {
                _isPressed = false;
                iconObject.transform.localPosition = _originalIconPosition;
                playerAction.StopActing();
            }
        }
    }
}
