using System;
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Amazon.Runtime;

namespace AWSUtils
{
    // Handles NPC speech functionality by communicating with AWS API Gateway
    // and displaying the response in a speech bubble
    public class NPCSpeech : MonoBehaviour
    {
        [SerializeField] private SpeechBubbleBehaviour speechBubbleBehaviour;

        // AWS API Gateway endpoint configuration
        private const string API_URL = "https://creolt9mzl.execute-api.us-east-1.amazonaws.com/dev/startnpc"; // API Gateway URL
        private const string ALGORITHM = "AWS4-HMAC-SHA256"; // AWS authentication algorithm
        private const string SERVICE_NAME = "execute-api";     // AWS service being accessed
        private const string REGION_NAME = "us-east-1";       // AWS region
        private const string CANONICAL_URI = "/dev/startnpc"; 
        private const string HOST = "creolt9mzl.execute-api.us-east-1.amazonaws.com"; 

        // Public method to trigger NPC speech from other scripts
        public void TriggerNPCSpeech() => SendNPCRequest();

        // Called when the script instance is being loaded
        private void Start() => SendNPCRequest();

        // Displays the given text in the speech bubble
        private void Speak(string text) => speechBubbleBehaviour.SetText(text);

        /// Gets the JSON request body with player statistics
        private string GetNPCRequestJson()
        {
            // For now its a static JSON
            var requestData = new
            {
                totalGamesPlayed = 0,
                consecutiveGamesPlayed = 0,
                killedLastGameBy = "none",
                daysSurvivedLastGame = 0,
                daysSurvivedHighScore = 0
            };

            return JsonConvert.SerializeObject(requestData);
        }

        // Initiates the NPC request process by getting AWS credentials
        private async void SendNPCRequest()
        {
            try
            {
                var credentials = await AWSCredentialsManager.Instance.GetTemporaryCredentialsAsync();
                StartCoroutine(SendNPCRequestCoroutine(credentials));
            }
            catch (Exception)
            {
                Speak("Sorry, I don't want to talk with you right now...");
            }
        }

        // Returns the required signed headers string based on whether we have a session token
        private string GetSignedHeaders(bool hasSessionToken) =>
            hasSessionToken ? "content-type;host;x-amz-date;x-amz-security-token" : "content-type;host;x-amz-date";

        // Creates the signature key using a series of HMAC-SHA256 operations
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

            // Create the canonical request string
            string canonicalRequest = $"POST\n{CANONICAL_URI}\n\n{canonicalHeaders}\n{signedHeaders}\n{payloadHash}";

            // Create the string to sign for AWS authentication
            string credentialScope = $"{dateStamp}/{REGION_NAME}/{SERVICE_NAME}/aws4_request";
            string stringToSign = $"{ALGORITHM}\n{amzDate}\n{credentialScope}\n{Hash(canonicalRequest)}";

            // Calculate the signature
            string signature = CalculateSignature(stringToSign, dateStamp, credentials);

            // Create the authorization header
            string authorization = $"{ALGORITHM} " +
                                 $"Credential={credentials.AccessKey}/{dateStamp}/{REGION_NAME}/{SERVICE_NAME}/aws4_request, " +
                                 $"SignedHeaders={signedHeaders}, " +
                                 $"Signature={signature}";

            using var request = new UnityWebRequest(API_URL, "POST")
            {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonBody)),
                downloadHandler = new DownloadHandlerBuffer()
            };

            // Set required headers
            request.SetRequestHeader("Host", HOST);
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("X-Amz-Date", amzDate);
            request.SetRequestHeader("Authorization", authorization);

            if (hasSessionToken)
            {
                request.SetRequestHeader("X-Amz-Security-Token", credentials.Token);
            }

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Speak("Sorry, I'm not feeling well...");
                yield break;
            }

            try
            {
                var response = JsonConvert.DeserializeObject<NPCResponse>(request.downloadHandler.text);
                Speak(response.response);
            }
            catch (Exception)
            {
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