// GameResultsManager.cs
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;
using Utils;
using Utils.Data;

namespace AWSUtils
{
    public class GameResultsManager : MonoBehaviour
    {
        private const string API_URL = "https://749wf7z76d.execute-api.us-east-1.amazonaws.com/dev/score";
        private CognitoAuthManager cognitoManager;
        private bool isSubmitting = false;
        private const int MAX_RETRIES = 3;
        private const float RETRY_DELAY = 2f;
        private Queue<GameResultData> pendingSubmissions = new Queue<GameResultData>();
        
        [Header("Debug Settings")]
        [SerializeField] private bool debugLogs = false;

        public bool IsSubmitting => isSubmitting;

        [Header("Events")]
        public UnityEvent<bool> onSubmissionComplete;

        private void Awake()
        {
            cognitoManager = FindObjectOfType<CognitoAuthManager>();
            if (cognitoManager == null)
            {
                Debug.LogError("GameResultsManager: CognitoAuthManager not found!");
            }
        }

        private void OnEnable()
        {
            EventManager.Instance.StartListening(EventManager.PlayerDied, OnGameEnd);
            if (cognitoManager != null)
            {
                cognitoManager.OnAuthenticationChanged += OnAuthenticationChanged;
            }
        }

        private void OnDisable()
        {
            EventManager.Instance.StopListening(EventManager.PlayerDied, OnGameEnd);
            if (cognitoManager != null)
            {
                cognitoManager.OnAuthenticationChanged -= OnAuthenticationChanged;
            }
        }

        private void OnAuthenticationChanged(bool isAuthenticated)
        {
            if (debugLogs) Debug.Log($"GameResultsManager: Authentication state changed to: {isAuthenticated}");
            if (isAuthenticated && pendingSubmissions.Count > 0)
            {
                if (debugLogs) Debug.Log("GameResultsManager: Authentication restored, processing pending submissions...");
                ProcessPendingSubmissions();
            }
        }

        private void ProcessPendingSubmissions()
        {
            if (!isSubmitting && pendingSubmissions.Count > 0)
            {
                if (!cognitoManager.IsAuthenticated || string.IsNullOrEmpty(cognitoManager.IdToken))
                {
                    if (debugLogs) Debug.Log("GameResultsManager: Waiting for valid authentication token...");
                    StartCoroutine(WaitForAuthentication());
                    return;
                }

                if (debugLogs) Debug.Log("GameResultsManager: Processing next submission from queue");
                var stats = pendingSubmissions.Dequeue();
                StartCoroutine(SubmitGameResultWithRetry(stats, 0));
            }
        }

        private void OnGameEnd(object causeOfDeath)
        {
            string deathCause = causeOfDeath?.ToString() ?? "unknown";
            if (debugLogs) Debug.Log($"GameResultsManager: Game ended. Cause: {deathCause}");
            
            GameResultData finalStats = GatherGameStats(deathCause);
            SubmitGameResult(finalStats);
            UpdateLocalStatistics(deathCause);
        }

        private GameResultData GatherGameStats(string deathCause)
        {
            return new GameResultData
            {
                playerName = cognitoManager.Username,
                daysSurvived = GameData.Instance.day,
                timeTaken = GameData.Instance.secondsSinceGameStarted,
                causeOfDeath = deathCause,
                activityData = new ActivityData
                {
                    damageTaken = ActivityTracker.Instance.CurrentActivity.damageTaken,
                    cropsPlanted = ActivityTracker.Instance.CurrentActivity.cropsPlanted,
                    cropsHarvested = ActivityTracker.Instance.CurrentActivity.cropsHarvested,
                    cropsDestroyed = ActivityTracker.Instance.CurrentActivity.cropsDestroyed,
                    towersBuilt = ActivityTracker.Instance.CurrentActivity.towersBuilt,
                    towersDestroyed = ActivityTracker.Instance.CurrentActivity.towersDestroyed
                },
                gameStats = new FinalGameStats
                {
                    finalHealth = GameData.Instance.curHealth,
                    totalCash = GameData.Instance.cash,
                    healthUpgradeLevel = GameData.Instance.healthUpgradeLevel,
                    regenUpgradeLevel = GameData.Instance.regenUpgradeLevel,
                    speedUpgradeLevel = GameData.Instance.speedUpgradeLevel,
                    swordLevel = GameData.Instance.swordLevel,
                    hoeLevel = GameData.Instance.hoeLevel,
                    hammerLevel = GameData.Instance.hammerLevel
                }
            };
        }

        private void UpdateLocalStatistics(string deathCause)
        {
            GameStatistics.OnGameOver(deathCause);
        }

        public void SubmitGameResult(GameResultData stats)
        {
            if (isSubmitting)
            {
                if (debugLogs) Debug.Log("GameResultsManager: Adding submission to queue - submission in progress");
                pendingSubmissions.Enqueue(stats);
                return;
            }

            if (!ValidateData(stats))
            {
                onSubmissionComplete?.Invoke(false);
                return;
            }

            if (!cognitoManager.IsAuthenticated || string.IsNullOrEmpty(cognitoManager.IdToken))
            {
                if (debugLogs) Debug.Log("GameResultsManager: Adding submission to queue - waiting for authentication");
                pendingSubmissions.Enqueue(stats);
                StartCoroutine(WaitForAuthentication());
                return;
            }

            StartCoroutine(SubmitGameResultWithRetry(stats, 0));
        }

        private IEnumerator WaitForAuthentication()
        {
            float timeout = 30f; // 30 seconds timeout
            float elapsed = 0f;

            while ((!cognitoManager.IsAuthenticated || string.IsNullOrEmpty(cognitoManager.IdToken)) && elapsed < timeout)
            {
                if (debugLogs && elapsed % 5f == 0) Debug.Log($"GameResultsManager: Waiting for authentication... {elapsed}s elapsed");
                yield return new WaitForSeconds(1f);
                elapsed += 1f;
            }

            if (cognitoManager.IsAuthenticated && !string.IsNullOrEmpty(cognitoManager.IdToken))
            {
                if (debugLogs) Debug.Log("GameResultsManager: Authentication complete, processing submissions");
                ProcessPendingSubmissions();
            }
            else
            {
                Debug.LogError("GameResultsManager: Authentication timeout - some results may not be submitted");
                onSubmissionComplete?.Invoke(false);
            }
        }

        private bool ValidateData(GameResultData stats)
        {
            if (string.IsNullOrEmpty(stats.playerName))
            {
                Debug.LogError("GameResultsManager: Player name is missing");
                return false;
            }

            if (stats.daysSurvived < 0)
            {
                Debug.LogError("GameResultsManager: Invalid days survived value");
                return false;
            }

            if (stats.timeTaken < 0)
            {
                Debug.LogError("GameResultsManager: Invalid time taken value");
                return false;
            }

            return true;
        }

        private IEnumerator SubmitGameResultWithRetry(GameResultData stats, int retryCount)
        {
            isSubmitting = true;
            UnityWebRequest request = null;
            bool success = false;

            try
            {
                // Check if we have a valid token before proceeding
                if (string.IsNullOrEmpty(cognitoManager.IdToken))
                {
                    Debug.LogWarning("GameResultsManager: No valid ID token available, requeueing submission");
                    pendingSubmissions.Enqueue(stats);
                    StartCoroutine(WaitForAuthentication());
                    yield break;
                }

                string jsonBody = JsonUtility.ToJson(stats);
                if (debugLogs) Debug.Log($"GameResultsManager: Submitting result (attempt {retryCount + 1}/{MAX_RETRIES}): {jsonBody}");

                request = new UnityWebRequest(API_URL, "POST");
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", cognitoManager.IdToken);

                if (debugLogs) Debug.Log($"GameResultsManager: Request created with Authorization token length: {cognitoManager.IdToken?.Length ?? 0}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"GameResultsManager: Failed to create request - {ex.Message}");
                HandleSubmissionFailure(stats, retryCount);
                yield break;
            }

            if (request == null)
            {
                Debug.LogError("GameResultsManager: Request creation failed");
                HandleSubmissionFailure(stats, retryCount);
                yield break;
            }

            UnityWebRequest.Result requestResult;
            string responseText;
            
            yield return request.SendWebRequest();
            
            try
            {
                requestResult = request.result;
                responseText = request.downloadHandler.text;

                if (requestResult != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"GameResultsManager: Request failed - {request.error}");
                    
                    if (request.responseCode == 401 || request.responseCode == 403)
                    {
                        if (debugLogs) Debug.Log("GameResultsManager: Authentication error, requeueing submission");
                        pendingSubmissions.Enqueue(stats);
                        StartCoroutine(WaitAndRetry());
                    }
                    else
                    {
                        HandleSubmissionFailure(stats, retryCount);
                    }
                }
                else
                {
                    var response = JsonUtility.FromJson<ApiResponse>(responseText);
                    if (debugLogs) Debug.Log($"GameResultsManager: Submission successful. Updated: {response.updated}");
                    
                    if (response.updated)
                    {
                        HandleNewRecord(response.data);
                    }
                    success = true;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"GameResultsManager: Failed to process response - {ex.Message}");
                HandleSubmissionFailure(stats, retryCount);
            }
            finally
            {
                request?.Dispose();
                isSubmitting = false;
                onSubmissionComplete?.Invoke(success);
                
                if (debugLogs) Debug.Log("GameResultsManager: Checking for more pending submissions...");
                ProcessPendingSubmissions();
            }
        }

        private IEnumerator WaitAndRetry()
        {
            if (debugLogs) Debug.Log($"GameResultsManager: Waiting {RETRY_DELAY} seconds before retry...");
            yield return new WaitForSeconds(RETRY_DELAY);
        }

        private void HandleSubmissionFailure(GameResultData stats, int retryCount)
        {
            if (retryCount < MAX_RETRIES - 1)
            {
                if (debugLogs) Debug.Log($"GameResultsManager: Retrying submission in {RETRY_DELAY} seconds...");
                StartCoroutine(RetrySubmission(stats, retryCount + 1));
            }
            else
            {
                Debug.LogError("GameResultsManager: Max retry attempts reached");
                isSubmitting = false;
                onSubmissionComplete?.Invoke(false);
                ProcessPendingSubmissions();
            }
        }

        private IEnumerator RetrySubmission(GameResultData stats, int retryCount)
        {
            yield return new WaitForSeconds(RETRY_DELAY);
            StartCoroutine(SubmitGameResultWithRetry(stats, retryCount));
        }

        private void HandleNewRecord(GameResultData data)
        {
            if (data.daysSurvived > GameStatistics.Instance.daysSurvivedHighScore)
            {
                GameStatistics.Instance.daysSurvivedHighScore = data.daysSurvived;
                GameStatistics.SaveStatistics();
                EventManager.Instance.TriggerEvent(EventManager.NewHighScore, data.daysSurvived);
            }
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }

    [Serializable]
    public class GameResultData
    {
        public string playerName;
        public int daysSurvived;
        public float timeTaken;
        public string causeOfDeath;
        public ActivityData activityData;
        public FinalGameStats gameStats;
    }

    [Serializable]
    public class ActivityData
    {
        public int damageTaken;
        public int cropsPlanted;
        public int cropsHarvested;
        public int cropsDestroyed;
        public int towersBuilt;
        public int towersDestroyed;
    }

    [Serializable]
    public class FinalGameStats
    {
        public int finalHealth;
        public int totalCash;
        public int healthUpgradeLevel;
        public int regenUpgradeLevel;
        public int speedUpgradeLevel;
        public int swordLevel;
        public int hoeLevel;
        public int hammerLevel;
    }

    [Serializable]
    public class ApiResponse
    {
        public string message;
        public bool updated;
        public GameResultData data;
    }
}