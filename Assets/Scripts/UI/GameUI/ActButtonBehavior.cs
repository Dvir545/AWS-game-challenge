using System;
using Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Utils;
using Utils.Data;
using World;

namespace UI.GameUI
{
    public class ActButtonBehavior : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private GameObject iconObject;
        [SerializeField] private PlayerActionManager playerActionManager;
        [SerializeField] private PlayerData playerData;
        [SerializeField] private float pressedOffset = 2f;
        [SerializeField] private InputActionReference actAction;

        private Button _button;
        private bool _isPressed;
        private Sprite _normalSprite;
        private Sprite _pressedSprite;
        private Image _buttonImage;
        private Image _iconImage;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _buttonImage = GetComponent<Image>();
            _iconImage = iconObject.GetComponent<Image>();
            actAction.action.Enable();
            actAction.action.performed += OnActPerformed;
            actAction.action.canceled += OnActCanceled;
        }
        
        private void OnDestroy()
        {
            // Clean up the subscriptions when the object is destroyed
            if (actAction != null)
            {
                actAction.action.performed -= OnActPerformed;
                actAction.action.canceled -= OnActCanceled;
            }
        }
        
        private void OnActPerformed(InputAction.CallbackContext context)
        {
            PressButton();
            _buttonImage.sprite = _pressedSprite;
        }

        private void OnActCanceled(InputAction.CallbackContext context)
        {
            ReleaseButton();
        }

        private void Start()
        {
            
            // Cache the button sprites
            _normalSprite = _buttonImage.sprite;
            _pressedSprite = _button.spriteState.pressedSprite;

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

        // private void Update()
        // {
        //     // if (Input.GetButtonDown("Attack"))
        //     {
        //         PressButton();
        //         _buttonImage.sprite = _pressedSprite;
        //     }
        //     // else if (Input.GetButtonUp("Attack"))
        //     {
        //         ReleaseButton(); }
        // }

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
                var localPosition = iconObject.transform.localPosition;
                localPosition = new Vector2(localPosition.x, localPosition.y-pressedOffset);
                iconObject.transform.localPosition = localPosition;
                _button.onClick.Invoke();
                playerActionManager.StartActing();
            }
        }

        private void ReleaseButton()
        {
            if (_isPressed)
            {
                _isPressed = false;
                var localPosition = iconObject.transform.localPosition;
                iconObject.transform.localPosition = new Vector2(localPosition.x, localPosition.y+pressedOffset);
                playerActionManager.StopActing();
                _buttonImage.sprite = _normalSprite;
            }
        }

        public void Init()
        {
            if (_button is null)
            {
                _button = GetComponent<Button>();
                _buttonImage = GetComponent<Image>();
                _iconImage = iconObject.GetComponent<Image>();
            }
            ReleaseButton();
            ChangeToolIcon(null);
        }
    }
}
