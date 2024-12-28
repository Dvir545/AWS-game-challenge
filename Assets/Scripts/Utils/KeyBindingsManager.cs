using System;
using Stores;
using UI.GameUI;
using UnityEngine;
using World;

namespace Utils
{
    public class KeyBindingsManager: Singleton<KeyBindingsManager>
    {
        private KeyCode _minimapKey = KeyCode.M;
        private KeyCode _settingsKey = KeyCode.Escape;
        private KeyCode _skipToNightKey = KeyCode.K;
        private KeyCode _buyItem1 = KeyCode.Alpha1;
        private KeyCode _buyItem2 = KeyCode.Alpha2;
        private KeyCode _buyItem3 = KeyCode.Alpha3;
        private KeyCode _buyItem4 = KeyCode.Alpha4;
        private KeyCode _buyItem5 = KeyCode.Alpha5;
        private void Update()
        {
            if (Input.GetKeyDown(_minimapKey))
            {
                MinimapBehaviour.Instance.ToggleMap();
            }
            if (Input.GetKeyDown(_settingsKey))
            {
                SettingsBehaviour.Instance.ToggleSettings();
            }
            if (Input.GetKeyDown(_skipToNightKey))
            {
                DayNightManager.Instance.JumpToNight();
            }
            if (Input.GetKeyDown(_buyItem1))
            {
                GeneralStoreManager.Instance.BuyItem(0);
            }
            if (Input.GetKeyDown(_buyItem2))
            {
                GeneralStoreManager.Instance.BuyItem(1);
            }
            if (Input.GetKeyDown(_buyItem3))
            {
                GeneralStoreManager.Instance.BuyItem(2);
            }
            if (Input.GetKeyDown(_buyItem4))
            {
                GeneralStoreManager.Instance.BuyItem(3);
            }
            if (Input.GetKeyDown(_buyItem5))
            {
                GeneralStoreManager.Instance.BuyItem(4);
            }
        }
    }
}