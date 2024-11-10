using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

namespace MultiLobby
{
    public class MultiLobbyOpener : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private bool isHost;
        [SerializeField] private TextMeshProUGUI errorMessageText;
        [SerializeField] private TMP_InputField joinCodeInput;
        [SerializeField] private GameObject loadingIndicator;
        [SerializeField] private float errorMessageDuration = 3f;

        [Header("Optional UI")]
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private GameObject joinPanel;
        [SerializeField] private GameObject hostPanel;

        private GameLiftClientManager _gameLiftClient;
        private bool _isProcessingRequest;
        private string _playerName;

        private void Start()
        {
            // Get GameLift client instance
            _gameLiftClient = GameLiftClientManager.Instance;

            // Subscribe to events
            _gameLiftClient.OnError += ShowErrorMessage;
            _gameLiftClient.OnGameCodeReceived += OnGameCodeReceived;
            _gameLiftClient.OnPlayerNameReceived += OnPlayerNameReceived;

            // Initialize UI
            if (errorMessageText != null)
                errorMessageText.gameObject.SetActive(false);
            if (loadingIndicator != null)
                loadingIndicator.SetActive(false);

            // Show appropriate panel
            if (joinPanel != null) joinPanel.SetActive(!isHost);
            if (hostPanel != null) hostPanel.SetActive(isHost);
        }

        private void OnDestroy()
        {
            if (_gameLiftClient != null)
            {
                _gameLiftClient.OnError -= ShowErrorMessage;
                _gameLiftClient.OnGameCodeReceived -= OnGameCodeReceived;
                _gameLiftClient.OnPlayerNameReceived -= OnPlayerNameReceived;
            }
        }

        public void EnterLobby()
        {
            if (_isProcessingRequest)
            {
                Debug.Log("Request already in progress");
                return;
            }

            _isProcessingRequest = true;
            if (loadingIndicator != null)
                loadingIndicator.SetActive(true);

            try
            {
                if (isHost)
                {
                    Debug.Log("Creating host game session...");
                    MultiLobbyBehavior.IsHost = true;
                    _gameLiftClient.CreateGame();
                    // Scene loading will happen after we receive the game code
                }
                else
                {
                    string joinCode = joinCodeInput?.text?.Trim().ToUpper();
                    if (string.IsNullOrEmpty(joinCode))
                    {
                        ShowErrorMessage("Please enter a join code");
                        return;
                    }

                    Debug.Log($"Attempting to join game with code: {joinCode}");
                    MultiLobbyBehavior.IsHost = false;
                    _gameLiftClient.JoinGame(joinCode);
                    MultiLobbyBehavior.JoinCode = joinCode;
                    LoadLobbyScene();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error in EnterLobby: {e}");
                ShowErrorMessage($"Error: {e.Message}");
            }
        }

        private void OnGameCodeReceived(string gameCode)
        {
            Debug.Log($"Received game code: {gameCode}");
            MultiLobbyBehavior.JoinCode = gameCode;
            LoadLobbyScene();
        }

        private void OnPlayerNameReceived(string playerName)
        {
            _playerName = playerName;
            if (playerNameText != null)
            {
                playerNameText.text = $"Playing as: {playerName}";
            }
        }

        private void LoadLobbyScene()
        {
            Debug.Log("Loading lobby scene...");
            if (loadingIndicator != null)
                loadingIndicator.SetActive(false);
            _isProcessingRequest = false;
            SceneManager.LoadScene("MultiLobby");
        }

        public void OnJoinCodeInputChanged(string newValue)
        {
            // Optional: Add validation or formatting here
            if (errorMessageText != null && errorMessageText.gameObject.activeSelf)
            {
                errorMessageText.gameObject.SetActive(false);
            }
        }

        public void OnBackButtonPressed()
        {
            SceneManager.LoadScene("MainMenu");
        }

        private void ShowErrorMessage(string message)
        {
            Debug.LogError($"Error: {message}");
            
            if (errorMessageText != null)
            {
                errorMessageText.text = message;
                errorMessageText.gameObject.SetActive(true);
                StartCoroutine(HideErrorMessage());
            }

            if (loadingIndicator != null)
                loadingIndicator.SetActive(false);
            
            _isProcessingRequest = false;
        }

        private System.Collections.IEnumerator HideErrorMessage()
        {
            yield return new WaitForSeconds(errorMessageDuration);
            if (errorMessageText != null)
                errorMessageText.gameObject.SetActive(false);
        }
    }
}