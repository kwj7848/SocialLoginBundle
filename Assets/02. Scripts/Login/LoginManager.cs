using System.Collections;
using System;
using Unity.VisualScripting;
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

public struct LoginUserInfo
{
    public bool isSuccess;
    public string userID;
    public string email;
}

public class LoginManager : MonoBehaviour
{
    public UnityEvent onLoginFail;
    public UnityEvent onLoginSuccess;

    KakaoLogin _kakaoLogin;
    GoogleLogin _googleLogin;
    AppleLogin _appleLogin;
    
    void Start()ㄴ
    {
        _kakaoLogin = GetComponent<KakaoLogin>();
        _googleLogin = GetComponent<GoogleLogin>();
        _appleLogin = GetComponent<AppleLogin>();
        
        _appleLogin.loginFailEvent.AddListener(OnLoginFail);
        _kakaoLogin.loginFailEvent.AddListener(OnLoginFail);
        //_googleLogin.loginFailEvent.AddListener(OnLoginFail);
    }

    #region LOGIN

    public void LoginByPlatform(SocialLogin platform, UnityEvent onFinish)
    {
        switch (platform)
        {
            case SocialLogin.Developer:
                break;
            case SocialLogin.Apple:
                LoginApple(onFinish);
                break;
            case SocialLogin.Google:
                LoginGoogle(onFinish);
                break;
            case SocialLogin.Kakao:
                LoginKakao(onFinish);
                break;
        }
    }

    public void LoginKakao(UnityEvent onFinish)
    {
        StartCoroutine(Login());

        IEnumerator Login()
        {
            _kakaoLogin.login();

            yield return new WaitUntil(() => _kakaoLogin.IsFinish == true);
        }
    }

    public void LoginApple(UnityEvent onFinish)
    {
        StartCoroutine(Login());

        IEnumerator Login()
        {
            _appleLogin.OnClick_SignInWithApple();
            
            yield return new WaitUntil(() => _appleLogin.IsFinish == true);
        }
    }

    public void LoginGoogle(UnityEvent onFinish)
    {
        StartCoroutine(Login());

        IEnumerator Login()
        {
            _googleLogin.SignInWithGoogle();
            
            yield return new WaitUntil(() => _googleLogin.IsFinish == true);
            
            if (_googleLogin.loginUserInfo.isSuccess)
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



