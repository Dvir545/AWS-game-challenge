using System.Collections;
using System.Collections.Generic;
using Enemies;
using Enemies.Demon;
using Player;
using TMPro;
using UI.Settings;
using UI.WarningSign;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using Utils;
using Utils.Data;
using World;

public class SettingsBehaviour : Singleton<SettingsBehaviour>
{
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject entry;
    [SerializeField] private GameObject game;
    [SerializeField] private GameObject saveNQuitButton;
    [SerializeField] private GameObject disconnectButton;
    [SerializeField] private GameObject controlsButton;
    [SerializeField] private GameObject darkOverlay;
    [SerializeField] private GameObject settingsEnterButton;
    [SerializeField] private GameObject settingsExitButton;
    [SerializeField] private GameObject settingsWindow;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private RightLeftHandedBehaviour rightLeftHandedBehaviour;
    [SerializeField] private TextMeshProUGUI daysCountText;
    
    [SerializeField] private GameObject settingsParent;
    [SerializeField] private GameObject controlsParent;
    [SerializeField] private GameObject controlsMobile;
    [SerializeField] private GameObject controlsPC;
    [SerializeField] private PlayerHealthManager playerHealthManager;
    [SerializeField] private Light2D globalLight;

    private bool _isOpen;
    private bool _fromMenu;
    
    public void Init()
    {
        rightLeftHandedBehaviour.Init();
        sfxVolumeSlider.value = PlayerPrefs.GetFloat(Constants.SFXVolumePlayerPref, 0.5f);
        musicVolumeSlider.value = PlayerPrefs.GetFloat(Constants.MusicVolumePlayerPref, 0.5f);
    }
    
    public void OpenSettings(bool fromMenu=false)
    {
        if (playerHealthManager.IsDead && !mainMenu.activeSelf) return;
        settingsParent.SetActive(true);
        controlsParent.SetActive(false);
        sfxVolumeSlider.value = PlayerPrefs.GetFloat(Constants.SFXVolumePlayerPref, 0.5f);
        musicVolumeSlider.value = PlayerPrefs.GetFloat(Constants.MusicVolumePlayerPref, 0.5f);
        if (fromMenu)
        {
            mainMenu.SetActive(false);
            daysCountText.gameObject.SetActive(false);
            controlsButton.SetActive(false);
            saveNQuitButton.SetActive(false);
            disconnectButton.SetActive(true);
        }
        else
        {
            darkOverlay.SetActive(true);
            daysCountText.gameObject.SetActive(true);
            daysCountText.text = "- DAY " + (GameData.Instance.day + 1) + " -";
            settingsEnterButton.SetActive(false);
            settingsExitButton.SetActive(true);
            controlsButton.SetActive(true);
            saveNQuitButton.SetActive(true);
            disconnectButton.SetActive(false);
            Time.timeScale = 0;  // pause game
            if (game.activeSelf)
                SoundManager.Instance.PauseAllMusic();
        }
        settingsWindow.SetActive(true);
        settingsWindow.transform.localScale = 0.75f * Vector3.one;
        _fromMenu = fromMenu;
        _isOpen = true;
    }
    
    private void Awake()
    {
        // Register pause/resume handlers
        Application.focusChanged += OnFocusChanged;
        
        if (MobileKeyboardManager.Instance.IsMobileDevice())
        {
            controlsMobile.SetActive(true);
            controlsPC.SetActive(false);
        }
        else
        {
            controlsMobile.SetActive(false);
            controlsPC.SetActive(true);
        }
    }
    
    private void OnDestroy()
    {
        Application.focusChanged -= OnFocusChanged;
    }
    
    private void OnFocusChanged(bool hasFocus)
    {
        if (game.activeSelf)
        {
            if (!hasFocus && !_isOpen)
            {
                OpenSettings();
            }
        }
    }
    
    public void CloseSettings()
    {
        if (_fromMenu)
        {
            mainMenu.SetActive(true);
        }
        else  // resume game
        {
            Time.timeScale = 1;
            if (game.activeSelf)
                SoundManager.Instance.ResumeAllMusic();
        }
        darkOverlay.SetActive(false);
        settingsEnterButton.SetActive(true);
        settingsExitButton.SetActive(false);
        settingsWindow.SetActive(false);
        _isOpen = false;
    }
    
    public void ToggleSettings()
    {
        if (_isOpen)
        {
            CloseSettings();
        }
        else
        {
            OpenSettings();
        }
    }
    
    public void OpenControls()
    {
        settingsParent.SetActive(false);
        controlsParent.SetActive(true);
        settingsWindow.transform.localScale = 1.25f * Vector3.one;
    }

    public void BackButton()
    {
        if (controlsParent.activeSelf)
        {
            settingsParent.SetActive(true);
            controlsParent.SetActive(false);
            settingsWindow.transform.localScale = 0.75f * Vector3.one;
        }
        else
        {
            CloseSettings();
        }
    }

    public void SaveNQuit()
    {
        // game is saved on its own each morning, so only quit to menu
        WarningSignPool.Instance.ReleaseAll();
        EnemyPool.Instance.ReleaseAll();
        BallPool.Instance.ReleaseAll();
        GameEnder.Instance.EndGame(died: false);
        DayNightManager.Instance.StopAllLightChanges();
        globalLight.intensity = 1;
        
        CloseSettings();
    }

    public void Disconnect()
    {
        CloseSettings();
        mainMenu.SetActive(false);
        entry.SetActive(true);
        EventManager.Instance.TriggerEvent(EventManager.Disconnect, null);
    }
    
    public void ChangeSFXVolume(float volume)
    {
        PlayerPrefs.SetFloat(Constants.SFXVolumePlayerPref, volume);
    }
    
    public void ChangeMusicVolume(float volume)
    {
        PlayerPrefs.SetFloat(Constants.MusicVolumePlayerPref, volume);
        SoundManager.Instance.SyncMusicVolume();
    }

    public float SFXVolume => sfxVolumeSlider.value;
    public float MusicVolume => musicVolumeSlider.value;
}
