using System;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Clickables
{
    public class SwitchButtonBehavior : MonoBehaviour
    {
        [SerializeField] private Image actIcon;
        [SerializeField] private Sprite[] icons;
        
        private Button _button;
        
        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        private void Update()
        {
            // also click on key press "Switch"
            if (Input.GetButtonDown("Switch"))
            {
                OnClick();
            }
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(OnClick);
        }
        
        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnClick);
        }
        
        private void OnClick()
        {
            GameData.SwitchTool();

            actIcon.sprite = GameData.GetCurTool().GetToolSprite(icons);
        }
    }
}
