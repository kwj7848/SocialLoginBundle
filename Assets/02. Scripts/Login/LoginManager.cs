using System.Collections;
using System;
using UnityEngine;
using UnityEngine.Events;
using Debug = UnityEngine.Debug;

public enum SocialLogin
{
    Developer,
    Kakao,
    Google,
    Apple,
}

public class LoginUserInfo
{
    public bool isSuccess;
    public string userID;
    public string email;
}

public class LoginManager : MonoBehaviour
{
    public LoginUserInfo loginUserInfo;

    public UnityEvent onLoginFail;
    public UnityEvent onLoginSuccess;

    KakaoLogin _kakaoLogin;
    GoogleLogin _googleLogin;
    AppleLogin _appleLogin;
    
    void Start()
    {
        _kakaoLogin = GetComponent<KakaoLogin>();
        _googleLogin = GetComponent<GoogleLogin>();
        _appleLogin = GetComponent<AppleLogin>();
    }

    #region LOGIN

    public void LoginByPlatform(SocialLogin platform)
    {
        switch (platform)
        {
            case SocialLogin.Developer:
                break;
            case SocialLogin.Apple:
                LoginApple();
                break;
            case SocialLogin.Google:
                LoginGoogle();
                break;
            case SocialLogin.Kakao:
                LoginKakao();
                break;
        }
    }

    public void LoginKakao()
    {
        StartCoroutine(Login());

        IEnumerator Login()
        {
            _kakaoLogin.login();

            yield return new WaitUntil(() => _kakaoLogin.IsFinish == true);
            loginUserInfo = _kakaoLogin.loginUserInfo;

            if (loginUserInfo.isSuccess)
                OnLoginSuccess();
            else
                OnLoginFail();
        }
    }

    public void LoginApple()
    {
        StartCoroutine(Login());

        IEnumerator Login()
        {
            _appleLogin.StartLogin();
            
            yield return new WaitUntil(() => _appleLogin.IsFinish == true);
            loginUserInfo = _appleLogin.loginUserInfo;

            if (loginUserInfo.isSuccess)
                OnLoginSuccess();
            else
                OnLoginFail();
        }
    }

    public void LoginGoogle()
    {
        StartCoroutine(Login());

        IEnumerator Login()
        {
            _googleLogin.SignInWithGoogle();
            
            yield return new WaitUntil(() => _googleLogin.IsFinish == true);
            loginUserInfo = _googleLogin.loginUserInfo;

            if (loginUserInfo.isSuccess)
                OnLoginSuccess();
            else
                OnLoginFail();
        }
    }

    private void OnLoginFail()
    {
        StopAllCoroutines();
        onLoginFail.Invoke();
    }

    private void OnLoginSuccess()
    {
        StopAllCoroutines();
        onLoginSuccess.Invoke();
    }

    #endregion

    #region WITHDRAW

    public void On_Withdraw(Action onSuccess, Action onFail)
    {

    }

    #endregion

    #region LOGOUT

    public void On_LogoutWithTerminateApp()
    {

    }

    public void On_LogoutByUser(UnityEvent onFinish = null)
    {

    }

    #endregion
}



