using TMPro;
using UnityEngine;

namespace MultiLobby
{
    public class MultiLobbyBehavior : MonoBehaviour
    {
        [SerializeField] private GameObject startButton;
        [SerializeField] private TextMeshProUGUI numPlayers;
        [SerializeField] private TextMeshProUGUI joinCode;
        [SerializeField] private TextMeshProUGUI errorText;
        [SerializeField] private TextMeshProUGUI playerNameText;    
        public static bool IsHost;
        public static string JoinCode;
        private int _currentPlayers = 1;

        private GameLiftClientManager _gameLiftClient;

        public void Start()
        {
            _gameLiftClient = GameLiftClientManager.Instance;

            // Subscribe to events
            _gameLiftClient.OnPlayerCountChanged += UpdatePlayerCount;
            _gameLiftClient.OnGameCodeReceived += UpdateGameCode;
            _gameLiftClient.OnError += ShowError;
            _gameLiftClient.OnPlayerNameReceived += UpdatePlayerName;

            SetHost();

            if (IsHost)
            {
                _gameLiftClient.CreateGame();
            }
        }

        private void OnDestroy()
        {
            if (_gameLiftClient != null)
            {
                _gameLiftClient.OnPlayerCountChanged -= UpdatePlayerCount;
                _gameLiftClient.OnGameCodeReceived -= UpdateGameCode;
                _gameLiftClient.OnError -= ShowError;
                _gameLiftClient.OnPlayerNameReceived -= UpdatePlayerName;
            }
        }

        private void UpdatePlayerName(string name)
        {
            playerNameText.text = $"Playing as: {name}";
        }

        private void UpdatePlayerCount(int count)
        {
            _currentPlayers = count;
            numPlayers.text = count.ToString();
        }

        private void UpdateGameCode(string code)
        {
            JoinCode = code;
            joinCode.text = code;
        }

        private void ShowError(string message)
        {
            errorText.text = message;
            errorText.gameObject.SetActive(true);
            Invoke(nameof(HideError), 3f);
        }

        private void HideError()
        {
            errorText.gameObject.SetActive(false);
        }

        private void SetHost()
        {
            startButton.SetActive(IsHost);
            if (IsHost)
            {
                joinCode.transform.parent.gameObject.SetActive(true);
            }
        }

        public void JoinGameWithCode(string code)
        {
            _gameLiftClient.JoinGame(code);
        }
    }
}