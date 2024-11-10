using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Amazon.CognitoIdentity;
using Amazon.Runtime;
using System.Text;
using System.Security.Cryptography;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;

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

    // AWS Configuration
    private const string IDENTITY_POOL_ID = "us-east-1:f163ac26-b96c-43bf-9d2a-86c8f91f3ca6";
    private const string AWS_REGION = "us-east-1";
    private const string GAME_API_URL = "https://g2pad9a2x9.execute-api.us-east-1.amazonaws.com/game";
    private const string NAME_API_URL = "https://zemr70uloh.execute-api.us-east-1.amazonaws.com/GET";
    private const string SERVICE = "execute-api";

    // Events
    public event Action<int> OnPlayerCountChanged;
    public event Action<string> OnGameCodeReceived;
    public event Action<string> OnPlayerNameReceived;
    public event Action<string> OnError;

    private CognitoAWSCredentials _credentials;
    private ImmutableCredentials _currentCredentials;
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
            _currentCredentials = await _credentials.GetCredentialsAsync();
            Debug.Log($"Got identity ID: {_identityId}");
            _isInitialized = true;
            StartCoroutine(GetPlayerNameCoroutine());
        }
        catch (Exception e)
        {
            Debug.LogError($"Error initializing AWS: {e}");
            OnError?.Invoke("Failed to initialize AWS services");
        }
    }

    private string SignRequest(string url, string method, string content = "", Dictionary<string, string> additionalHeaders = null)
    {
        if (_currentCredentials == null) return null;

        var requestDate = DateTime.UtcNow;
        var datestamp = requestDate.ToString("yyyyMMdd");
        var amzdate = requestDate.ToString("yyyyMMddTHHmmssZ");
        var uri = new Uri(url);
        var host = uri.Host;
        
        var headers = new Dictionary<string, string>
        {
            {"host", host},
            {"x-amz-date", amzdate},
            {"x-amz-security-token", _currentCredentials.Token}
        };

        if (additionalHeaders != null)
        {
            foreach (var header in additionalHeaders)
            {
                headers[header.Key.ToLower()] = header.Value;
            }
        }

        var signedHeaders = string.Join(";", headers.Keys.OrderBy(k => k));
        var canonicalHeaders = string.Join("\n", headers.OrderBy(k => k.Key).Select(h => $"{h.Key}:{h.Value}")) + "\n";
        var payloadHash = HashSHA256(content);

        var canonicalRequest = $"{method}\n{uri.AbsolutePath}\n{uri.Query.TrimStart('?')}\n{canonicalHeaders}\n{signedHeaders}\n{payloadHash}";
        var credentialScope = $"{datestamp}/{AWS_REGION}/{SERVICE}/aws4_request";
        var stringToSign = $"AWS4-HMAC-SHA256\n{amzdate}\n{credentialScope}\n{HashSHA256(canonicalRequest)}";
        
        var kSecret = Encoding.UTF8.GetBytes($"AWS4{_currentCredentials.SecretKey}");
        var kDate = HmacSHA256(Encoding.UTF8.GetBytes(datestamp), kSecret);
        var kRegion = HmacSHA256(Encoding.UTF8.GetBytes(AWS_REGION), kDate);
        var kService = HmacSHA256(Encoding.UTF8.GetBytes(SERVICE), kRegion);
        var kSigning = HmacSHA256(Encoding.UTF8.GetBytes("aws4_request"), kService);
        var signature = ToHexString(HmacSHA256(Encoding.UTF8.GetBytes(stringToSign), kSigning));

        return $"AWS4-HMAC-SHA256 Credential={_currentCredentials.AccessKey}/{credentialScope}, SignedHeaders={signedHeaders}, Signature={signature}";
    }

    private string HashSHA256(string data)
    {
        using (var sha256 = SHA256.Create())
        {
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
            return ToHexString(bytes);
        }
    }

    private byte[] HmacSHA256(byte[] data, byte[] key)
    {
        using (var hmac = new HMACSHA256(key))
        {
            return hmac.ComputeHash(data);
        }
    }

    private string ToHexString(byte[] bytes)
    {
        return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
    }

    public IEnumerator GetPlayerNameCoroutine()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(NAME_API_URL))
        {
            string authorization = SignRequest(NAME_API_URL, "GET");
            if (authorization != null)
            {
                www.SetRequestHeader("Authorization", authorization);
                www.SetRequestHeader("x-amz-date", DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ"));
                www.SetRequestHeader("x-amz-security-token", _currentCredentials.Token);
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
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error parsing name response: {e.Message}");
                    _playerName = $"Guest_{UnityEngine.Random.Range(1000, 9999)}";
                    OnPlayerNameReceived?.Invoke(_playerName);
                }
            }
            else
            {
                Debug.LogError($"Error getting player name: {www.error}");
                Debug.LogError($"Response Code: {www.responseCode}");
                Debug.LogError($"Response: {www.downloadHandler?.text}");
                
                _playerName = $"Guest_{UnityEngine.Random.Range(1000, 9999)}";
                OnPlayerNameReceived?.Invoke(_playerName);
            }
        }
    }

    public void CreateGame()
    {
        if (!_isInitialized)
        {
            OnError?.Invoke("AWS services not yet initialized");
            return;
        }
        StartCoroutine(CreateGameCoroutine());
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

        using (UnityWebRequest www = new UnityWebRequest($"{GAME_API_URL}/game", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequest);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            
            string authorization = SignRequest(GAME_API_URL + "/game", "POST", jsonRequest, 
                new Dictionary<string, string> { {"Content-Type", "application/json"} });
            
            if (authorization != null)
            {
                www.SetRequestHeader("Authorization", authorization);
                www.SetRequestHeader("x-amz-date", DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ"));
                www.SetRequestHeader("x-amz-security-token", _currentCredentials.Token);
            }
            www.SetRequestHeader("Content-Type", "application/json");

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

        using (UnityWebRequest www = new UnityWebRequest($"{GAME_API_URL}/game", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequest);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            
            string authorization = SignRequest(GAME_API_URL + "/game", "POST", jsonRequest,
                new Dictionary<string, string> { {"Content-Type", "application/json"} });
            
            if (authorization != null)
            {
                www.SetRequestHeader("Authorization", authorization);
                www.SetRequestHeader("x-amz-date", DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ"));
                www.SetRequestHeader("x-amz-security-token", _currentCredentials.Token);
            }
            www.SetRequestHeader("Content-Type", "application/json");

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