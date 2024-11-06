using System;
using TMPro;
using UnityEngine;

namespace MultiLobby
{
    public class MultiLobbyBehavior: MonoBehaviour
    {
        [SerializeField] private GameObject startButton;
        [SerializeField] private TextMeshProUGUI numPlayers;
        [SerializeField] private TextMeshProUGUI joinCode;
        public static bool IsHost;
        private int _currentPlayers = 1;  // DVIR - change this number from server
        private string _joinCode; // DVIR - change this number from server
        public void Start()
        {
            SetHost();
            _joinCode = "zzzz"; // DVIR - get join code from server
            joinCode.text = _joinCode;
        }

        public void Update()
        {
            int players = 2; // DVIR - if any change received from server, update _currentPlayers
            if (players != _currentPlayers)
            {
                _currentPlayers = players;
                numPlayers.text = $"{_currentPlayers}";
            }
        }

        private void SetHost()
        {
            if (IsHost)
            {
                startButton.SetActive(true);
            } else
            {
                startButton.SetActive(false);
            }
        }
    }
}