using UnityEngine;

public class KakaoLogin : MonoBehaviour
{
    public LoginUserInfo loginUserInfo = new LoginUserInfo();
    public bool IsFinish => _isFinish;

    private int saveInfoCount = 0;
    private bool _isFinish = false;
    private AndroidJavaObject _ajo;

    void Start()
    {
#if UNITY_ANDROID
        _ajo = new AndroidJavaObject("com.unity3d.player.UnityKakaoLogin");

#elif UNITY_IOS

#endif
    }

    public void login()
    {
        loginUserInfo = new LoginUserInfo();
        _isFinish = false;
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
        _isFinish = true;
        loginUserInfo.isSuccess = false;
    }

    public void SaveKakaoUserID(string userID)
    {
        loginUserInfo.userID = userID;

        Debug.Log($"ID : {userID}!!");
        saveInfoCount++;

        if (saveInfoCount >= 2)
        {
            loginUserInfo.isSuccess = true;
            _isFinish = true;
            saveInfoCount = 0;
        }
    }

    public void SaveKakaoUserEmail(string email)
    {
        loginUserInfo.email = email;
        Debug.Log($"ID : {email}!!");
        saveInfoCount++;

        if (saveInfoCount >= 2)
        {
            loginUserInfo.isSuccess = true;
            _isFinish = true;
            saveInfoCount = 0;
        }
    }
}