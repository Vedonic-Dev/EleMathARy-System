// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;

// public class LobbyManager : MonoBehaviour
// {
//      [Header("UI References")]
//     [SerializeField]
//     private GameObject profileUI;
//     [SerializeField]
//     private GameObject changePfpUI;
//     [SerializeField]
//     private GameObject changeEmailUI;
//     [SerializeField]
//     private GameObject changePasswordUI;
//     [SerializeField]
//     private GameObject reverifyUI;
//     [SerializeField]
//     private GameObject resetPasswordConfirmUI;
//     [SerializeField]
//     private GameObject actionSuccessPanelUI;
//     [SerializeField]
//     private GameObject deleteUserConfirmUI;
//     [Space(5f)]
 
//     [Header("Basic Info References")]
//     [SerializeField]
//     private Text usernameText;
//     [SerializeField]
//     private Text emailText;
//     [SerializeField]
//     private string token;
//     [Space(5f)]
 
//     [Header("Profile Picture References")]
//     [SerializeField]
//     private Image profilePicture;
//     [SerializeField]
//     private InputField profilePictureLink;
//     [SerializeField]
//     private Text outputText;
//     [Space(5f)]
 
//     [Header("Change Email References")]
//     [SerializeField]
//     private InputField changeEmailEmailInputField;
//     [Space(5f)]
 
//     [Header("Change Password References")]
//     [SerializeField]
//     private InputField changePasswordInputField;
//     [SerializeField]
//     private InputField changePasswordConfirmInputField;
//     [Space(5f)]
 
//     [Header("Reverify References")]
//     [SerializeField]
//     private InputField reverifyEmailInputField;
//     [SerializeField]
//     private InputField reverifyPasswordInputField;
//     [Space(5)]
 
//     [Header("Action Success Panel References")]
//     [SerializeField]
//     private Text actionSuccessText;



//     private void Awake()
//     {
//         if (instance == null)
//         {
//             instance = this;
//         }
//         else
//         {
//             Destroy(gameObject);
//         }
//     }

//     private void Start()
//     {
//         if (FirebaseManager.instance.user != null)
//         {
//             StartCoroutine(LoadProfile());
//         }
//     }

//     private IEnumerator LoadProfile()
//     {
//         if (Firebase.instance.user != null)
//         {
//             System.uri photoUrl = FirebaseManager.instance.user.PhotoUrl;

//         }
//     }
// }
