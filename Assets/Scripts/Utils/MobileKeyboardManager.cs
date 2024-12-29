using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;

public class MobileKeyboardManager : Singleton<MobileKeyboardManager>
{
    [SerializeField] private TMP_InputField[] inputFields;
    [CanBeNull] private TMP_InputField _selectedInputField;
    [SerializeField] private GameObject keyboardController;

    private void Start()
    {
        // Make sure keyboard is initially hidden
        if (keyboardController != null)
        {
            keyboardController.SetActive(false);
        }
        
        foreach (TMP_InputField inputField in inputFields)
        {
            // Add listeners for when the input field is selected and deselected
            inputField.onSelect.AddListener((string value) => OnInputFieldSelected(inputField));
        }
    }
    
    public bool IsMobileDevice()
    {
        // Check for mobile platforms
        if (Application.platform == RuntimePlatform.Android || 
            Application.platform == RuntimePlatform.IPhonePlayer)
        {
            return true;
        }
    
        // Check for mobile device in WebGL
        if (Application.platform == RuntimePlatform.WebGLPlayer && Application.isMobilePlatform)
        {
            return true;
        }
    
        return false;
    }
    
    private void OnInputFieldSelected(TMP_InputField inputField)
    {
        _selectedInputField = inputField;
        
        // Show the keyboard when an input field is selected
        if (keyboardController != null && IsMobileDevice())
        {
            keyboardController.gameObject.SetActive(true);
        }
    }
    
    public void DeleteLetter()
    {
        if(_selectedInputField.text.Length != 0) {
            _selectedInputField.text = _selectedInputField.text.Remove(_selectedInputField.text.Length - 1, 1);
        }
    }

    public void AddLetter(string letter)
    {
        _selectedInputField.text = _selectedInputField.text + letter;
    }

    public void SubmitWord()
    {
        keyboardController.SetActive(false);
    }
}
