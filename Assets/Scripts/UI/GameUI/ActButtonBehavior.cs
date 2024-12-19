using Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;
using Utils.Data;

namespace UI.GameUI
{
    public class ActButtonBehavior : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private GameObject iconObject;
        [SerializeField] private PlayerActionManager playerActionManager;
        [SerializeField] private PlayerData playerData;
        [SerializeField] private float pressedOffset = 2f;
        
        private Button _button;
        private Vector3 _originalIconPosition;
        private bool _isPressed;
        private Sprite _normalSprite;
        private Sprite _pressedSprite;
        private Image _buttonImage;
        private Image _iconImage;

        private void Start()
        {
            _button = GetComponent<Button>();
            _buttonImage = GetComponent<Image>();
            _iconImage = iconObject.GetComponent<Image>();
            
            // Cache the button sprites
            _normalSprite = _buttonImage.sprite; // Changed this line
            _pressedSprite = _button.spriteState.pressedSprite;

            if (iconObject != null)
            {
                _originalIconPosition = iconObject.transform.localPosition;
            }
            
            EventManager.Instance.StartListening(EventManager.TowerBuilt, ForciblyReleaseButton);
            EventManager.Instance.StartListening(EventManager.CropHarvested, ForciblyReleaseButton);
            EventManager.Instance.StartListening(EventManager.ActiveToolChanged, ChangeToolIcon);
            ChangeToolIcon(null);
        }

        private void ChangeToolIcon(object arg0)
        {
            var tool = playerData.GetCurTool();
            var level = playerData.GetToolLevel(tool);
            _iconImage.sprite = SpriteData.Instance.GetToolSprite(tool, level, outline: false);
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
