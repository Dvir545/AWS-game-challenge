using System;
using System.Collections;
using TMPro;
using Utils;
using UnityEngine;
using Utils.Data;
using World;

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
    
    private Coroutine _userWarningCoroutine;
    private int _signUpStep = 0;

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

    private bool Login(string username, string password)
    {
        Debug.Log("Username: " + username);
        Debug.Log("Password: " + password);
        return true;  // DVIR - implement login (if there's a problem, show it in the description)
    }
    
    private bool SignUp(string username, string password, string email)
    {
        Debug.Log("Email: " + username);
        Debug.Log("Username: " + username);
        Debug.Log("Password: " + password);
        return true;  // DVIR - implement sign up (if use already exists or anything else interesting, show it in the description)
    }
    
    private bool VerifyCode(string verificationCodeText)
    {
        Debug.Log("Verification code: " + verificationCodeText);
        return true;  // DVIR - implement verification code (if it's wrong, show it in the description)
    }
    
    private bool IsValidInput(string username, string password, string email = "valid")
    {
        return !string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password) && !string.IsNullOrWhiteSpace(email);
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

    private void FinishEntry(string username)
    {
        // DVIR - try loading GameStatistics from dynamo, load using GameStatistics.Instance.LoadFromJson(json)
        GameStatistics.Instance.Init(username);
        GameStarter.Instance.Init();
        gameObject.SetActive(false);
    }

    public void OnGuestLogin()
    {
        var username = GetRandomUsername();
        FinishEntry(username);
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
            FinishEntry(username.text);
        }
    }
    
    public void OnSignUp()
    {
        Debug.Log("Sign up button clicked");
        bool success;
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
            success = SignUp(username.text, password.text, email.text);
            if (success)
            {
                description.text = "Please check your email and enter the verification code.";
                ShowVerification();
                _signUpStep = 2;
            }
            return;
        }
        if (_signUpStep == 2)
        {
            description.text = "Verifying...";
            success = VerifyCode(verificationCode.text);
            if (success)
            {
                description.text = "Sign up successful! logging in...";
                OnLogin(fromSignUp: true);
            }
        }
    }

    public void OnBack()
    {
        _signUpStep = 0;
        ShowEntry();
    }
}