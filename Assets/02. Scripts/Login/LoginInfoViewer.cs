using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(LoginManager))]
public class LoginInfoViewer : MonoBehaviour
{
    public TMP_Text uidText;
    public TMP_Text emailText;
    private LoginManager _loginManager;

    void Start()
    {
        _loginManager = GetComponent<LoginManager>();
    }

    public void OnSuccessLogin()
    {
        uidText.text = $"UID : {_loginManager.loginUserInfo.userID}";
        emailText.text = $"Email : {_loginManager.loginUserInfo.email}";
    }
}
