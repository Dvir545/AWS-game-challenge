using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MultiLobby
{


    public class MultiLobbyOpener : MonoBehaviour
    {
        [SerializeField] private bool isHost;
        [SerializeField] private TextMeshProUGUI errorMessageText; // Reference to the UI Text element
        [SerializeField] private float errorMessageDuration; // Duration to show the error message

        public void EnterLobby()
        {
            if (isHost)
            {
                string joinCode = GetJoinCode(); // DVIR - get join code from server
                if (joinCode == null)
                {
                    ShowErrorMessage("could not get join code from server");
                    return;
                }
                MultiLobbyBehavior.JoinCode = joinCode;
                MultiLobbyBehavior.IsHost = true;
                Debug.Log("Host");
                SceneManager.LoadScene("MultiLobby");
            }
            else
            {
                if (JoinCodeIsValid())
                {
                    MultiLobbyBehavior.IsHost = false;
                    Debug.Log("Client");
                    SceneManager.LoadScene("MultiLobby");
                }
                else
                {
                    ShowErrorMessage("Invalid code");
                }
            }
        }
        private string GetJoinCode()
        {
            return "zzzz" ; // DVIR - get join code from server
        }
        private bool JoinCodeIsValid()
        {
            return false;  // DVIR - allow entrance only if join code is valid
        }

        private void ShowErrorMessage(string message)
        {
            errorMessageText.text = message;
            errorMessageText.gameObject.SetActive(true);
            StartCoroutine(HideErrorMessage());
        }

        private System.Collections.IEnumerator HideErrorMessage()
        {
            yield return new WaitForSeconds(errorMessageDuration);
            errorMessageText.gameObject.SetActive(false);
        }
    }
}