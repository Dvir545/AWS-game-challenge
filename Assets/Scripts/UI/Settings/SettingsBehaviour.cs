using System.Collections;
using System.Collections.Generic;
using Enemies;
using Enemies.Demon;
using UI.Settings;
using UI.WarningSign;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.Data;
using World;

public class SettingsBehaviour : Singleton<SettingsBehaviour>
{
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject entry;
    [SerializeField] private GameObject saveNQuitButton;
    [SerializeField] private GameObject disconnectButton;
    [SerializeField] private GameObject darkOverlay;
    [SerializeField] private GameObject settingsEnterButton;
    [SerializeField] private GameObject settingsExitButton;
    [SerializeField] private GameObject settingsWindow;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private RightLeftHandedBehaviour rightLeftHandedBehaviour;

    private bool _isOpen;
    private bool _fromMenu;

    public void Init()
    {
        rightLeftHandedBehaviour.Init();
        sfxVolumeSlider.value = GameStatistics.Instance.sfxVolume;
        musicVolumeSlider.value = GameStatistics.Instance.musicVolume;
    }
    
    public void OpenSettings(bool fromMenu=false)
    {
        sfxVolumeSlider.value = GameStatistics.Instance.sfxVolume;
        musicVolumeSlider.value = GameStatistics.Instance.musicVolume;
        if (fromMenu)
        {
            mainMenu.SetActive(false);
            saveNQuitButton.SetActive(false);
            // disconnectButton.SetActive(true);  todo restore this line
        }
        else  // pause game
        {
            darkOverlay.SetActive(true);
            settingsEnterButton.SetActive(false);
            settingsExitButton.SetActive(true);
            saveNQuitButton.SetActive(true);
            disconnectButton.SetActive(false);
            Time.timeScale = 0;
            SoundManager.Instance.PauseBackgroundSong();
        }
        settingsWindow.SetActive(true);
        _fromMenu = fromMenu;
        _isOpen = true;
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
            SoundManager.Instance.ResumeBackgroundSong();
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

    public void SaveNQuit()
    {
        CloseSettings();
        // game is saved on its own each morning, so only quit to menu
        WarningSignPool.Instance.ReleaseAll();
        EnemyPool.Instance.ReleaseAll();
        BallPool.Instance.ReleaseAll();
        SoundManager.Instance.PauseBackgroundSong(0f);
        GameEnder.Instance.EndGame(died: false);
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
        GameStatistics.Instance.SetSfxVolume(volume);
    }
    
    public void ChangeMusicVolume(float volume)
    {
        GameStatistics.Instance.SetMusicVolume(volume);
        SoundManager.Instance.SyncMusicVolume();
    }
}
