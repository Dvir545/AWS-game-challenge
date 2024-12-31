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
            if (Input.GetKeyDown(KeyCode.Q) || Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                OnClick(next: false);
            }
            else if (Input.GetKeyDown(KeyCode.E) || Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                OnClick(next: true);
            }
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(Click);
        }
        
        private void OnDisable()
        {
            _button.onClick.RemoveListener(Click);
        }
        
        private void Click() => OnClick();
        
        private void OnClick(bool next=true)
        {
            playerData.SwitchTool(next);
            playerActionManager.SwitchActing();
            SoundManager.Instance.ShortButton();
            EventManager.Instance.TriggerEvent(EventManager.ActiveToolChanged, null);
        }
    }
}
