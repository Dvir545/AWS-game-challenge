// using UnityEngine;
// using Amazon.CognitoIdentity;
// using Amazon.Runtime;
// using System.Threading.Tasks;
// // 
// namespace AWSUtils
// {
//     public class AWSCredentialsManager : MonoBehaviour
//     {
//         private static AWSCredentialsManager instance;
        
//         // AWS Configuration
//         [SerializeField] private string identityPoolId = "us-east-1:65246273-92c2-4a6c-969d-0b6f9d1db586";
//         [SerializeField] private string cognitoRegion = "us-east-1";
        
//         // Credentials cache
//         private static AWSCredentials awsCredentials;
//         private static CognitoAWSCredentials cognitoCredentials;

//         public static AWSCredentialsManager Instance
//         {
//             get
//             {
//                 if (instance == null)
//                 {
//                     GameObject go = new GameObject("AWSCredentialsManager");
//                     instance = go.AddComponent<AWSCredentialsManager>();
//                     DontDestroyOnLoad(go);
//                 }
//                 return instance;
//             }
//         }

//         private void Awake()
//         {
//             if (instance != null && instance != this)
//             {
//                 Destroy(gameObject);
//                 return;
//             }
//             instance = this;
//             DontDestroyOnLoad(gameObject);
            
//             // Initialize credentials on awake
//             InitializeCredentials();
//         }

//         private void InitializeCredentials()
//         {
//             if (cognitoCredentials == null)
//             {
//                 cognitoCredentials = new CognitoAWSCredentials(
//                     identityPoolId,
//                     Amazon.RegionEndpoint.GetBySystemName(cognitoRegion)
//                 );
//             }
//         }

//         public async Task<AWSCredentials> GetAWSCredentialsAsync()
//         {
//             if (awsCredentials == null)
//             {
//                 InitializeCredentials();
//                 // Force credentials refresh
//                 await Task.Run(() => cognitoCredentials.GetCredentials());
//                 awsCredentials = cognitoCredentials;
//             }
//             return awsCredentials;
//         }

//         // Helper method to get temporary credentials
//         public async Task<ImmutableCredentials> GetTemporaryCredentialsAsync()
//         {
//             var credentials = await GetAWSCredentialsAsync();
//             return await credentials.GetCredentialsAsync();
//         }
//     }
// }