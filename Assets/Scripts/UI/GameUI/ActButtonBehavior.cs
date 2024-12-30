using System;
using System.Collections.Generic;
using Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
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
        
        PointerEventData click_data;
        List<RaycastResult> click_results;
 
        // void Update()
        // {
        //     if (!GameStarter.Instance.GameStarted) return;
        //     // use isPressed if you wish to ray cast every frame:
        //     if(Mouse.current.leftButton.wasPressedThisFrame)
        //         //
        //         // // use wasReleasedThisFrame if you wish to ray cast just once per click:
        //         // if(Mouse.current.leftButton.wasReleasedThisFrame)
        //     {
        //         GetUiElementsClicked();
        //     }
        // }
 
        void GetUiElementsClicked()
        {
            /** Get all the UI elements clicked, using the current mouse position and raycasting. **/
 
            click_data.position = Mouse.current.position.ReadValue();
            click_results.Clear();

            EventSystem.current.RaycastAll(click_data, click_results);
            
            // only keep button objects
            click_results.RemoveAll(
                result => result.gameObject.GetComponent<Button>() == null && result.gameObject.name != "Window" && result.gameObject.name != "DarkOverlay"
            );
            
        }

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

        private bool _acting;
        
        private void OnActPerformed(InputAction.CallbackContext context)
        {
            if (!playerActionManager.CanAct) return;
            if (Time.timeScale == 0) return;
            // only if button
            if (context.control.device == Mouse.current)
            {
                GetUiElementsClicked();
                if (click_results.Count > 0) return;  // ignore if clicking a ui element
            }
            PressButton();
            _buttonImage.sprite = _pressedSprite;
            _acting = true;
        }

        private void OnActCanceled(InputAction.CallbackContext context)
        {
            if (!_acting) return;
            ReleaseButton();
            _acting = false;
        }

        private void Start()
        {
            click_data = new PointerEventData(EventSystem.current);
            click_results = new List<RaycastResult>();
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
