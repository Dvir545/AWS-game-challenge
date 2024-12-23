using System.Collections;
using System.Collections.Generic;
using Enemies;
using Enemies.Demon;
using UI.WarningSign;
using UnityEngine;
using World;

public class SettingsBehaviour : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject saveNQuitButton;
    [SerializeField] private GameObject darkOverlay;
    [SerializeField] private GameObject settingsWindow;
    
    private bool _fromMenu;
    
    public void OpenSettings(bool fromMenu=false)
    {
        if (fromMenu)
        {
            mainMenu.SetActive(false);
            saveNQuitButton.SetActive(false);
        }
        else  // pause game
        {
            darkOverlay.SetActive(true);
            Time.timeScale = 0;
        }
        settingsWindow.SetActive(true);
        _fromMenu = fromMenu;
    }
    
    public void CloseSettings()
    {
        if (_fromMenu)
        {
            mainMenu.SetActive(true);
            saveNQuitButton.SetActive(true);
        }
        else  // resume game
        {
            Time.timeScale = 1;
        }
        darkOverlay.SetActive(false);
        settingsWindow.SetActive(false);
    }

    public void SaveNQuit()
    {
        CloseSettings();
        // game is saved on its own each morning, so only quit to menu
        WarningSignPool.Instance.ReleaseAll();
        EnemyPool.Instance.ReleaseAll();
        BallPool.Instance.ReleaseAll();
        GameEnder.Instance.EndGame();
    }
}