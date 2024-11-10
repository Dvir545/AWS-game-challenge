using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Amazon.CognitoIdentity;
using Amazon.Runtime;
using Amazon.Util;
using Amazon.Runtime.Internal.Auth;

public class GameLiftClientManager : MonoBehaviour
{
    private static GameLiftClientManager _instance;
    public static GameLiftClientManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("GameLiftClientManager");
                _instance = go.AddComponent<GameLiftClientManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    // AWS Configuration - Replace these with your values
    private const string IDENTITY_POOL_ID = "us-east-1:f163ac26-b96c-43bf-9d2a-86c8f91f3ca6";
    private const string AWS_REGION = "us-east-1";
    private const string GAME_API_URL = "https://g2pad9a2x9.execute-api.us-east-1.amazonaws.com/game";
    private const string NAME_API_URL = "https://zemr70uloh.execute-api.us-east-1.amazonaws.com/names";

    // Events
    public event Action<int> OnPlayerCountChanged;
    public event Action<string> OnGameCodeReceived;
    public event Action<string> OnPlayerNameReceived;
    public event Action<string> OnError;

    private CognitoAWSCredentials _credentials;
    private string _identityId;
    private string _playerName;
    private bool _isInitialized = false;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAWS();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private async void InitializeAWS()
    {
        try
        {
            Debug.Log("Initializing AWS credentials...");
            _credentials = new CognitoAWSCredentials(
                IDENTITY_POOL_ID,
                Amazon.RegionEndpoint.GetBySystemName(AWS_REGION)
            );

            _identityId = await _credentials.GetIdentityIdAsync();
            Debug.Log($"Got identity ID: {_identityId}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error initializing AWS: {e}");
            OnError?.Invoke("Failed to initialize AWS services");
        }
        StartCoroutine(GetPlayerNameCoroutine());
        Debug.Log(_playerName);
    }

    public IEnumerator GetPlayerNameCoroutine()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(NAME_API_URL))
        {
            // Add error handling for credentials
            try 
            {
                var credentials = _credentials.GetCredentials();
                if (credentials != null)
                {
                    www.SetRequestHeader("Authorization", credentials.Token);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Could not get credentials: {e.Message}");
            }

            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                try 
                {
                    var response = JsonUtility.FromJson<NameResponse>(www.downloadHandler.text);
                    _playerName = response.guestName;
                    Debug.Log($"Got player name: {_playerName}");
                    OnPlayerNameReceived?.Invoke(_playerName);
                    _isInitialized = true;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error parsing name response: {e.Message}");
                    // Fallback name generation
                    _playerName = $"Guest_{UnityEngine.Random.Range(1000, 9999)}";
                    OnPlayerNameReceived?.Invoke(_playerName);
                    _isInitialized = true;
                }
            }
            else
            {
                Debug.LogError($"Error getting player name: {www.error}");
                Debug.LogError($"Response Code: {www.responseCode}");
                Debug.LogError($"Response: {www.downloadHandler?.text}");
                
                // Fallback name generation
                _playerName = $"Guest_{UnityEngine.Random.Range(1000, 9999)}";
                OnPlayerNameReceived?.Invoke(_playerName);
                _isInitialized = true;
            }
        }
        // CreateGameCoroutine();
    }

    public void CreateGame()
    {
        if (!_isInitialized)
        {
            OnError?.Invoke("AWS services not yet initialized");
            return;
        }
        StartCoroutine(CreateGameCoroutine());
        // StartCoroutine(GetPlayerNameCoroutine());
    }

    public void JoinGame(string gameCode)
    {
        if (!_isInitialized)
        {
            OnError?.Invoke("AWS services not yet initialized");
            return;
        }
        if (string.IsNullOrEmpty(gameCode))
        {
            OnError?.Invoke("Game code cannot be empty");
            return;
        }
        StartCoroutine(JoinGameCoroutine(gameCode));
    }

    private IEnumerator CreateGameCoroutine()
    {
        Debug.Log("Creating game...");
        var request = new CreateGameRequest
        {
            opCode = "2",
            playerId = _identityId,
            playerName = _playerName
        };

        string jsonRequest = JsonUtility.ToJson(request);
        Debug.Log($"Create game request: {jsonRequest}");

        using (UnityWebRequest www = UnityWebRequest.PostWwwForm($"{GAME_API_URL}/game", jsonRequest))
        {
            www.SetRequestHeader("Content-Type", "application/json");
            
            var credentials = _credentials.GetCredentials();
            if (credentials != null)
            {
                www.SetRequestHeader("Authorization", credentials.Token);
            }

            yield return www.SendWebRequest();

            ProcessWebRequest(www, (result) =>
            {
                var response = JsonUtility.FromJson<CreateGameResponse>(result);
                OnGameCodeReceived?.Invoke(response.gameCode);
                Debug.Log($"Game created with code: {response.gameCode}");
            });
        }
    }

    private IEnumerator JoinGameCoroutine(string gameCode)
    {
        Debug.Log($"Joining game with code: {gameCode}");
        var request = new JoinGameRequest
        {
            opCode = "3",
            gameCode = gameCode,
            playerId = _identityId,
            playerName = _playerName
        };

        string jsonRequest = JsonUtility.ToJson(request);
        Debug.Log($"Join game request: {jsonRequest}");

        using (UnityWebRequest www = UnityWebRequest.PostWwwForm($"{GAME_API_URL}/game", jsonRequest))
        {
            www.SetRequestHeader("Content-Type", "application/json");
            
            var credentials = _credentials.GetCredentials();
            if (credentials != null)
            {
                www.SetRequestHeader("Authorization", credentials.Token);
            }

            yield return www.SendWebRequest();

            ProcessWebRequest(www, (result) =>
            {
                var response = JsonUtility.FromJson<JoinGameResponse>(result);
                OnPlayerCountChanged?.Invoke(response.CurrentPlayerSessionCount);
                Debug.Log($"Joined game session");
            });
        }
    }

    private void ProcessWebRequest(UnityWebRequest www, Action<string> onSuccess)
    {
        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"Request successful. Response: {www.downloadHandler.text}");
            onSuccess?.Invoke(www.downloadHandler.text);
        }
        else
        {
            Debug.LogError($"Request failed: {www.error}");
            Debug.LogError($"Response Code: {www.responseCode}");
            Debug.LogError($"Response Text: {www.downloadHandler?.text}");
            OnError?.Invoke($"Request failed: {www.error}");
        }
    }

    [Serializable]
    private class NameResponse
    {
        public string guestName;
    }

    [Serializable]
    private class CreateGameRequest
    {
        public string opCode;
        public string playerId;
        public string playerName;
    }

    [Serializable]
    private class JoinGameRequest
    {
        public string opCode;
        public string gameCode;
        public string playerId;
        public string playerName;
    }

    [Serializable]
    private class CreateGameResponse
    {
        public string gameCode;
        public string GameSessionId;
    }

    [Serializable]
    private class JoinGameResponse
    {
        public string GameSessionId;
        public int CurrentPlayerSessionCount;
    }
}