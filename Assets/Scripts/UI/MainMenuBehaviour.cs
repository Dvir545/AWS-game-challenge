using TMPro;
using UnityEngine;
using Utils.Data;

public class MainMenuBehaviour : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI welcomeMessage;
    
    private void Awake()
    {
        welcomeMessage.text = "Welcome, " + GameStatistics.Instance.username + "!";
    }
}
