using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class GradeChecker : MonoBehaviour
{
    public Button button1;
    public Button button2;
    public Button button3;

    private FirebaseAuth auth;
    private FirebaseFirestore db;
    private int userGradeLevel;

    private void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            button1.interactable = false;
            button2.interactable = false;
            button3.interactable = false;

            FirebaseApp app = FirebaseApp.DefaultInstance;
            auth = FirebaseAuth.GetAuth(app);
            db = FirebaseFirestore.GetInstance(app);

            // Check if the user is logged in
            if (auth.CurrentUser != null)
            {
                // Fetch the user's grade level from Firestore
                FetchUserGradeLevel();
            }
            else
            {
                Debug.LogError("User not logged in.");
            }
        });
    }

    private async void FetchUserGradeLevel()
    {
        string userId = auth.CurrentUser.UserId;
        DocumentReference docRef = db.Collection("Users").Document(userId);

        try
        {
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                // Retrieve the user's grade level from Firestore
                userGradeLevel = snapshot.GetValue<int>("UserGrade");

                // Disable buttons 2 and 3 if the grade level is 1
                switch (userGradeLevel)
                {
                    case 1:
                        button1.interactable = true;
                        break;
                    case 2:
                        button2.interactable = true;
                        break;
                    case 3:
                        button3.interactable = true;
                        break;
                    default:
                        button1.interactable = false;
                        button2.interactable = false;
                        button3.interactable = false;
                        break;
                }
            }
            else
            {
                Debug.LogError("User document does not exist.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error fetching user grade level: {e}");
        }
    }
}
