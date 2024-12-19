using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;
using AWSUtils;
using Utils;
using Utils.Data;

namespace AWSUtils
{
    public class GameResultsManager : MonoBehaviour
    {
        private const string API_URL = "https://749wf7z76d.execute-api.us-east-1.amazonaws.com/dev/score"; 

        [Header("Test Configuration")]
        [SerializeField] private bool useTestData = true;
        [SerializeField] private string testPlayerName = "Gerzil";
        [SerializeField] private int testDaysSurvived = 22;
        [SerializeField] private float testTimeTaken = 432.67f;

        [Header("Events")]
        public UnityEvent onSubmissionSuccess;
        public UnityEvent onSubmissionFailure;

        private bool isSubmitting = false;
        public bool IsSubmitting => isSubmitting;
        private CognitoAuthManager cognitoManager;
        private string lastCauseOfDeath;

        [Serializable]
        public class GameResult
        {
            public string playerName;
            public int daysSurvived;
            public float timeTaken;
            public string causeOfDeath;
            public ActivityStats activityData;
            public GameStats gameStats;
        }

        [Serializable]
        public class ActivityStats
        {
            public int damageTaken;
            public int cropsPlanted;
            public int cropsHarvested;
            public int cropsDestroyed;
            public int towersBuilt;
            public int towersDestroyed;
        }

        [Serializable]
        public class GameStats
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

        [Serializable]
        public class GameResultData
        {
            public string playerName;
            public int daysSurvived;
            public float timeTaken;
            public string timestamp;
        }

        private ApiResponse lastSubmissionResult;
        public ApiResponse LastSubmissionResult => lastSubmissionResult;

        private void Awake()
        {
            cognitoManager = FindObjectOfType<CognitoAuthManager>();
            if (cognitoManager == null)
            {
                Debug.LogError("GameResultsManager: CognitoAuthManager not found in scene!");
                return;
            }
            cognitoManager.OnAuthenticationChanged += OnAuthenticationChanged;
        }

        private void OnEnable()
        {
            EventManager.Instance.StartListening(EventManager.PlayerDied, OnGameOver);
        }

        private void OnDisable()
        {
            EventManager.Instance.StopListening(EventManager.PlayerDied, OnGameOver);
        }

        private void Start()
        {
            if (useTestData && cognitoManager != null)
            {
                StartCoroutine(WaitForAuthAndSubmitTest());
            }
        }

        private void OnGameOver(object causeOfDeath)
        {
            if (causeOfDeath is string cause)
            {
                lastCauseOfDeath = cause;
                var activity = ActivityTracker.Instance.CurrentActivity;
                SubmitGameResultWithStats(
                    cognitoManager.Username,
                    GameData.Instance.day,
                    GameData.Instance.secondsSinceGameStarted,
                    cause,
                    activity,
                    GetCurrentGameStats()
                );
            }
        }

        private GameStats GetCurrentGameStats()
        {
            var gameData = GameData.Instance;
            return new GameStats
            {
                finalHealth = gameData.curHealth,
                totalCash = gameData.cash,
                healthUpgradeLevel = gameData.healthUpgradeLevel,
                regenUpgradeLevel = gameData.regenUpgradeLevel,
                speedUpgradeLevel = gameData.speedUpgradeLevel,
                swordLevel = gameData.swordLevel,
                hoeLevel = gameData.hoeLevel,
                hammerLevel = gameData.hammerLevel
            };
        }

        private IEnumerator WaitForAuthAndSubmitTest()
        {
            yield return new WaitUntil(() => cognitoManager.IsAuthenticated);
            Debug.Log("*****************************************Authentication completed, submitting test result*****************************************");
            SubmitTestResult();
        }

        private void OnAuthenticationChanged(bool isAuthenticated)
        {
            Debug.Log($"GameResultsManager: Authentication state changed to {isAuthenticated}");
        }

        public void SubmitTestResult()
        {
            var testActivity = new LastRoundActivity
            {
                damageTaken = 10,
                cropsPlanted = 5,
                cropsHarvested = 3,
                cropsDestroyed = 1,
                towersBuilt = 2,
                towersDestroyed = 0
            };

            var testGameStats = new GameStats
            {
                finalHealth = 3,
                totalCash = 100,
                healthUpgradeLevel = 1,
                regenUpgradeLevel = 1,
                speedUpgradeLevel = 1,
                swordLevel = 1,
                hoeLevel = 1,
                hammerLevel = 1
            };

            SubmitGameResultWithStats(testPlayerName, testDaysSurvived, testTimeTaken, "test", testActivity, testGameStats);
        }

        public void SubmitGameResultWithStats(string playerName, int daysSurvived, float timeTaken, string causeOfDeath, LastRoundActivity activity, GameStats gameStats)
        {
            if (!ValidateSubmission(playerName, daysSurvived, timeTaken))
            {
                return;
            }

            var activityStats = new ActivityStats
            {
                damageTaken = activity.damageTaken,
                cropsPlanted = activity.cropsPlanted,
                cropsHarvested = activity.cropsHarvested,
                cropsDestroyed = activity.cropsDestroyed,
                towersBuilt = activity.towersBuilt,
                towersDestroyed = activity.towersDestroyed
            };

            if (!isSubmitting)
            {
                StartCoroutine(SubmitGameResultCoroutine(playerName, daysSurvived, timeTaken, causeOfDeath, activityStats, gameStats));
            }
            else
            {
                Debug.LogWarning("GameResultsManager: Submission already in progress");
            }
        }

        private bool ValidateSubmission(string playerName, int daysSurvived, float timeTaken)
        {
            if (cognitoManager == null || !cognitoManager.IsAuthenticated)
            {
                Debug.LogError("GameResultsManager: Not authenticated. Please authenticate first.");
                onSubmissionFailure?.Invoke();
                return false;
            }

            if (string.IsNullOrEmpty(playerName))
            {
                Debug.LogError("GameResultsManager: Player name cannot be empty");
                onSubmissionFailure?.Invoke();
                return false;
            }

            if (daysSurvived < 0)
            {
                Debug.LogError("GameResultsManager: Days survived cannot be negative");
                onSubmissionFailure?.Invoke();
                return false;
            }

            if (timeTaken < 0)
            {
                Debug.LogError("GameResultsManager: Time taken cannot be negative");
                onSubmissionFailure?.Invoke();
                return false;
            }

            return true;
        }

        private IEnumerator SubmitGameResultCoroutine(string playerName, int daysSurvived, float timeTaken, string causeOfDeath, ActivityStats activityStats, GameStats gameStats)
        {
            isSubmitting = true;
            UnityWebRequest request = null;

            var gameResult = new GameResult
            {
                playerName = playerName,
                daysSurvived = daysSurvived,
                timeTaken = timeTaken,
                causeOfDeath = causeOfDeath,
                activityData = activityStats,
                gameStats = gameStats
            };

            string jsonBody = JsonUtility.ToJson(gameResult);
            Debug.Log($"GameResultsManager: Sending game result: {jsonBody}");

            request = new UnityWebRequest(API_URL, "POST");
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", cognitoManager.IdToken);

            yield return request.SendWebRequest();

            try
            {
                if (request.result != UnityWebRequest.Result.Success)
                {
                    throw new Exception($"Request failed: {request.error}\nResponse: {request.downloadHandler.text}");
                }

                lastSubmissionResult = JsonUtility.FromJson<ApiResponse>(request.downloadHandler.text);
                Debug.Log($"GameResultsManager: Submission successful: {lastSubmissionResult.message}");
                onSubmissionSuccess?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"GameResultsManager: Submission failed - {ex.Message}");
                onSubmissionFailure?.Invoke();
                lastSubmissionResult = null;
            }
            finally
            {
                if (request != null)
                {
                    request.Dispose();
                }
                isSubmitting = false;
            }
        }

        private void OnDestroy()
        {
            if (cognitoManager != null)
            {
                cognitoManager.OnAuthenticationChanged -= OnAuthenticationChanged;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (testTimeTaken < 0) testTimeTaken = 0;
            if (testDaysSurvived < 0) testDaysSurvived = 0;
        }
#endif
    }
}