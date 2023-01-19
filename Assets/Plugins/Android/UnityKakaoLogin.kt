package com.unity3d.player

import android.util.Log
import com.kakao.sdk.auth.model.OAuthToken
import com.kakao.sdk.common.model.ClientError
import com.kakao.sdk.common.model.ClientErrorCause
import com.kakao.sdk.user.UserApiClient
import com.unity3d.player.UnityPlayer

class UnityKakaoLogin {
    val context = UnityPlayer.currentActivity
    fun KakaoLogin(){
        val callback: (OAuthToken?, Throwable?) -> Unit = {token, error ->
            if (error != null) {
                Log.e("UnityLog", "로그인 실패", error)
                UnityPlayer.UnitySendMessage("LoginManager", "FailKakaoLogin", error.message)
            }
            else if (token != null){
                Log.i("UnityLog", "로그인 성공 ${token.accessToken}")
            }
        }

        if (UserApiClient.instance.isKakaoTalkLoginAvailable((context))) {
            UserApiClient.instance.loginWithKakaoTalk(context) {token, error ->
                if (error != null) {
                    Log.e("UnityLog", "카카오톡으로 로그인 실패", error)
                    UnityPlayer.UnitySendMessage("LoginManager", "FailKakaoLogin", error.message)

                    if (error is ClientError && error.reason == ClientErrorCause.Cancelled) {
                        return@loginWithKakaoTalk
                    }
                }
                else if (token != null){
                    Log.i("UnityLog", "카카오톡으로 로그인 성공 ${token.accessToken}")

                    UserApiClient.instance.me { user, error ->
                        if (error != null) {
                            Log.e("UnityLog", "사용자 정보 요청 실패", error)
                        }
                        else if (user != null) {
                            Log.e("UnityLog", "카카오톡 사용자 정보 반환", error)
                            UnityPlayer.UnitySendMessage("LoginManager", "SaveKakaoUserEmail", user.kakaoAccount?.email)
                            UnityPlayer.UnitySendMessage("LoginManager", "SaveKakaoUserID", token.accessToken)
                        }
                    }
                }
            }
        } else {
            UserApiClient.instance.loginWithKakaoAccount(context, callback = callback)
        }
    }
}