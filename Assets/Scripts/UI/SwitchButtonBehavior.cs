using System;
using Player;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Utils;

namespace Clickables
{
    public class SwitchButtonBehavior : MonoBehaviour
    {
        [SerializeField] private Image actIcon;
        [SerializeField] private Sprite[] icons;
        [FormerlySerializedAs("playerAction")] [SerializeField] private PlayerActionManager playerActionManager;
        [SerializeField] private PlayerData playerData;
        
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
            playerData.SwitchTool();
            actIcon.sprite = playerData.GetCurTool().GetToolSprite(icons);
            playerActionManager.SwitchActing();
        }
    }
}
