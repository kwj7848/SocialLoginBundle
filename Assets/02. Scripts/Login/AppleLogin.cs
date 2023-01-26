using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Extensions;
using AppleAuth.Interfaces;
using AppleAuth.Native;
using UnityEngine;
using Firebase.Auth;
using Firebase;
using System;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;
using Firebase.Extensions;
using System.Threading.Tasks;

public class AppleLogin : MonoBehaviour
{
    public bool IsFinish => _isFinish;

    public LoginUserInfo loginUserInfo = new LoginUserInfo();

    private const string AppleUserIdKey = "AppleUserId";
    private IAppleAuthManager _appleAuthManager;
    private bool _isFinish = false;

    private void Awake()
    {
#if UNITY_IOS
        CheckFirebaseDependencies();
#endif
    }

    private void Start()
    {
#if UNITY_IOS
        // If the current platform is supported
        if (AppleAuthManager.IsCurrentPlatformSupported)
        {
            // Creates a default JSON deserializer, to transform JSON Native responses to C# instances
            var deserializer = new PayloadDeserializer();
            // Creates an Apple Authentication manager with the deserializer
            this._appleAuthManager = new AppleAuthManager(deserializer);
        }
#endif
    }

    private void Update()
    {
#if UNITY_IOS
        // Updates the AppleAuthManager instance to execute
        // pending callbacks inside Unity's execution loop
        if (this._appleAuthManager != null)
        {
            this._appleAuthManager.Update();
        }
#endif
    }

    public void StartLogin()
    {
        loginUserInfo = new LoginUserInfo();
        _isFinish = false;

        this.SignInWithApple();
    }

    private void Initialize()
    {
        // If at any point we receive a credentials revoked notification, we delete the stored User ID, and go back to login
        this._appleAuthManager.SetCredentialsRevokedCallback(result =>
        {
            Debug.Log("Received revoked callback " + result);
            PlayerPrefs.DeleteKey(AppleUserIdKey);
        });

        // If we have an Apple User Id available, get the credential status for it
        if (PlayerPrefs.HasKey(AppleUserIdKey))
        {
            var storedAppleUserId = PlayerPrefs.GetString(AppleUserIdKey);
            this.CheckCredentialStatusForUserId(storedAppleUserId);
        }
        // If we do not have an stored Apple User Id, attempt a quick login
        else
        {
            this.AttemptQuickLogin();
        }
    }


    private void CheckCredentialStatusForUserId(string appleUserId)
    {
        // If there is an apple ID available, we should check the credential state
        this._appleAuthManager.GetCredentialState(
            appleUserId,
            state =>
            {
                switch (state)
                {
                    // If it's authorized, login with that user id
                    case CredentialState.Authorized:
                        return;
                    // If it was revoked, or not found, we need a new sign in with apple attempt
                    // Discard previous apple user id
                    case CredentialState.Revoked:
                    case CredentialState.NotFound:
                        PlayerPrefs.DeleteKey(AppleUserIdKey);
                        return;
                }
            },
            error =>
            {
                var authorizationErrorCode = error.GetAuthorizationErrorCode();
                Debug.LogWarning("Error while trying to get credential state " + authorizationErrorCode.ToString() +
                                 " " + error.ToString());
            });
    }

    private void AttemptQuickLogin()
    {
        var quickLoginArgs = new AppleAuthQuickLoginArgs();

        // Quick login should succeed if the credential was authorized before and not revoked
        this._appleAuthManager.QuickLogin(
            quickLoginArgs,
            credential =>
            {
                // If it's an Apple credential, save the user ID, for later logins
                var appleIdCredential = credential as IAppleIDCredential;

                if (appleIdCredential != null)
                    PlayerPrefs.SetString(AppleUserIdKey, credential.User);
            },
            error =>
            {
                // If Quick Login fails, we should show the normal sign in with apple menu, to allow for a normal Sign In with apple
                var authorizationErrorCode = error.GetAuthorizationErrorCode();
                Debug.LogWarning("Quick Login Failed " + authorizationErrorCode.ToString() + " " + error.ToString());
            });
    }

    private void SignInWithApple()
    {
        var loginArgs = new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName);

        this._appleAuthManager.LoginWithAppleId(
            loginArgs,
            credential =>
            {
                var appleIdCredential = credential as IAppleIDCredential;

                if (appleIdCredential != null)
                {
                    // If a sign in with apple succeeds, we should have obtained the credential with the user id, name, and email, save it
                    loginUserInfo.userID = appleIdCredential.User;
                    loginUserInfo.email = appleIdCredential.Email;
                    loginUserInfo.isSuccess = true;

                    _isFinish = true;
                }
            },
            error =>
            {
                var authorizationErrorCode = error.GetAuthorizationErrorCode();
                Debug.LogWarning("Sign in with Apple failed " + authorizationErrorCode.ToString() + " " +
                                 error.ToString());

                loginUserInfo.isSuccess = false;
                _isFinish = true;
                //this.SetupLoginMenuForSignInWithApple();
            });
    }


    public void PerformLoginWithAppleIdAndFirebase(Action<FirebaseUser> firebaseAuthCallback)
    {
        var rawNonce = GenerateRandomString(32);
        var nonce = GenerateSHA256NonceFromRawNonce(rawNonce);
        Debug.Log("RWANONCE = " + rawNonce);
        Debug.Log("NONCE = " + nonce);

        var loginArgs = new AppleAuthLoginArgs(
            LoginOptions.IncludeEmail | LoginOptions.IncludeFullName,
            nonce);

        this._appleAuthManager.LoginWithAppleId(
            loginArgs,
            credential =>
            {
                var appleIdCredential = credential as IAppleIDCredential;
                if (appleIdCredential != null)
                {
                    loginUserInfo.userID = appleIdCredential.User;
                    loginUserInfo.email = appleIdCredential.Email;

                    Debug.Log("AppleUserID =" + appleIdCredential.User);

                    //_isFinish = true;
                    this.PerformFirebaseAuthentication(appleIdCredential, rawNonce, firebaseAuthCallback);
                }
            },
            error =>
            {
                Debug.LogWarning("cannot to firebase");
                // Something went wrong
            });
    }


    public void PerformQuickLoginWithFirebase(Action<FirebaseUser> firebaseAuthCallback)
    {
        var rawNonce = GenerateRandomString(32);
        var nonce = GenerateSHA256NonceFromRawNonce(rawNonce);

        var quickLoginArgs = new AppleAuthQuickLoginArgs(nonce);

        this._appleAuthManager.QuickLogin(
            quickLoginArgs,
            credential =>
            {
                var appleIdCredential = credential as IAppleIDCredential;
                if (appleIdCredential != null)
                {
                    this.PerformFirebaseAuthentication(appleIdCredential, rawNonce, firebaseAuthCallback);
                }
            },
            error =>
            {
                // Something went wrong
            });
    }


    private static string GenerateRandomString(int length)
    {
        if (length <= 0)
        {
            throw new Exception("Expected nonce to have positive length");
        }

        const string charset = "0123456789ABCDEFGHIJKLMNOPQRSTUVXYZabcdefghijklmnopqrstuvwxyz-._";
        var cryptographicallySecureRandomNumberGenerator = new RNGCryptoServiceProvider();
        var result = string.Empty;
        var remainingLength = length;

        var randomNumberHolder = new byte[1];
        while (remainingLength > 0)
        {
            var randomNumbers = new List<int>(16);
            for (var randomNumberCount = 0; randomNumberCount < 16; randomNumberCount++)
            {
                cryptographicallySecureRandomNumberGenerator.GetBytes(randomNumberHolder);
                randomNumbers.Add(randomNumberHolder[0]);
            }

            for (var randomNumberIndex = 0; randomNumberIndex < randomNumbers.Count; randomNumberIndex++)
            {
                if (remainingLength == 0)
                    break;

                var randomNumber = randomNumbers[randomNumberIndex];
                if (randomNumber < charset.Length)
                {
                    result += charset[randomNumber];
                    remainingLength--;
                }
            }
        }

        return result;
    }

    private static string GenerateSHA256NonceFromRawNonce(string rawNonce)
    {
        var sha = new SHA256Managed();
        var utf8RawNonce = Encoding.UTF8.GetBytes(rawNonce);
        var hash = sha.ComputeHash(utf8RawNonce);

        var result = string.Empty;
        for (var i = 0; i < hash.Length; i++)
        {
            result += hash[i].ToString("x2");
        }

        return result;
    }

    private void CheckFirebaseDependencies()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                if (task.Result == DependencyStatus.Available)
                    firebaseAuth = FirebaseAuth.DefaultInstance;
                else
                    Debug.LogError("Firebase is null");
            }
            else
            {
                Debug.LogWarning("Dependency check was not completed. Error : " + task.Exception.Message);
            }
        });
    }


    // Your Firebase authentication client
    private FirebaseAuth firebaseAuth;

    private void PerformFirebaseAuthentication(
        IAppleIDCredential appleIdCredential,
        string rawNonce,
        Action<FirebaseUser> firebaseAuthCallback)
    {
        var identityToken = Encoding.UTF8.GetString(appleIdCredential.IdentityToken);
        var authorizationCode = Encoding.UTF8.GetString(appleIdCredential.AuthorizationCode);
        var firebaseCredential = OAuthProvider.GetCredential(
            "apple.com",
            identityToken,
            rawNonce,
            authorizationCode);

        this.firebaseAuth.SignInWithCredentialAsync(firebaseCredential)
            .ContinueWithOnMainThread(task => HandleSignInWithUser(task, firebaseAuthCallback));
    }

    private static void HandleSignInWithUser(Task<FirebaseUser> task, Action<FirebaseUser> firebaseUserCallback)
    {
        if (task.IsCanceled)
        {
            Debug.Log("Firebase auth was canceled");
            firebaseUserCallback(null);
        }
        else if (task.IsFaulted)
        {
            Debug.Log("Firebase auth failed");
            firebaseUserCallback(null);
        }
        else
        {
            var firebaseUser = task.Result;
            Debug.Log("Firebase auth completed | User ID:" + firebaseUser.UserId);
            firebaseUserCallback(firebaseUser);
        }
    }
}