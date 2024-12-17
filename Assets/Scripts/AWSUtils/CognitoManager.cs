using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Runtime;

// Manages AWS Cognito authentication for the game
// Handles user authentication flows and token management throughout the game session
public class CognitoAuthManager : MonoBehaviour 
{
    // AWS Cognito configuration settings
    private const string USER_POOL_ID = "us-east-1_VovkJZAi2";    // Cognito User Pool identifier
    private const string CLIENT_ID = "1dd9nbsu4crg0pibogobnutt5r"; // App's client ID in Cognito
    private const string AWS_REGION = "us-east-1";                 // AWS region for Cognito services

    // Internal state tracking
    private AmazonCognitoIdentityProviderClient _cognitoClient;    // AWS Cognito client instance
    private string _idToken;                                       // JWT token for authenticated sessions
    private bool _isAuthenticated;                                 // Current user authentication state
    private bool _isInitialized;                                  // Whether Cognito client is ready

    // Public properties for external components to check state
    public bool IsAuthenticated { get { return _isAuthenticated; } }
    public bool IsInitialized { get { return _isInitialized; } }
    public string IdToken { get { return _idToken; } }

    // Event that fires when authentication state changes
    // Other components can subscribe to this to react to auth changes
    public event Action<bool> OnAuthenticationChanged;

    // Initialize Cognito client when component is created ( i want this in order to test it - i want in to start when game starts)
    void Awake() 
    {
        InitializeCognitoClient();
    }

    // Begin auto-authentication if client is initialized
    void Start()
    {
        if (_isInitialized)
        {
            StartCoroutine(AutoAuthenticateCoroutine());
        }
    }

    // Cleanup when component is destroyed
    void OnDestroy()
    {
        if (_cognitoClient != null)
        {
            _cognitoClient.Dispose();
            _cognitoClient = null;
        }
    }

    // Sets up the AWS Cognito client with anonymous credentials
    // This is required before any authentication attempts
    private void InitializeCognitoClient()
    {
        try
        {
            // Create anonymous credentials for initial setup
            var credentials = new AnonymousAWSCredentials();
            var region = RegionEndpoint.GetBySystemName(AWS_REGION);
            _cognitoClient = new AmazonCognitoIdentityProviderClient(credentials, region);
            _isInitialized = true;
            Debug.Log("Cognito client initialized successfully");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to initialize Cognito client: {ex.Message}");
            _isInitialized = false;
        }
    }

    // Coroutine that handles automatic authentication on startup
    // Currently set to use hardcoded credentials for testing - will be replaced with user input(ohad)
    private IEnumerator AutoAuthenticateCoroutine()
    {
        yield return null;
        Debug.Log("Starting auto-authentication");
        AuthenticateUser("dvirklein88@gmail.com", "12345678");
    }

    // Public method for other components to trigger authentication
    // Takes username and password as parameters
    public void AuthenticateUser(string username, string password)
    {
        if (!_isInitialized)
        {
            Debug.LogError("Cannot authenticate: Cognito client not initialized");
            return;
        }

        StartCoroutine(AuthenticateUserCoroutine(username, password));
    }

    // Handles the actual authentication process with AWS Cognito
    // Uses USER_PASSWORD_AUTH flow to get ID token
    private IEnumerator AuthenticateUserCoroutine(string username, string password)
    {
        // Input validation
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            Debug.LogError("Username and password cannot be empty");
            SetAuthenticationState(false, null);
            yield break;
        }

        // Create authentication request with user credentials
        InitiateAuthRequest authRequest = new InitiateAuthRequest
        {
            AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
            ClientId = CLIENT_ID,
            AuthParameters = new Dictionary<string, string>
            {
                {"USERNAME", username},
                {"PASSWORD", password}
            }
        };

        // Send authentication request to AWS
        var authTask = _cognitoClient.InitiateAuthAsync(authRequest);
        
        // Wait for response
        while (!authTask.IsCompleted)
        {
            yield return null;
        }

        // Handle authentication failures
        if (authTask.IsFaulted)
        {
            Debug.LogError($"Authentication failed: {authTask.Exception.Message}");
            SetAuthenticationState(false, null);
            yield break;
        }

        try
        {
            // Process successful authentication
            var authResponse = authTask.Result;
            _idToken = authResponse.AuthenticationResult.IdToken;
            SetAuthenticationState(true, _idToken);
            Debug.Log("Authentication successful");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to process authentication response: {ex.Message}");
            SetAuthenticationState(false, null);
        }
    }

    // Updates authentication state and notifies subscribers
    // Used internally to maintain consistent state
    private void SetAuthenticationState(bool isAuthenticated, string token)
    {
        _isAuthenticated = isAuthenticated;
        _idToken = token;
        OnAuthenticationChanged?.Invoke(_isAuthenticated);
    }
}