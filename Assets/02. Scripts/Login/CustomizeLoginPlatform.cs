using UnityEngine;
using UnityEngine.Events;

public class CustomizeLoginPlatform : MonoBehaviour
{
    public GameObject[] loginButtons;
    public GameObject loadingPanel;

    public UnityEvent onBeforeLoginEvent;

    public UnityEvent onLoginSuccess;
    public UnityEvent onLoginFail;

    public LoginManager _loginManager;

    void Start()
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer)
            loginButtons[1].SetActive(false);
        else if (Application.platform == RuntimePlatform.Android)
            loginButtons[2].SetActive(false);
        
        _loginManager.onLoginFail.AddListener(() => onLoginFail.Invoke());
    }

    private void OnDisable()
    {
        loadingPanel.SetActive(false);
    }
    
    public void OnClick_Kakao()
    {
        onBeforeLoginEvent.Invoke();
        _loginManager.LoginByPlatform(SocialLogin.Kakao);
    }

    public void OnClick_Google()
    {
        onBeforeLoginEvent.Invoke();
        _loginManager.LoginByPlatform(SocialLogin.Google);
    }

    public void OnClick_Apple()
    {
        onBeforeLoginEvent.Invoke();
        _loginManager.LoginByPlatform(SocialLogin.Apple);
    }
}
