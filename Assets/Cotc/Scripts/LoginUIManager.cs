using UnityEngine;
using UnityEngine.UI;

public class LoginUIManager : MonoBehaviour
{
    // Input field for the Email
    [SerializeField] private InputField EmailInput;
    // Input field for the Password
    [SerializeField] private InputField PasswordInput;

    [SerializeField] private GameObject m_LoginUI = null;
    [SerializeField] private GameObject m_LoginInfoUI = null;
    [SerializeField] private Text m_PlayerName = null;

    public CustomLoginManager customLoginManager = null;


    private void Start()
    {
        customLoginManager = FindObjectOfType<CustomLoginManager>();
    }

    public void ShouldDisplayLogin(bool isPlayerLogged)
    {
        m_LoginUI.SetActive(!isPlayerLogged);
        m_LoginInfoUI.SetActive(isPlayerLogged);
    }

    public void Login(string PlayerName)
    {
        EmailInput.text = "";
        PasswordInput.text = "";
        m_LoginUI.SetActive(false);
        m_LoginInfoUI.SetActive(true);
        m_PlayerName.text = PlayerName;
    }

    public void DoLogin()
    {
        if (customLoginManager != null)
        {
            customLoginManager.DoLogin();
        }
    }

    public void DoLoginEmail()
    {
        if (customLoginManager != null)
        {
            customLoginManager.DoLoginEmail(EmailInput.text, PasswordInput.text);
        }
    }

    public void DoLogout()
    {
        if (customLoginManager != null)
        {
            customLoginManager.DoLogout();
            m_LoginInfoUI.SetActive(false);
            m_LoginUI.SetActive(true);
        }
    }

}
