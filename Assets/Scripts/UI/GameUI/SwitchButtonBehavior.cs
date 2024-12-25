using Player;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.Data;
using World;

namespace UI.GameUI
{
    public class SwitchButtonBehavior : MonoBehaviour
    {
        [SerializeField] private Image actIcon;
        [SerializeField] private PlayerActionManager playerActionManager;
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
            playerActionManager.SwitchActing();
            SoundManager.Instance.ShortButton();
            EventManager.Instance.TriggerEvent(EventManager.ActiveToolChanged, null);
        }
    }
}
