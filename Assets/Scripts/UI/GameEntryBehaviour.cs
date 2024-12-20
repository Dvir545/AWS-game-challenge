using System;
using System.Collections;
using TMPro;
using UnityEngine;
using Utils.Data;
using World;

public class GameEntryBehaviour : MonoBehaviour
{
    [SerializeField] private TMP_InputField username;
    [SerializeField] private TMP_InputField password;
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private GameObject mainMenu;
    
    private Coroutine _userWarningCoroutine;

    private bool Login(string username, string password)
    {
        Debug.Log("Username: " + username);
        Debug.Log("Password: " + password);
        return true;  // DVIR - implement login (if there's a problem, show it in the description)
    }
    
    private bool SignUp(string username, string password)
    {
        Debug.Log("Username: " + username);
        Debug.Log("Password: " + password);
        return true;  // DVIR - implement sign up (if use already exists or anything else interesting, show it in the description)
    }
    
    private bool IsValidInput(string username, string password)
    {
        return !string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password);
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
        description.text = "Please enter username and password to continue.";
        yield return new WaitForSeconds(4);
        description.text = "";
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
        }
        var success = Login(username.text, password.text);
        if (success)
        {
            gameObject.SetActive(false);
            GameStatistics.Initialize(username.text);
            GameStarter.Instance.Init();
        }
    }
    
    public void OnSignUp()
    {
        Debug.Log("Sign up button clicked");
        if (!IsValidInput(username.text, password.text))
        {
            OnInvalidInput();
            return;
        }
        description.text = "Signing up...";
        var success = SignUp(username.text, password.text);
        if (success)
        {
            description.text = "Sign up successful! logging in...";
            OnLogin(fromSignUp: true);
        }
    }
}
