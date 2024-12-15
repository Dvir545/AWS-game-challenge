using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Amazon.Runtime;
using Utils;

namespace AWSUtils
{
    // This class manages NPC dialogue by interacting with AWS API Gateway to fetch and display
    // contextual responses based on the game state and player interactions
    public class NPCSpeech : MonoBehaviour
    {
        // Reference to the UI component that displays the NPC's speech
        [SerializeField] private SpeechBubbleBehaviour speechBubbleBehaviour;
        // Determines the type of NPC (Start or Mid) which affects the API endpoint and behavior
        [SerializeField] private NPCType npcType;

        // AWS API Gateway configuration constants
        private const string API_URL_PREFIX = "https://creolt9mzl.execute-api.us-east-1.amazonaws.com/dev/"; // Base URL for API endpoints
        private const string ALGORITHM = "AWS4-HMAC-SHA256"; // AWS Signature Version 4 algorithm
        private const string SERVICE_NAME = "execute-api";   // AWS service identifier
        private const string REGION_NAME = "us-east-1";     // AWS region identifier
        private const string HOST = "creolt9mzl.execute-api.us-east-1.amazonaws.com";
        
        // Instance variables for API communication
        private string _apiURL;          // Complete API endpoint URL
        private string _canonicalUri;    // Canonical URI for AWS request signing
        private List<string> _previousResponses = new List<string>(); // Stores previous NPC responses for context
        private bool _isWaitingForResponse = false; // Tracks if we're waiting for an API response

        // Auto-refresh timer variables
        private const float REFRESH_INTERVAL = 10f; // Time in seconds between auto-refresh
        private float _timeSinceLastRefresh = 0f;   // Tracks time since last refresh

        // Set default message for NPC when waiting for response
        private const string DEFAULT_MESSAGE = "...";

        // Initializes NPC-specific endpoints based on the NPC type
        private void Awake()
        {
            var urlPostfix = "";
            // Configure endpoints based on NPC type
            if (npcType == NPCType.Start)
            {
                urlPostfix = "startnpc";
                _canonicalUri = "/dev/startnpc";
            }
            else if (npcType == NPCType.Mid)
            {
                urlPostfix = "storenpc";
                _canonicalUri = "/dev/storenpc";
            }
            _apiURL = API_URL_PREFIX + urlPostfix;
            Debug.Log($"Initialized {npcType} NPC with URL: {_apiURL} and canonical URI: {_canonicalUri}");
        }

        // Update is called once per frame
        private void Update()
        {
            // Only auto-refresh for Mid NPC
            if (npcType == NPCType.Mid)
            {
                _timeSinceLastRefresh += Time.deltaTime;
                
                // Check if it's time to refresh and we're not already waiting for a response
                if (_timeSinceLastRefresh >= REFRESH_INTERVAL && !_isWaitingForResponse)
                {
                    _timeSinceLastRefresh = 0f;
                    SendNPCRequest();
                }
            }
        }

        // External interface to trigger NPC dialogue
        public void TriggerNPCSpeech()
        {
            if (!_isWaitingForResponse)
            {
                _timeSinceLastRefresh = 0f; // Reset timer when manually triggered
                SendNPCRequest();
            }
        }

        // Automatically triggers NPC dialogue when the object is initialized
        private void Start() => SendNPCRequest();

        // Updates the speech bubble UI with new text and stores the response for Mid NPCs
        private void Speak(string text)
        { 
            speechBubbleBehaviour.SetText(text);
            if (npcType == NPCType.Mid && !_isWaitingForResponse)
            {
                _previousResponses.Add(text);
            }
        }

        // Creates the request payload for Start NPCs with initial game statistics
        private object GetStartNPCRequest()
        {
            return new {
                totalGamesPlayed = 0,
                consecutiveGamesPlayed = 0,
                killedLastGameBy = "none",
                daysSurvivedLastGame = 0,
                daysSurvivedHighScore = 0
            };
        }

        // Creates the request payload for Mid NPCs with comprehensive game state information
        private object GetMidNPCRequest()
        {
            return new
            {
                playerStatus = new
                {
                    health = 4,
                    money = 1200,
                    inventory = new
                    {
                        crops = new
                        {
                            wheat = 10,
                            carrot = 5,
                            tomato = 3,
                            corn = 4,
                            pumpkin = 0
                        },
                        towerMaterials = new
                        {
                            wood = 3,
                            stone = 2,
                            iron = 1,
                            gold = 0,
                            diamond = 0
                        }
                    },
                    worldStatus = new
                    {
                        daysSurvived = 14,
                        availableTowerSpots = 3,
                        existingTowers = 3,
                        shops = new
                        {
                            seedShopAvailableSeeds = new
                            {
                                wheat = 100,
                                carrot = 75,
                                tomato = 50,
                                corn = 40,
                                pumpkin = 25
                            },
                            resourcesShopAvailableMaterials = new
                            {
                                wood = 200,
                                stone = 150,
                                iron = 100,
                                gold = 50,
                                diamond = 20
                            },
                            toolShopAvailableUpgradeLevel = new
                            {
                                hoe = 2,
                                hammer = 3,
                                sword = 3
                            },
                            utilityShopAvailableUpgradeLevels = new
                            {
                                health = 2,
                                speed = 1,
                                healthRegen = 3
                            }
                        }
                    },
                    lastRoundActivity = new
                    {
                        damageTaken = 3,
                        cropsPlanted = 20,
                        cropsHarvested = 12,
                        cropsDestroyed = 4,
                        towersBuilt = 8,
                        towersDestroyed = 2
                    },
                    nextRoundEnemies = new
                    {
                        slime = 4,
                        skeleton = 2,
                        goblinArcher = 1,
                        chicken = 3,
                        orc = 1,
                        demon = 0
                    },
                    previousResponses = _previousResponses
                }
            };
        }

        // Generates the appropriate JSON request body based on NPC type
        private string GetNPCRequestJson()
        {
            try
            {
                object requestData;
                if (npcType == NPCType.Start)
                    requestData = GetStartNPCRequest();
                else if (npcType == NPCType.Mid)
                    requestData = GetMidNPCRequest();
                else
                    throw new Exception("Invalid NPC type");

                var json = JsonConvert.SerializeObject(requestData);
                Debug.Log($"Generated JSON for {npcType}: {json}");
                return json;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to generate NPC request JSON: {ex.Message}");
                throw;
            }
        }

        // Initiates the API request process by first obtaining AWS credentials
        private async void SendNPCRequest()
        {
            try
            {
                _isWaitingForResponse = true;
                // Set default message while waiting for response
                Speak(DEFAULT_MESSAGE);
                var credentials = await AWSCredentialsManager.Instance.GetTemporaryCredentialsAsync();
                StartCoroutine(SendNPCRequestCoroutine(credentials));
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to get credentials: {ex.Message}");
                Speak("Sorry, I don't want to talk with you right now...");
                _isWaitingForResponse = false;
            }
        }

        // Determines which headers need to be signed based on the presence of a session token
        private string GetSignedHeaders(bool hasSessionToken) =>
            hasSessionToken ? "content-type;host;x-amz-date;x-amz-security-token" : "content-type;host;x-amz-date";

        // Generates the AWS Signature Version 4 signing key through multiple HMAC operations
        private string CalculateSignature(string stringToSign, string dateStamp, ImmutableCredentials credentials)
        {
            byte[] kSecret = Encoding.UTF8.GetBytes($"AWS4{credentials.SecretKey}");
            byte[] kDate = HmacSHA256(dateStamp, kSecret);
            byte[] kRegion = HmacSHA256(REGION_NAME, kDate);
            byte[] kService = HmacSHA256(SERVICE_NAME, kRegion);
            byte[] kSigning = HmacSHA256("aws4_request", kService);
            
            return HexEncode(HmacSHA256(stringToSign, kSigning));
        }

        // Computes HMAC-SHA256 hash of the data using the provided key
        private byte[] HmacSHA256(string data, byte[] key)
        {
            using var hmac = new HMACSHA256(key);
            return hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        }

        // Converts a byte array to a lowercase hexadecimal string
        private string HexEncode(byte[] data) =>
            BitConverter.ToString(data).Replace("-", "").ToLowerInvariant();

        // Computes SHA256 hash of the input string
        private string Hash(string data)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
            return HexEncode(bytes);
        }

        // Handles the actual API request including AWS authentication
        private IEnumerator SendNPCRequestCoroutine(ImmutableCredentials credentials)
        {
            string amzDate = DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ");
            string dateStamp = DateTime.UtcNow.ToString("yyyyMMdd");
            string jsonBody = GetNPCRequestJson();
            bool hasSessionToken = !string.IsNullOrEmpty(credentials.Token);

            var canonicalHeaders = new StringBuilder()
                .Append("content-type:application/json\n")
                .Append($"host:{HOST}\n")
                .Append($"x-amz-date:{amzDate}\n");

            if (hasSessionToken)
            {
                canonicalHeaders.Append($"x-amz-security-token:{credentials.Token}\n");
            }

            string signedHeaders = GetSignedHeaders(hasSessionToken);
            string payloadHash = Hash(jsonBody);

            // Use the dynamic canonical URI
            string canonicalRequest = $"POST\n{_canonicalUri}\n\n{canonicalHeaders}\n{signedHeaders}\n{payloadHash}";

            string credentialScope = $"{dateStamp}/{REGION_NAME}/{SERVICE_NAME}/aws4_request";
            string stringToSign = $"{ALGORITHM}\n{amzDate}\n{credentialScope}\n{Hash(canonicalRequest)}";

            string signature = CalculateSignature(stringToSign, dateStamp, credentials);

            string authorization = $"{ALGORITHM} " +
                                 $"Credential={credentials.AccessKey}/{dateStamp}/{REGION_NAME}/{SERVICE_NAME}/aws4_request, " +
                                 $"SignedHeaders={signedHeaders}, " +
                                 $"Signature={signature}";

            using var request = new UnityWebRequest(_apiURL, "POST")
            {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonBody)),
                downloadHandler = new DownloadHandlerBuffer()
            };

            // request.SetRequestHeader("Host", HOST);
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("X-Amz-Date", amzDate);
            request.SetRequestHeader("Authorization", authorization);

            if (hasSessionToken)
            {
                request.SetRequestHeader("X-Amz-Security-Token", credentials.Token);
            }

            yield return request.SendWebRequest();

            _isWaitingForResponse = false;

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"API request failed for {npcType}: {request.error}\nResponse: {request.downloadHandler.text}");
                Speak("Sorry, I'm not feeling well...");
                yield break;
            }

            try
            {
                var response = JsonConvert.DeserializeObject<NPCResponse>(request.downloadHandler.text);
                Speak(response.response);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to process {npcType} response: {ex.Message}\nResponse: {request.downloadHandler.text}");
                Speak("WHO AM I?! WHERE AM I?!");
            }
        }

        // Class to deserialize the API response
        private class NPCResponse
        {
            public string response { get; set; }
        }
    }
}