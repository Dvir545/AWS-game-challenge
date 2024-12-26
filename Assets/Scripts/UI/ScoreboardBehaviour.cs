using System;
using System.Collections;
using System.Collections.Generic;
using Enemies;
using Enemies.Demon;
using TMPro;
using UI.WarningSign;
using UnityEngine;
using UnityEngine.Networking;
using World;

[Serializable]
public class APIResponse
{
    public int statusCode;
    public Headers headers;
    public string body;
}

[Serializable]
public class Headers
{
    public string Access_Control_Allow_Origin;
    public bool Access_Control_Allow_Credentials;
    public string Content_Type;
}

[Serializable]
public class ScoreData
{
    public List<ScoreItem> scores;
}

[Serializable]
public class ScoreItem
{
    [SerializeField]
    public string playerName;
    [SerializeField]
    public string Days;  // matches the JSON field name
    [SerializeField]
    public float timeTaken;

    public float daysSurvived
    {
        get { return float.Parse(Days); }
    }
}

[Serializable]
public class ScoreRequest
{
    public string playerName;
    public int daysSurvived;
    public float timeTaken;
}

public class ScoreboardBehaviour : MonoBehaviour
{
    [SerializeField] private GameObject scorePrefab;
    [SerializeField] private GameObject scoresParent;
    [SerializeField] private GameObject playerScore;
    private GameObject _darkOverlay;
    private GameObject _window;
    private const float StartY = 92.6f;
    private const float YOffsetBetweenScores = 65;
    private const string FETCH_API_URL = "https://wjfv1q5r9e.execute-api.us-east-1.amazonaws.com/fetch/fetch";
    private const string CREATE_API_URL = "https://jy3dw0v0uh.execute-api.us-east-1.amazonaws.com/create/";
    private const string API_KEY = "eVZBuSzrn113f2bFvQjTZ9tXmNyhHGxU3YcwPmWT";
    private const int MAX_RETRIES = 3;
    private const float RETRY_DELAY = 2f;

    private int _nScores = 0;
    
    private void Awake()
    {
        _darkOverlay = transform.GetChild(0).gameObject;
        _window = transform.GetChild(1).gameObject;
        _darkOverlay.SetActive(false);
        _window.SetActive(false);
        Debug.Log("ScoreboardBehaviour Awake - Initialized");
    }

    private void ClearExistingScores()
    {
        Debug.Log("Clearing existing scores");
        foreach (Transform child in scoresParent.transform)
        {
            if (child.gameObject != playerScore)
            {
                Destroy(child.gameObject);
            }
        }
        _nScores = 0;
    }

    private void AddScore(string playerName, int daysSurvived, float secondsPlayed)
    {
        Debug.Log($"Adding score: Player={playerName}, Days={daysSurvived}, Time={secondsPlayed}");
        var score = Instantiate(scorePrefab, scoresParent.transform);
        SetScore(score, playerName, daysSurvived, secondsPlayed);
        score.transform.localPosition = new Vector3(0, StartY - YOffsetBetweenScores * _nScores, 0);
        _nScores++;
    }

    public void SetPlayerScore(string playerName, int daysSurvived, float secondsPlayed)
    {  
        Debug.Log($"Setting player score: Player={playerName}, Days={daysSurvived}, Time={secondsPlayed}");
        SetScore(playerScore, playerName, daysSurvived, secondsPlayed);
        StartCoroutine(SetNewRecordWithRetry(playerName, daysSurvived, secondsPlayed, 0));
    }

    private void SetScore(GameObject score, string playerName, int daysSurvived, float secondsPlayed)
    {
        var nameText = score.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        nameText.text = playerName;
        var daysText = score.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        daysText.text = daysSurvived.ToString();
        var timeText = score.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        TimeSpan time = TimeSpan.FromSeconds(secondsPlayed);
        timeText.text = time.Hours > 0 
            ? $"{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2}"
            : $"{time.Minutes:D2}:{time.Seconds:D2}";
        Debug.Log($"Score set: Name={playerName}, Days={daysSurvived}, Time={timeText.text}");
    }

    private IEnumerator SetNewRecordWithRetry(string playerName, int daysSurvived, float secondsPlayed, int retryCount)
    {
        Debug.Log($"Starting SetNewRecordWithRetry - Attempt {retryCount + 1}/{MAX_RETRIES}");
        UnityWebRequest request = null;

        try
        {
            var requestData = new ScoreRequest
            {
                playerName = playerName,
                daysSurvived = daysSurvived,
                timeTaken = secondsPlayed
            };

            string jsonBody = JsonUtility.ToJson(requestData);
            Debug.Log($"Request URL: {CREATE_API_URL}");
            Debug.Log($"Request Body: {jsonBody}");

            request = new UnityWebRequest(CREATE_API_URL, "POST");
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error creating record request: {e.Message}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
            if (request != null)
                request.Dispose();
            yield break;
        }

        using (request)
        {
            Debug.Log("Sending record request...");
            yield return request.SendWebRequest();
            Debug.Log($"Record request completed with result: {request.result}");
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error setting new record: {request.error}");
                Debug.LogError($"Response Code: {request.responseCode}");
                Debug.LogError($"Response Body: {request.downloadHandler?.text}");
                Debug.LogError($"Response Headers:");
                var responseHeaders = request.GetResponseHeaders();
                if (responseHeaders != null)
                {
                    foreach (var header in responseHeaders)
                    {
                        Debug.LogError($"{header.Key}: {header.Value}");
                    }
                }

                if (retryCount < MAX_RETRIES - 1)
                {
                    Debug.Log($"Retrying request in {RETRY_DELAY} seconds...");
                    yield return new WaitForSeconds(RETRY_DELAY);
                    StartCoroutine(SetNewRecordWithRetry(playerName, daysSurvived, secondsPlayed, retryCount + 1));
                }
            }
            else 
            {
                try
                {
                    Debug.Log($"Success Response: {request.downloadHandler.text}");
                    APIResponse response = JsonUtility.FromJson<APIResponse>(request.downloadHandler.text);
                    Debug.Log($"Record update response: {response.body}");
                    
                    StartCoroutine(FetchAndDisplayScores(null, _darkOverlay, _window));
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error parsing record response: {e.Message}");
                    Debug.LogError($"Raw response: {request.downloadHandler.text}");
                    Debug.LogError($"Stack trace: {e.StackTrace}");
                }
            }
        }
    }

    private IEnumerator FetchAndDisplayScores(TextMeshProUGUI gameOverText, GameObject darkOverlay, GameObject window)
    {
        Debug.Log("Starting FetchAndDisplayScores");
        UnityWebRequest webRequest = null;
        try
        {
            webRequest = UnityWebRequest.Get(FETCH_API_URL);
            webRequest.SetRequestHeader("x-api-key", API_KEY);
            webRequest.SetRequestHeader("Content-Type", "application/json");
            Debug.Log($"Fetch request created with URL: {FETCH_API_URL}");
            Debug.Log($"Headers set: x-api-key and Content-Type");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error creating fetch request: {e.Message}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
            yield break;
        }
        SoundManager.Instance.PauseBackgroundSong(1f);
        using (webRequest)
        {
            Debug.Log("Sending fetch request...");
            yield return webRequest.SendWebRequest();
            Debug.Log($"Fetch request completed with result: {webRequest.result}");
            
            if (gameOverText != null)
            {
                gameOverText.enabled = false;
            }
            if (darkOverlay != null)
            {
                darkOverlay.SetActive(false);
            }
            if (window != null)
            {
                window.SetActive(false);
            }
            
            WarningSignPool.Instance.ReleaseAll();
            EnemyPool.Instance.ReleaseAll();
            BallPool.Instance.ReleaseAll();
            
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                SoundManager.Instance.StartEntryMusicCR(true, 1f);
                try
                {
                    Debug.Log($"Raw fetch response: {webRequest.downloadHandler.text}");
                    APIResponse apiResponse = JsonUtility.FromJson<APIResponse>(webRequest.downloadHandler.text);
                    ScoreData scoreData = JsonUtility.FromJson<ScoreData>(apiResponse.body);
                    Debug.Log($"Parsed score data: {apiResponse.body}");

                    ClearExistingScores();

                    if (scoreData != null && scoreData.scores != null)
                    {
                        Debug.Log($"Processing {scoreData.scores.Count} scores");
                        foreach (var score in scoreData.scores)
                        {
                            AddScore(score.playerName, Mathf.RoundToInt(score.daysSurvived), score.timeTaken);
                        }
                    }
                    else
                    {
                        Debug.LogError("Score data or scores list is null");
                    }
                    _darkOverlay.SetActive(true);
                    _window.SetActive(true);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error parsing JSON from fetch: {e.Message}");
                    Debug.LogError($"Raw response: {webRequest.downloadHandler.text}");
                    Debug.LogError($"Stack trace: {e.StackTrace}");
                    ReturnToMenu();
                }
            }
            else
            {
                Debug.LogError($"Error fetching scores: {webRequest.error}");
                Debug.LogError($"Response Code: {webRequest.responseCode}");
                Debug.LogError($"Response Headers:");
                var responseHeaders = webRequest.GetResponseHeaders();
                if (responseHeaders != null)
                {
                    foreach (var header in responseHeaders)
                    {
                        Debug.LogError($"{header.Key}: {header.Value}");
                    }
                }
                ReturnToMenu();
            }
        }
    }

    public void RefreshScores(TextMeshProUGUI gameOverText, GameObject darkOverlay, GameObject window)
    {
        Debug.Log("RefreshScores called");
        StartCoroutine(FetchAndDisplayScores(gameOverText, darkOverlay, window));
    }

    public void ReturnToMenu()
    {
        Debug.Log("ReturnToMenu called");
        _darkOverlay.SetActive(false);
        _window.SetActive(false);
        GameEnder.Instance.EndGame();
    }
}