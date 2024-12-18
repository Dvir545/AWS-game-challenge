using UnityEngine;
using System;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Security.Cryptography;

namespace AWSUtils
{
    public class CognitoAuthManager : MonoBehaviour
    {
        private const string COGNITO_ENDPOINT = "https://cognito-idp.us-east-1.amazonaws.com/";
        private const string CLIENT_ID = "1dd9nbsu4crg0pibogobnutt5r";
        private const string CLIENT_SECRET = "b8b42dfa68nrklp072lp88dalhqel5aslf77ald4hi0e07al3ii";
        private const string USER_POOL_ID = "us-east-1_VovkJZAi2";
        
        private const string TEMP_USERNAME = "dvirklein88@gmail.com";
        private const string TEMP_PASSWORD = "12345678";
        private const string TEMP_EMAIL = "dvirklein88@gmail.com";
        
        private string _idToken;
        private string _accessToken;
        private string _refreshToken;
        private DateTime _tokenExpirationTime;
        private bool _isRefreshing = false;
        private const int TOKEN_REFRESH_BUFFER_MINUTES = 5;
        private const float TOKEN_CHECK_INTERVAL = 30f; // Check token every 30 seconds
        
        public event Action<bool> OnAuthenticationChanged;
        public string Username => TEMP_USERNAME;
        public bool IsAuthenticated => !string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpirationTime;
        public string IdToken => _idToken;
        public string AccessToken => _accessToken;

        private void Start()
        {
            StartCoroutine(AuthenticateWithTemporaryCredentials());
            StartCoroutine(TokenRefreshCoroutine());
        }

        private IEnumerator TokenRefreshCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(TOKEN_CHECK_INTERVAL);
                
                if (!_isRefreshing && _tokenExpirationTime != default(DateTime))
                {
                    TimeSpan timeUntilExpiration = _tokenExpirationTime - DateTime.UtcNow;
                    if (timeUntilExpiration.TotalMinutes <= TOKEN_REFRESH_BUFFER_MINUTES)
                    {
                        _isRefreshing = true;
                        yield return RefreshSession((success, message) =>
                        {
                            _isRefreshing = false;
                            if (!success)
                            {
                                Debug.LogWarning($"Failed to refresh token: {message}");
                                StartCoroutine(AuthenticateWithTemporaryCredentials());
                            }
                        });
                    }
                }
            }
        }

        private string CalculateSecretHash(string username)
        {
            var message = Encoding.UTF8.GetBytes(username + CLIENT_ID);
            var key = Encoding.UTF8.GetBytes(CLIENT_SECRET);
            
            using (var hmac = new HMACSHA256(key))
            {
                var hash = hmac.ComputeHash(message);
                return Convert.ToBase64String(hash);
            }
        }

        private IEnumerator AuthenticateWithTemporaryCredentials()
        {
            Debug.Log("Starting authentication process...");
            yield return SignIn(TEMP_USERNAME, TEMP_PASSWORD, (success, message) =>
            {
                if (success)
                {
                    Debug.Log("Successfully signed in with temporary credentials");
                }
                else if (message.Contains("UserNotFoundException"))
                {
                    StartCoroutine(SignUpWithTemporaryCredentials());
                }
                else
                {
                    Debug.LogError($"Authentication failed: {message}");
                    // Retry authentication after a delay
                    StartCoroutine(RetryAuthentication());
                }
            });
        }

        private IEnumerator RetryAuthentication(int delaySeconds = 5)
        {
            Debug.Log($"Retrying authentication in {delaySeconds} seconds...");
            yield return new WaitForSeconds(delaySeconds);
            StartCoroutine(AuthenticateWithTemporaryCredentials());
        }

        private IEnumerator SignUpWithTemporaryCredentials()
        {
            yield return SignUp(TEMP_USERNAME, TEMP_PASSWORD, TEMP_EMAIL, (success, message) =>
            {
                if (success)
                {
                    Debug.Log("Successfully signed up with temporary credentials");
                    StartCoroutine(SignIn(TEMP_USERNAME, TEMP_PASSWORD, (signInSuccess, signInMessage) =>
                    {
                        if (signInSuccess)
                        {
                            Debug.Log("Successfully signed in after signup");
                        }
                        else
                        {
                            Debug.LogError($"Sign in after signup failed: {signInMessage}");
                            StartCoroutine(RetryAuthentication());
                        }
                    }));
                }
                else
                {
                    Debug.LogError($"Signup failed: {message}");
                    StartCoroutine(RetryAuthentication());
                }
            });
        }

        public IEnumerator SignUp(string username, string password, string email, Action<bool, string> callback)
        {
            string secretHash = CalculateSecretHash(username);
            
            var jsonPayload = $@"{{
                ""ClientId"": ""{CLIENT_ID}"",
                ""Username"": ""{username}"",
                ""Password"": ""{password}"",
                ""SecretHash"": ""{secretHash}"",
                ""UserAttributes"": [
                    {{
                        ""Name"": ""email"",
                        ""Value"": ""{email}""
                    }}
                ]
            }}";

            using (var request = CreateCognitoRequest("SignUp"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"SignUp Error Response: {request.downloadHandler.text}");
                    callback(false, request.error);
                    OnAuthenticationChanged?.Invoke(false);
                    yield break;
                }

                callback(true, "Successfully signed up!");
            }
        }

        public IEnumerator SignIn(string username, string password, Action<bool, string> callback)
        {
            string secretHash = CalculateSecretHash(username);
            
            string jsonPayload = $@"{{
                ""AuthFlow"": ""USER_PASSWORD_AUTH"",
                ""ClientId"": ""{CLIENT_ID}"",
                ""AuthParameters"": {{
                    ""USERNAME"": ""{username}"",
                    ""PASSWORD"": ""{password}"",
                    ""SECRET_HASH"": ""{secretHash}""
                }}
            }}";

            using (var request = CreateCognitoRequest("InitiateAuth"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"SignIn Error Response: {request.downloadHandler.text}");
                    callback(false, request.error);
                    OnAuthenticationChanged?.Invoke(false);
                    yield break;
                }

                try 
                {
                    var response = JsonUtility.FromJson<AuthenticationResponse>(request.downloadHandler.text);
                    UpdateTokens(response.AuthenticationResult);
                    callback(true, "Successfully signed in!");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to parse authentication response: {e.Message}");
                    Debug.LogError($"Response content: {request.downloadHandler.text}");
                    callback(false, "Failed to parse authentication response");
                    OnAuthenticationChanged?.Invoke(false);
                }
            }
        }

        public IEnumerator RefreshSession(Action<bool, string> callback)
        {
            if (string.IsNullOrEmpty(_refreshToken))
            {
                callback(false, "No refresh token available");
                OnAuthenticationChanged?.Invoke(false);
                yield break;
            }

            string secretHash = CalculateSecretHash(Username);
            
            string jsonPayload = $@"{{
                ""AuthFlow"": ""REFRESH_TOKEN_AUTH"",
                ""ClientId"": ""{CLIENT_ID}"",
                ""AuthParameters"": {{
                    ""REFRESH_TOKEN"": ""{_refreshToken}"",
                    ""SECRET_HASH"": ""{secretHash}""
                }}
            }}";

            using (var request = CreateCognitoRequest("InitiateAuth"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"RefreshSession Error Response: {request.downloadHandler.text}");
                    callback(false, request.error);
                    OnAuthenticationChanged?.Invoke(false);
                    yield break;
                }

                try
                {
                    var response = JsonUtility.FromJson<AuthenticationResponse>(request.downloadHandler.text);
                    UpdateTokens(response.AuthenticationResult);
                    callback(true, "Successfully refreshed session!");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to parse refresh response: {e.Message}");
                    Debug.LogError($"Response content: {request.downloadHandler.text}");
                    callback(false, "Failed to parse refresh response");
                    OnAuthenticationChanged?.Invoke(false);
                }
            }
        }

        private void UpdateTokens(AuthenticationResult result)
        {
            Debug.Log("CognitoAuthManager: Starting token update...");
            
            if (result == null)
            {
                Debug.LogError("CognitoAuthManager: Received null AuthenticationResult");
                return;
            }
            
            _idToken = result.IdToken;
            _accessToken = result.AccessToken;
            if (!string.IsNullOrEmpty(result.RefreshToken))
            {
                _refreshToken = result.RefreshToken;
            }
            _tokenExpirationTime = DateTime.UtcNow.AddSeconds(result.ExpiresIn);
            
            Debug.Log($"CognitoAuthManager: Tokens updated - IdToken null?: {string.IsNullOrEmpty(_idToken)}, " +
                    $"AccessToken null?: {string.IsNullOrEmpty(_accessToken)}, " +
                    $"RefreshToken null?: {string.IsNullOrEmpty(_refreshToken)}");
            
            OnAuthenticationChanged?.Invoke(true);
            
            Debug.Log($"CognitoAuthManager: Token update complete. Expiration: {_tokenExpirationTime}");
        }

        private UnityWebRequest CreateCognitoRequest(string action)
        {
            var request = new UnityWebRequest(COGNITO_ENDPOINT, "POST");
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/x-amz-json-1.1");
            request.SetRequestHeader("X-Amz-Target", $"AWSCognitoIdentityProviderService.{action}");
            return request;
        }

        [Serializable]
        private class AuthenticationResult
        {
            public string AccessToken;
            public int ExpiresIn;
            public string IdToken;
            public string RefreshToken;
            public string TokenType;
        }

        [Serializable]
        private class AuthenticationResponse
        {
            public AuthenticationResult AuthenticationResult;
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}