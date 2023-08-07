using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;

using Firebase.Extensions;
using Firebase.Firestore;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager instance;
    FirebaseFirestore db;

    [Header("Firebase")]
    public FirebaseAuth auth;
    public FirebaseUser user;
    [Space(5f)]

    [Header("Login References")]
    [SerializeField]
    private InputField loginEmail;
    [SerializeField]
    private InputField loginPassword;
    [SerializeField]
    private Text loginOutputText;
    [Space(5f)]

    [Header("Register References")]
    [SerializeField]
    private InputField registerUsername;
    [SerializeField]
    private InputField registerEmail;
    [SerializeField]
    private InputField registerPassword;
    [SerializeField]
    private InputField registerConfirmPassword;
    [SerializeField]
    private Text registerOutputText;
    [SerializeField]
    private Dropdown teacherDropdown;
    [Space(5f)]

    [Header("Output GameObject")]
    [SerializeField]
    private GameObject outputGO;
    [Space(5f)]

    [Header("Output GameObject")]
    [SerializeField]
    private InputField forgotPassword;

    private string uid;

    private void Awake() 
    {
        DontDestroyOnLoad(gameObject);
        if (instance == null)
        {
            instance = this;        
        }
        else if (instance != this)
        {
            Destroy(instance.gameObject);
            instance = this;
        }
    }

    private void Start() {
        StartCoroutine(CheckAndFixDependencies());
    }

    private IEnumerator CheckAndFixDependencies()
    {
        var checkAndFixDependenciesTask = FirebaseApp.CheckAndFixDependenciesAsync();

        yield return new WaitUntil(predicate: () => checkAndFixDependenciesTask.IsCompleted);

        var dependencyResult = checkAndFixDependenciesTask.Result;

        if (dependencyResult == DependencyStatus.Available)
        {
            InitializeFirebase();
        }
        else
        {
            Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyResult}");
        }
    }

    private void InitializeFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
        StartCoroutine(CheckAutoLogin());

        auth.StateChanged +=  AuthStateChanged;
        AuthStateChanged(this, null);

        db = FirebaseFirestore.DefaultInstance;

    }

    private IEnumerator CheckAutoLogin()
    {
        yield return new WaitForEndOfFrame();

        if (user != null)
        {
            var reloadUserTask = user.ReloadAsync();

            yield return new WaitUntil(predicate: () => reloadUserTask.IsCompleted);
            
            AutoLogin();
        }
        else
        {
            AuthUIManager.instance.LoginScreen();
        }
    }

    private void AutoLogin()
    {
        if (user != null)
        {
            AuthUIManager.instance.LoadScene(3);
        }
        else
        {
            AuthUIManager.instance.LoginScreen();
        }
    }

    private void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth != null)
        {
            if (auth.CurrentUser != user)
            {
                bool signedIn = user != auth.CurrentUser && user != null;

                if (!signedIn && user != null)
                {
                    Debug.Log("Signed Out");
                }

                user = auth.CurrentUser;

                if (signedIn && user != null)
                {
                    Debug.Log($"Signed In: {user.DisplayName}");
                }
            }
        }
    }

    public void ClearOuputs()
    {
        loginOutputText.text = "";
        registerOutputText.text = "";
    }

    public void LoginButton()
    {
        StartCoroutine(LoginLogic(loginEmail.text, loginPassword.text));
    }

    public void RegisterButton()
    {
        string dropdownValue = teacherDropdown.options[teacherDropdown.value].text;
        StartCoroutine(RegisterLogic(registerUsername.text, registerEmail.text, registerPassword.text, registerConfirmPassword.text, dropdownValue));
    }

    public void ForgotPasswordButton()
    {
        StartCoroutine(forgotPasswordLogic(forgotPassword.text));
    }

    public IEnumerator LoginLogic(string _email, string _password)
    {
        Credential credential = EmailAuthProvider.GetCredential(_email, _password);

        var loginTask = auth.SignInWithCredentialAsync(credential);

        yield return new WaitUntil(predicate: () => loginTask.IsCompleted);

        if (loginTask.Exception != null)
        {
            FirebaseException firebaseException = (FirebaseException)loginTask.Exception.GetBaseException();
            AuthError error = (AuthError)firebaseException.ErrorCode;
            string output = "Unknown Error, Please Try Again!";

            switch (error)
            {
                case AuthError.MissingEmail:
                    output = "Please Enter Your Email";
                    break;

                case AuthError.MissingPassword:
                    output = "Please Enter Your Password";
                    break;

                case AuthError.InvalidEmail:
                    output = "Please Enter a Valid Email";
                    break;

                case AuthError.WrongPassword:
                    output = "Incorrect Password";
                    break;

                case AuthError.UserNotFound:
                    output = "Account Does not Exist";
                    break;
            }
            loginOutputText.text = output;
            OutputGO();
        }
        else
        {
            yield return new WaitForSeconds(1f);
            AuthUIManager.instance.LoadScene(3);
        }
    }

    private IEnumerator RegisterLogic(string _username, string _email, string _password, string _confirmPassword, string _teacher)
    {
        if (_username == "")
        {
            registerOutputText.text = "Please Enter your Name";
            OutputGO();
        }
        else if (_password != _confirmPassword)
        {
            registerOutputText.text = "Passwords Do Not Match!";
            OutputGO();
        }
        else if (_teacher == "Teacher" || _teacher == "")
        {
            registerOutputText.text = "Please Pick a Teacher!";
            OutputGO();
        }
        else
        {
            var registerTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);

            yield return new WaitUntil(predicate: () => registerTask.IsCompleted);

            if (registerTask.Exception != null)
            {
                FirebaseException firebaseException = (FirebaseException)registerTask.Exception.GetBaseException();
                AuthError error = (AuthError)firebaseException.ErrorCode;
                string output = "Unknown Error, Please Try Again!";

                switch (error)
                {
                    case AuthError.InvalidEmail:
                        output = "Please Enter a Valid Email";
                        break;

                    case AuthError.EmailAlreadyInUse:
                        output = "Email Already In Use";
                        break;

                    case AuthError.WeakPassword:
                        output = "Weak Password";
                        break;

                    case AuthError.MissingEmail:
                        output = "Please Enter Your Email";
                        break;

                    case AuthError.MissingPassword:
                        output = "Please Enter Your Password";
                        break;
                }
                registerOutputText.text = output;
                OutputGO();
            }
            else
            {
                uid = registerTask.Result.User.UserId;

                DocumentReference userRef = db.Collection("Users").Document(uid);

                UserProfile profile = new UserProfile
                {
                    DisplayName = _username,
                };

                var defaultUserTask = user.UpdateUserProfileAsync(profile);

                yield return new WaitUntil(predicate: () => defaultUserTask.IsCompleted);

                if (defaultUserTask.Exception != null)
                {
                    user.DeleteAsync();
                    FirebaseException firebaseException = (FirebaseException)defaultUserTask.Exception.GetBaseException();
  
                    AuthError error = (AuthError)firebaseException.ErrorCode;
                    string output = "Unknown Error, Please Try Again!";

                    switch (error)
                    {
                        case AuthError.Cancelled:
                            output = "Update User Cancelled";
                            break;

                        case AuthError.SessionExpired:
                            output = "Session Expired";
                            break;
                    }
                    registerOutputText.text = output;
                    OutputGO();
                }
                else
                {
                    Debug.Log($"Firebase User Created Successfully: {user.DisplayName}");

                    UserModel userInfo = new UserModel
                    {
                        UserID = uid,
                        UserName = _username,
                        UserEmail = _email,

                        //TODO: Give Profile Default Photo
                        UserImage = "https://firebasestorage.googleapis.com/v0/b/elemathary-f60bd.appspot.com/o/displaypic.png?alt=media&token=1b004d83-b840-4244-b629-ba46af94b2a1",
                        UserTeacher = _teacher
                    };

                    userRef.SetAsync(userInfo).ContinueWithOnMainThread(task => 
                    {
                        Debug.Log("Updated User");
                    });
                    AuthUIManager.instance.LoadScene(3);
                }
            }
        }
    }

    private IEnumerator forgotPasswordLogic(string _email)
    {
        var resetTask = auth.SendPasswordResetEmailAsync(_email);
        yield return new WaitUntil(() => resetTask.IsCompleted);

        if (resetTask.Exception != null)
        {
            FirebaseException firebaseException = (FirebaseException)resetTask.Exception.GetBaseException();
            AuthError error = (AuthError)firebaseException.ErrorCode;
            string output = "Unknown Error, Please Try Again!";

            switch (error)
            {
                case AuthError.MissingEmail:
                    output = "Please Enter Your Email";
                    break;

                case AuthError.InvalidEmail:
                    output = "Please Enter a Valid Email";
                    break;

                case AuthError.UserNotFound:
                    output = "Account Does not Exist";
                    break;
            }
            registerOutputText.text = output;
            OutputGO();
        }
        else
        {
            registerOutputText.text = "Password reset email sent. Please check your email.";
            OutputGO();
        }
    }
    
    public void OutputGO() {
        outputGO.SetActive(true);
    }

    public void UpdateProfilePicture(string _newProfileUrl)
    {
        StartCoroutine(UpdateProfilePictureLogic(_newProfileUrl));
    }

    private IEnumerator UpdateProfilePictureLogic(string _newProfileUrl)
    {
        if (user != null)
        {
            UserProfile profile = new UserProfile();

            try
            {
                UserProfile _profile = new UserProfile{
                    PhotoUrl = new System.Uri(_newProfileUrl),
                };

                profile = _profile;
            }
            catch 
            {
                // TODO: Add Lobby Manager "Ouput"
                // LobbyManager.instance.Ouput("Error Fetching Image, Make Sure Your Link is Valid!");
                yield break;
            }

            var pfpTask = user.UpdateUserProfileAsync(profile);
            yield return new WaitUntil(predicate: () => pfpTask.IsCompleted);

            if (pfpTask.Exception != null)
            {
                Debug.LogError($"Updating Profile Picture was unsuccessful: {pfpTask.Exception}");
            }
            else
            {
                //TODO: Add Lobby Manager "ChangePfpSuccess"
                // LobbyManager.instance.ChangePfpSuccess();
                Debug.LogError($"Profile Image Updated Successfully");
            }
        }
    }

    public string GetCurrentUserId()
    {
        // Replace this with the appropriate code to retrieve the current user's ID
        return auth.CurrentUser.UserId;
    }

    public void Logout(){
        if (auth.CurrentUser != null)
        {
            auth.SignOut();
            Debug.Log("User logged out");
        }
    }
}
