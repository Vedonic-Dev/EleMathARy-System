using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Firebase.Auth;

public class LogoutButton : MonoBehaviour
{
    [Header("Confirmation Dialog")]
    [SerializeField]
    private GameObject confirmationDialog;

    public static LogoutButton instance;
    private FirebaseAuth auth;

    public FirebaseManager firebaseManager;

    private void Awake() 
    {
        if (instance == null)
        {
            instance = this;        
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start() 
    {
        auth = FirebaseAuth.DefaultInstance;
        firebaseManager = FindObjectOfType<FirebaseManager>();
    }

    public void OnLogoutButtonClicked()
    {
        // Show confirmation dialog
        confirmationDialog.SetActive(true);
    }

    public void OnConfirmLogoutButtonClicked()
    {
        firebaseManager.Logout();
        SceneManager.LoadScene(2);
    }

    public void OnCancelLogouttButtonClicked()
    {
        // Hide confirmation dialog
        confirmationDialog.SetActive(false);
    }

    public void BackButton()
    {
        SceneManager.LoadScene(3);
    }
}
