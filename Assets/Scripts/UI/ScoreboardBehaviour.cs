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
    public string playerName;
    public float daysSurvived;
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
    private const string API_URL = "https://wjfv1q5r9e.execute-api.us-east-1.amazonaws.com/fetch/fetch";
    private const string API_KEY = "eVZBuSzrn113f2bFvQjTZ9tXmNyhHGxU3YcwPmWT";

    private int _nScores = 0;
    
    private void Awake()
    {
        _darkOverlay = transform.GetChild(0).gameObject;
        _window = transform.GetChild(1).gameObject;
        _darkOverlay.SetActive(false);
        _window.SetActive(false);
    }

    private void ClearExistingScores()
    {
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
        var score = Instantiate(scorePrefab, scoresParent.transform);
        SetScore(score, playerName, daysSurvived, secondsPlayed);
        score.transform.localPosition = new Vector3(0, StartY - YOffsetBetweenScores * _nScores, 0);
        _nScores++;
    }

    public void SetPlayerScore(string playerName, int daysSurvived, float secondsPlayed)
    {  // this should be used on your current score when losing
        SetScore(playerScore, playerName, daysSurvived, secondsPlayed);
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
    }

    private IEnumerator FetchAndDisplayScores(TextMeshProUGUI gameOverText, GameObject darkOverlay, GameObject window)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(API_URL))
        {
            webRequest.SetRequestHeader("x-api-key", API_KEY);

            yield return webRequest.SendWebRequest();
            gameOverText.enabled = false;
            darkOverlay.SetActive(false);
            window.SetActive(false);
            WarningSignPool.Instance.ReleaseAll();
            EnemyPool.Instance.ReleaseAll();
            BallPool.Instance.ReleaseAll();
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    // First parse the outer API response
                    APIResponse apiResponse = JsonUtility.FromJson<APIResponse>(webRequest.downloadHandler.text);
                    
                    // Then parse the inner body which contains the scores
                    ScoreData scoreData = JsonUtility.FromJson<ScoreData>(apiResponse.body);

                    ClearExistingScores();

                    if (scoreData != null && scoreData.scores != null)
                    {
                        foreach (var score in scoreData.scores)
                        {
                            // Convert float daysSurvived to int for display
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
                    Debug.LogError($"Error parsing JSON: {e.Message}");
                    Debug.LogError($"Raw response: {webRequest.downloadHandler.text}");
                    ReturnToMenu();
                }
            }
            else
            {
                Debug.LogError($"Error fetching scores: {webRequest.error}");
                ReturnToMenu();
            }
        }
    }

    public void RefreshScores(TextMeshProUGUI gameOverText, GameObject darkOverlay, GameObject window)
    {
        StartCoroutine(FetchAndDisplayScores(gameOverText, darkOverlay, window));
    }

    public void ReturnToMenu()
    {
        _darkOverlay.SetActive(false);
        _window.SetActive(false);
        GameEnder.Instance.EndGame();
    }
}