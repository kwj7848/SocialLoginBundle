using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Google;
using UnityEngine;
using UnityEngine.Events;

public class GoogleLogin : MonoBehaviour
{
    public string webClientId = "<your client id here>";
    public bool IsFinish => _isFinish;
    public LoginUserInfo loginUserInfo = new LoginUserInfo();

    [HideInInspector]
    public UnityEvent loginFailEvent;

    [HideInInspector]
    public UnityEvent loginSuccessEvent;

    private bool _isFinish = false;
    private FirebaseAuth _auth;
    private GoogleSignInConfiguration _configuration;

    private void Awake()
    {
        _configuration = new GoogleSignInConfiguration
        {
            WebClientId = webClientId,
            RequestEmail = true,
            RequestIdToken = true
        };

        CheckFirebaseDependencies();
    }

    private void CheckFirebaseDependencies()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                if (task.Result == DependencyStatus.Available)
                    _auth = FirebaseAuth.DefaultInstance;
                else
                    Debug.LogWarning("Could not resolve all Firebase dependencies: " + task.Result);
            }
            else
            {
                Debug.LogWarning("Dependency check was not completed. Error : " + task.Exception?.Message);
            }
        });
    }

    public void SignInWithGoogle()
    {
        OnSignIn();
    }

    public void SignOutFromGoogle()
    {
        OnSignOut();
    }

    private void OnSignIn()
    {
        if (GoogleSignIn.Configuration == null)
        {
            GoogleSignIn.Configuration = _configuration;
            GoogleSignIn.Configuration.UseGameSignIn = false;
            GoogleSignIn.Configuration.RequestIdToken = true;
        }

        Debug.Log("Calling SignIn");

        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnAuthenticationFinished);
    }

    private void OnSignOut()
    {
        Debug.Log("Calling SignOut");
        GoogleSignIn.DefaultInstance.SignOut();
    }

    public void OnDisconnect()
    {
        Debug.Log("Calling Disconnect");
        GoogleSignIn.DefaultInstance.Disconnect();
    }

    internal void OnAuthenticationFinished(Task<GoogleSignInUser> task)
    {
        if (task.IsFaulted)
        {
            using (IEnumerator<Exception> enumerator = task.Exception?.InnerExceptions.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    GoogleSignIn.SignInException error = (GoogleSignIn.SignInException)enumerator.Current;
                    Debug.LogWarning("Got Error: " + error.Status + " " + error.Message);
                }
                else
                {
                    Debug.LogWarning("Got Unexpected Exception?!?" + task.Exception);
                }
            }

            loginFailEvent.Invoke();
        }
        else if (task.IsCanceled)
        {
            Debug.LogWarning("Canceled");
            loginFailEvent.Invoke();
        }
        else
        {
            Debug.Log("Welcome: " + task.Result.DisplayName + "!");
            SignInWithGoogleOnFirebase(task.Result.IdToken);
            loginSuccessEvent.Invoke();
        }
    }

    private void SignInWithGoogleOnFirebase(string idToken)
    {
        Credential credential = GoogleAuthProvider.GetCredential(idToken, null);
        _auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
        {
            AggregateException ex = task.Exception;

            if (ex != null)
            {
                Debug.Log("LOGIN FAILED");
                loginFailEvent.Invoke();
                if (ex.InnerExceptions[0] is FirebaseException inner && (inner.ErrorCode != 0))
                {
                }
            }
            else
            {
                Debug.Log("SIGN IN SUCCESSFUL");
                loginUserInfo.userID = task.Result.UserId;
                loginUserInfo.email = task.Result.Email;
                
                _isFinish = true;
            }
        });
    }

    public void Test()
    {
        int i = 10;
        Debug.Log(i);
        i++;
        i++;
        Debug.Log(i);
    }

    public void OnSignInSilently()
    {
        GoogleSignIn.Configuration = _configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;
        GoogleSignIn.DefaultInstance.SignInSilently().ContinueWith(OnAuthenticationFinished);
    }

    public void OnGamesSignIn()
    {
        GoogleSignIn.Configuration = _configuration;
        GoogleSignIn.Configuration.UseGameSignIn = true;
        GoogleSignIn.Configuration.RequestIdToken = false;
        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnAuthenticationFinished);
    }
}