using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputFieldNavigation : MonoBehaviour
{
    EventSystem system;
    [SerializeField] private TMP_InputField username;
    [SerializeField] private TMP_InputField password;
    [SerializeField] private TMP_InputField email;
    [SerializeField] private TMP_InputField verificationCode;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button registerButton;
    [SerializeField] private Button backButton;
  
    void Start()
    {
        system = EventSystem.current;

    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (backButton.IsActive())
                backButton.onClick.Invoke();
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (loginButton.IsActive())
                loginButton.onClick.Invoke();
            else if (registerButton.IsActive())
                registerButton.onClick.Invoke();
        }
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            TMP_InputField cur = system.currentSelectedGameObject?.GetComponent<TMP_InputField>();
            if (cur == null)
            {
                if (email.IsActive())
                {
                    cur = email;
                } else if (verificationCode.IsActive())
                {
                    cur = verificationCode;
                } else if (username.IsActive())
                {
                    cur = username;
                } else
                {
                    return;
                }
                system.SetSelectedGameObject(cur.gameObject, new BaseEventData(system));
            }
            else
            {
                cur = system.currentSelectedGameObject.GetComponent<TMP_InputField>();
                if (cur != null)
                {
                    TMP_InputField next;
                    // if cur is username, go to password
                    if (cur == username)
                    {
                        next = password;
                    } else if (cur == password)
                    {
                        if (email.IsActive())
                        {
                            next = email;
                        }
                        else
                        {
                            next = username;
                        }
                    } else if (cur == email)
                    {
                        next = username;
                    }
                    else
                    {
                        next = username;
                    }

                    next.OnPointerClick(new PointerEventData(system));
              
                    system.SetSelectedGameObject(next.gameObject, new BaseEventData(system));
                }
            }
            
        }
    }
}
