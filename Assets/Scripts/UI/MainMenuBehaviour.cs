using TMPro;
using UnityEngine;
using Utils;
using Utils.Data;

public class MainMenuBehaviour : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI welcomeMessage;

    private void OnEnable()
    {
        if (GameStatistics.Instance != null)
        {
            GameStatistics.Instance.OnGameDataLoaded += UpdateWelcomeMessage;
        }
        if (EventManager.Instance != null)
        {
            EventManager.Instance.StartListening(EventManager.Disconnect, OnDisconnect);
        }
        UpdateWelcomeMessage();
    }

    private void OnDisable()
    {
        if (GameStatistics.Instance != null)
        {
            GameStatistics.Instance.OnGameDataLoaded -= UpdateWelcomeMessage;
        }
        if (EventManager.Instance != null)
        {
            EventManager.Instance.StopListening(EventManager.Disconnect, OnDisconnect);
        }
    }

    private void UpdateWelcomeMessage()
    {
        if (welcomeMessage != null)
        {
            string username = GameStatistics.Instance?.username;
            if (!string.IsNullOrEmpty(username))
            {
                welcomeMessage.text = "WELCOME, " + username + "!";
            }
            else
            {
                welcomeMessage.text = "WELCOME!";
            }
        }
    }

    private void OnDisconnect(object _)
    {
        if (welcomeMessage != null)
        {
            welcomeMessage.text = "WELCOME!";
        }
    }
}