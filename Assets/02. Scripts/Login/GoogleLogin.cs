using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Google;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;


public class GoogleLogin : MonoBehaviour
{
    [HideInInspector]
    public bool isFinish = false;

    public bool IsFinish
    {
        get => isFinish;
    }

    [HideInInspector]
    public UnityEvent loginFailEvent;

    public UserInfo userInfo = new UserInfo();

    public struct UserInfo
    {
        public string userID;
        public string email;
    }


    public string webClientId = "<your client id here>";

    private FirebaseAuth auth;
    private GoogleSignInConfiguration configuration;

    private void Awake()
    {
        configuration = new GoogleSignInConfiguration { WebClientId = webClientId, RequestEmail = true, RequestIdToken = true };

        CheckFirebaseDependencies();
    }

    private void CheckFirebaseDependencies()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                if (task.Result == DependencyStatus.Available)
                    auth = FirebaseAuth.DefaultInstance;
                else
                    Debug.LogWarning("Could not resolve all Firebase dependencies: " + task.Result.ToString());
            }
            else
            {
                Debug.LogWarning("Dependency check was not completed. Error : " + task.Exception.Message);
            }
        });
    }

    public void SignInWithGoogle() { OnSignIn(); }
    public void SignOutFromGoogle() { OnSignOut(); }

    private void OnSignIn()
    {
        if (GoogleSignIn.Configuration == null)
        {
            GoogleSignIn.Configuration = configuration;
            GoogleSignIn.Configuration.UseGameSignIn = false;
            GoogleSignIn.Configuration.RequestIdToken = true;
        }
        else
        {

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
            using (IEnumerator<Exception> enumerator = task.Exception.InnerExceptions.GetEnumerator())
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
        }
    }

    private void SignInWithGoogleOnFirebase(string idToken)
    {
        Credential credential = GoogleAuthProvider.GetCredential(idToken, null);
        auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
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
                userInfo.userID = task.Result.UserId;
                userInfo.email = task.Result.Email;

                Debug.Log("USER_ID : " + userInfo.userID);
                Debug.Log("USER_EMAIL : " + userInfo.email);

                isFinish = true;
            }
        });

    }

    public void OnSignInSilently()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;
        GoogleSignIn.DefaultInstance.SignInSilently().ContinueWith(OnAuthenticationFinished);
    }

    public void OnGamesSignIn()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = true;
        GoogleSignIn.Configuration.RequestIdToken = false;
        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnAuthenticationFinished);
    }
}
