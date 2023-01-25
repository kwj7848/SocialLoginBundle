using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class KakaoLogin : MonoBehaviour
{
    public class UserInfo
    {
        public string userID;
        public string email;
    }

    public LoginUserInfo loginUserInfo = new LoginUserInfo();

    public bool IsFinish => _isFinish;

    public int saveInfoCount = 0;

    [HideInInspector]
    public UnityEvent loginSuccessEvent, loginFailEvent, logoutSuccessEvent;


    private AndroidJavaObject _ajo;
    private bool _isFinish = false;

    private string id;
    private string token;

    void Start()
    {
#if UNITY_ANDROID
        _ajo = new AndroidJavaObject("com.unity3d.player.UnityKakaoLogin");
#endif
    }

    public void login()
    {
#if UNITY_ANDROID
        _ajo.Call("KakaoLogin");

#elif UNITY_IOS
        FrameworkBridge.SendLoginKakaoIOS();

#endif
    }

    public void logout()
    {
#if UNITY_ANDROID
        //StartCoroutine(StartLogout("https://kapi.kakao.com/v1/user/logout", token));

#elif UNITY_IOS
        //FrameworkBridge.SendLogoutKakaoIOS();

#endif
    }

    void FailKakaoLogin(string error)
    {
        Debug.LogError("KakaoLoginError : " + error);
        loginFailEvent.Invoke();
    }

    void SaveKakaoUserID(string userID)
    {
        loginUserInfo.userID = userID;
        Debug.Log("SAVE:" + userID);
        saveInfoCount++;
        
        if (saveInfoCount >= 2)
        {
            _isFinish = true;
            saveInfoCount = 0;
        }
    }

    void SaveKakaoUserEmail(string email)
    {
        loginUserInfo.email = email;
        Debug.Log("SAVE:" + email);
        saveInfoCount++;
        
        if (saveInfoCount >= 2)
        {
            _isFinish = true;
            saveInfoCount = 0;
        }
    }
}