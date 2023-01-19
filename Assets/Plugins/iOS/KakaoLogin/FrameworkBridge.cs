using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using AOT;

public class FrameworkBridge : MonoBehaviour
{
#if UNITY_IOS
    public static void SendLoginKakaoIOS()
    {
        Debug.Log("@kakao - login bridge function called");
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {   
            _sendKakaoLogin();
        }
    }

    [DllImport("__Internal")]
    static extern void _sendKakaoLogin();

#endif
}