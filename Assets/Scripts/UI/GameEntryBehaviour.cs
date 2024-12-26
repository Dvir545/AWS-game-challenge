using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Utils;
using UnityEngine;
using Utils.Data;
using World;
using UnityEngine.Networking;

public class GameEntryBehaviour : MonoBehaviour
{
    [SerializeField] private GameObject title;
    [SerializeField] private GameObject loginButton;
    [SerializeField] private GameObject backButton;
    [SerializeField] private GameObject guestLoginButton;
    [SerializeField] private TMP_InputField email;
    [SerializeField] private TMP_InputField username;
    [SerializeField] private TMP_InputField password;
    [SerializeField] private TMP_InputField verificationCode;
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private GameObject mainMenu;

    private const string SignUpApiUrl = "https://7tapke4vd6.execute-api.us-east-1.amazonaws.com/cognito/register";
    private const string SignInApiUrl = "https://7tapke4vd6.execute-api.us-east-1.amazonaws.com/cognito/sign-in";
    private const string ApiKey = "eVZBuSzrn113f2bFvQjTZ9tXmNyhHGxU3YcwPmWT";
    private string _userEmail;
    private Coroutine _userWarningCoroutine;
    private int _signUpStep = 0;
    private string _pendingUsername;

    private void OnEnable()
    {
        if (GameStatistics.Instance != null)
        {
            GameStatistics.Instance.OnGameDataLoaded += HandleGameDataLoaded;
        }
    }

    private void OnDisable()
    {
        if (GameStatistics.Instance != null)
        {
            GameStatistics.Instance.OnGameDataLoaded -= HandleGameDataLoaded;
        }
    }

    private void HandleGameDataLoaded()
    {
        if (!string.IsNullOrEmpty(_pendingUsername))
        {
            CompleteGameEntry(_pendingUsername);
            _pendingUsername = null;
        }
    }

    private void ShowEntry()
    {
        title.SetActive(true);
        loginButton.gameObject.SetActive(true);
        backButton.gameObject.SetActive(false);
        guestLoginButton.gameObject.SetActive(true);
        email.gameObject.SetActive(false);
        username.gameObject.SetActive(true);
        password.gameObject.SetActive(true);
        verificationCode.gameObject.SetActive(false);
        description.gameObject.SetActive(true);
        mainMenu.SetActive(false);
    }

    private void ShowSignUp()
    {
        title.SetActive(false);
        loginButton.gameObject.SetActive(false);
        backButton.gameObject.SetActive(true);
        guestLoginButton.gameObject.SetActive(false);
        email.gameObject.SetActive(true);
        username.gameObject.SetActive(true);
        password.gameObject.SetActive(true);
        verificationCode.gameObject.SetActive(false);
        description.gameObject.SetActive(true);
        mainMenu.SetActive(false);
    }
    
    private void ShowVerification()
    {
        title.SetActive(false);
        loginButton.gameObject.SetActive(false);
        backButton.gameObject.SetActive(true);
        guestLoginButton.gameObject.SetActive(false);
        email.gameObject.SetActive(false);
        username.gameObject.SetActive(false);
        password.gameObject.SetActive(false);
        verificationCode.gameObject.SetActive(true);
        description.gameObject.SetActive(true);
        mainMenu.SetActive(false);
    }

    private string GetRandomUsername()
    {
        return NameGenerator.GenerateGuestName();
    }

    private IEnumerator LoginCoroutine(string username, string password)
    {
        var requestData = new SignInRequest
        {
            username = username,
            password = password
        };

        string jsonData = JsonUtility.ToJson(requestData);
        
        using (UnityWebRequest www = new UnityWebRequest(SignInApiUrl, "POST"))
        {
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);
            www.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("x-api-key", ApiKey);

            Debug.Log($"[LOGIN] Sending login request with username: {username}");
            
            yield return www.SendWebRequest();

            Debug.Log($"[LOGIN] Response received. Status: {www.responseCode}");
            Debug.Log($"[LOGIN] Response body: {www.downloadHandler.text}");
            
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[LOGIN] Error during login: {www.error}\nResponse: {www.downloadHandler.text}");
                description.text = "Login failed. Please check your credentials.";
                yield break;
            }

            try
            {
                var lambdaResponse = JsonUtility.FromJson<LambdaResponse>(www.downloadHandler.text);
                var signInResponse = JsonUtility.FromJson<ApiResponse>(lambdaResponse.body);
                
                if (!signInResponse.success)
                {
                    description.text = signInResponse.message;
                    yield break;
                }

                // Explicitly check username from response
                Debug.Log($"[LOGIN] API Response username: {signInResponse.username}");
                Debug.Log($"[LOGIN] Input username: {username}");

                // Prioritize the username from API response (when email is used to login)
                // Fall back to the input username if API doesn't return one
                string usernameToUse = !string.IsNullOrEmpty(signInResponse.username) ? signInResponse.username : username;
                Debug.Log($"[LOGIN] Selected username to use: {usernameToUse}");
                
                // Store username first to ensure it's available
                PlayerPrefs.SetString("Username", usernameToUse);
                PlayerPrefs.Save(); // Ensure the preferences are saved immediately
                
                // Verify the username was stored
                string storedUsername = PlayerPrefs.GetString("Username");
                Debug.Log($"[LOGIN] Verified stored username in PlayerPrefs: {storedUsername}");
                
                if (signInResponse.tokens != null)
                {
                    PlayerPrefs.SetString("AccessToken", signInResponse.tokens.accessToken);
                    PlayerPrefs.SetString("RefreshToken", signInResponse.tokens.refreshToken);
                    PlayerPrefs.SetString("IdToken", signInResponse.tokens.idToken);
                    PlayerPrefs.Save();
                }
                
                Debug.Log($"[LOGIN] Successfully logged in user: {usernameToUse}");
                
                FinishEntry(usernameToUse);
            }
            catch (Exception e)
            {
                Debug.LogError($"[LOGIN] Error parsing response: {e.Message}");
                Debug.LogError($"[LOGIN] Raw response was: {www.downloadHandler.text}");
                description.text = "Error during login. Please try again.";
            }
        }
    }

    private IEnumerator SignUpCoroutine(string email, string password, string displayUsername)
    {
        var requestData = new SignUpRequest
        {
            email = email,
            password = password,
            username = displayUsername
        };

        string jsonData = JsonUtility.ToJson(requestData);
        
        using (UnityWebRequest www = new UnityWebRequest(SignUpApiUrl, "POST"))
        {
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);
            www.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("x-api-key", ApiKey);

            Debug.Log($"Sending signup request to {SignUpApiUrl} with data: {jsonData}");
            
            yield return www.SendWebRequest();

            Debug.Log($"Response received. Status: {www.responseCode}");
            Debug.Log($"Response body: {www.downloadHandler.text}");

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error during sign up: {www.error}\nResponse: {www.downloadHandler.text}");
                description.text = "Error during sign up. Please try again.";
                yield break;
            }

            try
            {
                Debug.Log($"Raw response body: {www.downloadHandler.text}");
                var lambdaResponse = JsonUtility.FromJson<LambdaResponse>(www.downloadHandler.text);
                var innerResponse = JsonUtility.FromJson<ApiResponse>(lambdaResponse.body);
                
                if (!innerResponse.success)
                {
                    description.text = innerResponse.message;
                    yield break;
                }

                _userEmail = email;
                PlayerPrefs.SetString("Username", displayUsername);
                Debug.Log($"Successfully initiated sign up for: {email}");
                
                // Update UI for verification step
                _signUpStep = 2;
                ShowVerification();
                description.text = "Please check your email and enter the verification code.";
                verificationCode.gameObject.SetActive(true);
                verificationCode.text = "";
            }
            catch (Exception e)
            {
                Debug.LogError($"Error parsing response: {e.Message}");
                Debug.LogError($"Raw response was: {www.downloadHandler.text}");
                description.text = "Error during sign up. Please try again.";
            }
        }
    }

    private IEnumerator VerifyCodeCoroutine(string code)
    {
        var requestData = new VerifyRequest
        {
            email = _userEmail,
            code = code
        };

        string jsonData = JsonUtility.ToJson(requestData);
        
        using (UnityWebRequest www = new UnityWebRequest(SignUpApiUrl, "POST"))
        {
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);
            www.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("x-api-key", ApiKey);

            Debug.Log($"Sending verification request to {SignUpApiUrl} with data: {jsonData}");
            
            yield return www.SendWebRequest();

            Debug.Log($"Response received. Status: {www.responseCode}");
            Debug.Log($"Response body: {www.downloadHandler.text}");

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error during verification: {www.error}\nResponse: {www.downloadHandler.text}");
                description.text = "Error during verification. Please try again.";
                yield break;
            }

            try
            {
                Debug.Log($"Raw response body: {www.downloadHandler.text}");
                var lambdaResponse = JsonUtility.FromJson<LambdaResponse>(www.downloadHandler.text);
                var innerResponse = JsonUtility.FromJson<ApiResponse>(lambdaResponse.body);
                
                if (!innerResponse.success)
                {
                    description.text = innerResponse.message;
                    yield break;
                }

                Debug.Log($"Successfully verified user: {_userEmail}");
                description.text = "Sign up successful! Logging in...";
                OnLogin(fromSignUp: true);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error parsing response: {e.Message}");
                Debug.LogError($"Raw response was: {www.downloadHandler.text}");
                description.text = "Error during verification. Please try again.";
            }
        }
    }
    
    private bool IsValidInput(string username, string password, string email = "valid")
    {
        bool isValidPassword = !string.IsNullOrWhiteSpace(password) && password.Length >= 8;
        
        if (!isValidPassword)
        {
            description.text = "Password must be at least 8 characters long.";
            return false;
        }
        
        return !string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(email);
    }

    private void OnInvalidInput()
    {
        if (_userWarningCoroutine != null)
        {
            StopCoroutine(_userWarningCoroutine);
        }
        _userWarningCoroutine = StartCoroutine(UserWarning());
    }

    private IEnumerator UserWarning()
    {
        if (string.IsNullOrEmpty(description.text))
        {
            description.text = "Please enter username and password to continue.";
        }
        yield return new WaitForSeconds(4);
        description.text = "";
    }

    private void CompleteGameEntry(string username)
    {
        Debug.Log($"[COMPLETE] Starting CompleteGameEntry with username: {username}");
        GameStarter.Instance.Init();
        gameObject.SetActive(false);
    }
    private void FinishEntry(string username)
    {
        Debug.Log($"[FINISH] Starting FinishEntry with username: {username}");
        _pendingUsername = username;
        Debug.Log($"[FINISH] Set _pendingUsername to: {_pendingUsername}");
        GameStatistics.Instance.Init(username);
        Debug.Log($"[FINISH] Called GameStatistics.Init with username: {username}");
    }

    public void OnGuestLogin()
    {
        var username = GetRandomUsername();
        FinishEntry(username);
        CompleteGameEntry(username);
    }

    public void OnLogin(bool fromSignUp = false)
    {
        if (!fromSignUp)
        {
            Debug.Log("Login button clicked");
            if (!IsValidInput(username.text, password.text))
            {
                OnInvalidInput();
                return;
            }
            description.text = "Logging in...";
            StartCoroutine(LoginCoroutine(username.text, password.text));
        }
        else
        {
            FinishEntry(username.text);
        }
    }
    
    public void OnSignUp()
    {
        Debug.Log($"Sign up button clicked. Current step: {_signUpStep}");
        if (_signUpStep == 0)
        {
            ShowSignUp();
            _signUpStep = 1;
            return;
        }
        if (_signUpStep == 1)
        {
            if (!IsValidInput(username.text, password.text, email.text))
            {
                OnInvalidInput();
                return;
            }
            description.text = "Signing up...";
            StartCoroutine(SignUpCoroutine(email.text, password.text, username.text));
            return;
        }
        if (_signUpStep == 2)
        {
            if (string.IsNullOrEmpty(verificationCode.text))
            {
                description.text = "Please enter the verification code from your email.";
                return;
            }
            description.text = "Verifying...";
            StartCoroutine(VerifyCodeCoroutine(verificationCode.text));
        }
    }

    public void OnBack()
    {
        _signUpStep = 0;
        ShowEntry();
    }

    [Serializable]
    private class SignUpRequest
    {
        public string email;
        public string password;
        public string username;
    }

    [Serializable]
    private class SignInRequest
    {
        public string username;
        public string password;
    }

    [Serializable]
    private class VerifyRequest
    {
        public string email;
        public string code;
    }

    [Serializable]
    private class LambdaResponse
    {
        public int statusCode;
        public Dictionary<string, string> headers;
        public string body;
    }

    [Serializable]
    private class ApiResponse
    {
        public bool success;
        public string message;
        public string username;
        public SignInTokens tokens;
    }

    [Serializable]
        private class SignInTokens
        {
            public string accessToken;
            public string refreshToken;
            public string idToken;
        }
    }
