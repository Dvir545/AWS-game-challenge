using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;

// Manages the submission of game results to AWS backend
// Integrates with CognitoAuthManager for secure API calls
public class GameResultsManager : MonoBehaviour
{
    // API endpoint for submitting game scores
    // this activates a lambda that writes to dynamoDB
    private const string API_URL = "https://749wf7z76d.execute-api.us-east-1.amazonaws.com/dev/score"; 

    // Test configuration for development and debugging  - this is static date that needs to be passed in a function.
    [Header("Test Configuration")]
    [SerializeField] private bool useTestData = true;          // Toggle to use test data
    [SerializeField] private string testPlayerName = "Gerzil"; // Test player name
    [SerializeField] private int testDaysSurvived = 22;       // Test survival days
    [SerializeField] private float testTimeTaken = 432.67f;   // Test completion time

    // Unity events for submission status
    [Header("Events")]
    public UnityEvent onSubmissionSuccess; // Triggered when submission succeeds
    public UnityEvent onSubmissionFailure; // Triggered when submission fails

    // State tracking
    private bool isSubmitting = false;
    public bool IsSubmitting => isSubmitting;
    
    // Reference to the CognitoAuthManager for authentication
    private CognitoAuthManager cognitoManager;

    // Data structure for game result submission
    [Serializable]
    public class GameResult
    {
        public string playerName;
        public int daysSurvived;
        public float timeTaken;
    }

    // Data structures for API response handling
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

    // Store the last submission result for reference
    private ApiResponse lastSubmissionResult;
    public ApiResponse LastSubmissionResult => lastSubmissionResult;

    // Initialize manager and set up Cognito authentication listener
    private void Awake()
    {
        // Find and connect to the CognitoAuthManager
        cognitoManager = FindObjectOfType<CognitoAuthManager>();
        if (cognitoManager == null)
        {
            Debug.LogError("GameResultsManager: CognitoAuthManager not found in scene!");
            return;
        }
        cognitoManager.OnAuthenticationChanged += OnAuthenticationChanged;
    }

    // Start test submission if configured
    private void Start()
    {
        if (useTestData && cognitoManager != null)
        {
            StartCoroutine(WaitForAuthAndSubmitTest());
        }
    }

    // Waits for Cognito authentication before submitting test data
    private IEnumerator WaitForAuthAndSubmitTest()
    {
        // Wait until Cognito authentication is complete
        yield return new WaitUntil(() => cognitoManager.IsAuthenticated);
        Debug.Log("Authentication completed, submitting test result");
        SubmitTestResult();
    }

    // Handler for authentication state changes
    private void OnAuthenticationChanged(bool isAuthenticated)
    {
        Debug.Log($"GameResultsManager: Authentication state changed to {isAuthenticated}");
    }

    // Submits test data using configured values
    public void SubmitTestResult()
    {
        SubmitGameResult(testPlayerName, testDaysSurvived, testTimeTaken);
    }

    // Public method to submit game results
    public void SubmitGameResult(string playerName, int daysSurvived, float timeTaken)
    {
        if (!ValidateSubmission(playerName, daysSurvived, timeTaken))
        {
            return;
        }

        if (!isSubmitting)
        {
            StartCoroutine(SubmitGameResultCoroutine(playerName, daysSurvived, timeTaken));
        }
        else
        {
            Debug.LogWarning("GameResultsManager: Submission already in progress");
        }
    }

    // Validates submission data and authentication state
    private bool ValidateSubmission(string playerName, int daysSurvived, float timeTaken)
    {
        // Check authentication status from CognitoAuthManager
        if (cognitoManager == null || !cognitoManager.IsAuthenticated)
        {
            Debug.LogError("GameResultsManager: Not authenticated. Please authenticate first.");
            onSubmissionFailure?.Invoke();
            return false;
        }

        // Validate input data
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

    // Coroutine to handle the actual submission process
    private IEnumerator SubmitGameResultCoroutine(string playerName, int daysSurvived, float timeTaken)
    {
        isSubmitting = true;
        UnityWebRequest request = null;

        // Prepare game result data
        var gameResult = new GameResult
        {
            playerName = playerName,
            daysSurvived = daysSurvived,
            timeTaken = timeTaken
        };

        string jsonBody = JsonUtility.ToJson(gameResult);
        Debug.Log($"GameResultsManager: Sending game result: {jsonBody}");

        // Set up the HTTP request
        request = new UnityWebRequest(API_URL, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        
        // Get and use the Cognito ID token for authentication
        request.SetRequestHeader("Authorization", cognitoManager.IdToken);

        // Send the request
        yield return request.SendWebRequest();

        try
        {
            // Handle request result
            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new Exception($"Request failed: {request.error}\nResponse: {request.downloadHandler.text}");
            }

            // Process successful response
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

    // Cleanup: Remove authentication change listener
    private void OnDestroy()
    {
        if (cognitoManager != null)
        {
            cognitoManager.OnAuthenticationChanged -= OnAuthenticationChanged;
        }
    }

    // Editor validation for test values
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (testTimeTaken < 0) testTimeTaken = 0;
        if (testDaysSurvived < 0) testDaysSurvived = 0;
    }
#endif
}