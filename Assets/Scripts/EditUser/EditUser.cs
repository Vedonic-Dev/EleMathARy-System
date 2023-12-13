using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;
using Firebase.Auth;
using Firebase.Storage;
using UnityEngine.EventSystems;
using SFB;

public class EditUser : MonoBehaviour
{
    public InputField usernameText;
    public Image userImage;
    public Image borderUserImage;
    public Button submitButton;

    private FirebaseAuth auth;
    private FirebaseFirestore db;
    private DocumentReference userRef;
    private FirebaseStorage storage;
    private StorageReference userImageRef;

    private FirebaseManager firebaseManager;
    private Texture2D selectedImage;

    private void Start()
    {
        // Initialize Firebase
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError($"Failed to initialize Firebase: {task.Exception}");
                return;
            }

            // Access FirebaseAuth, Firestore, and Storage
            auth = FirebaseAuth.DefaultInstance;
            db = FirebaseFirestore.DefaultInstance;
            storage = FirebaseStorage.DefaultInstance;

            // Get the storage reference for the user's image
            userImageRef = storage.GetReferenceFromUrl("gs://elemathary-f60bd.appspot.com").Child("Users");

            // Display the current user's profile
            DisplayUserProfile();
        });

        // Add a click listener to the submit button
        submitButton.onClick.AddListener(UpdateUserProfile);

        // Add a click listener to the user image
        borderUserImage.GetComponent<Button>().onClick.AddListener(UploadImage);
    }

    private void Awake()
    {
        firebaseManager = FirebaseManager.instance;
    }

    private void DisplayUserProfile()
    {
        // Get the current user's ID
        string userId = auth.CurrentUser.UserId;

        // Get the document reference for the current user
        userRef = db.Collection("Users").Document(userId);

        // Retrieve the user document
        userRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    // Retrieve the username from the document
                    string username = snapshot.GetValue<string>("UserName");

                    // Display the username
                    usernameText.text = username;

                    // Retrieve the image URL from the document
                    string imageURL = snapshot.GetValue<string>("UserImage");

                    if (!string.IsNullOrEmpty(imageURL))
                    {
                        // Load and display the user image
                        StartCoroutine(LoadImageFromURL(imageURL));
                    }
                }
                else
                {
                    Debug.LogError("User document does not exist.");
                }
            }
            else if (task.Exception != null)
            {
                Debug.LogError($"Failed to retrieve user document: {task.Exception}");
            }
        });
    }

    private void LoadUserImage()
    {
        // Download the user's image from Firebase Storage
        userImageRef.GetDownloadUrlAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                string imageURL = task.Result.ToString();

                StartCoroutine(LoadImageFromURL(imageURL));
            }
            else if (task.Exception != null)
            {
                Debug.LogError($"Failed to retrieve user image URL: {task.Exception}");
            }
        });
    }

    private IEnumerator LoadImageFromURL(string imageURL)
    {
        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageURL))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                // Create a texture from the downloaded data
                Texture2D texture = DownloadHandlerTexture.GetContent(www);

                // Create a sprite from the texture
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);

                // Set the user image sprite
                userImage.sprite = sprite;

                // Set the user image component to display as a circle
                userImage.type = Image.Type.Filled;
                userImage.fillMethod = Image.FillMethod.Radial360;
                userImage.fillOrigin = (int)Image.Origin360.Bottom;
            }
            else
            {
                Debug.LogError($"Failed to load user image: {www.error}");
            }
        }
    }

    private void UpdateUserProfile()
    {
        string newUsername = usernameText.text;

        // Update the user document with the new username
        Dictionary<string, object> updates = new Dictionary<string, object>
        {
            { "UserName", newUsername }
        };

        userRef.UpdateAsync(updates).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("User profile updated successfully.");

                // Check if an image was selected
                if (selectedImage != null)
                {
                    // Create a unique filename for the image
                    string filename = $"{auth.CurrentUser.UserId}_{System.DateTime.Now.Ticks}.png";

                    // Create a reference to the image in Firebase Storage
                    StorageReference imageReference = userImageRef.Child(filename);

                    // Encode the selected image into PNG format
                    byte[] imageData = selectedImage.EncodeToPNG();

                    // Upload the image to Firebase Storage
                    imageReference.PutBytesAsync(imageData).ContinueWithOnMainThread(uploadTask =>
                    {
                        if (uploadTask.IsCompleted)
                        {
                            Debug.Log("Image uploaded successfully.");

                            // Get the download URL of the uploaded image
                            imageReference.GetDownloadUrlAsync().ContinueWithOnMainThread(downloadTask =>
                            {
                                if (downloadTask.IsCompleted)
                                {
                                    string imageURL = downloadTask.Result.ToString();

                                    // Update the user document with the new image URL
                                    Dictionary<string, object> imageUpdate = new Dictionary<string, object>
                                    {
                                        { "UserImage", imageURL }
                                    };

                                    userRef.UpdateAsync(imageUpdate).ContinueWithOnMainThread(updateTask =>
                                    {
                                        if (updateTask.IsCompleted)
                                        {
                                            Debug.Log("User image URL updated successfully.");

                                            // Clear the selected image
                                            selectedImage = null;

                                            // Display the updated user profile
                                            DisplayUserProfile();
                                        }
                                        else if (updateTask.Exception != null)
                                        {
                                            Debug.LogError($"Failed to update user image URL: {updateTask.Exception}");
                                        }
                                    });
                                }
                                else if (downloadTask.Exception != null)
                                {
                                    Debug.LogError($"Failed to retrieve image download URL: {downloadTask.Exception}");
                                }
                            });
                        }
                        else if (uploadTask.Exception != null)
                        {
                            Debug.LogError($"Failed to upload image: {uploadTask.Exception}");
                        }
                    });
                }
            }
            else if (task.Exception != null)
            {
                Debug.LogError($"Failed to update user profile: {task.Exception}");
            }
        });
    }

    private void UploadImage()
    {
        // Open a file picker to choose an image
        NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) =>
        {
            if (path != null)
            {
                StartCoroutine(LoadImageFromFile(path));
            }
        }, "Select Image", "image/*");

        if (permission == NativeGallery.Permission.Denied)
        {
            Debug.LogError("Permission to access image gallery denied.");
        }
    }

    private IEnumerator LoadImageFromFile(string imagePath)
    {
        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture("file://" + imagePath))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                selectedImage = DownloadHandlerTexture.GetContent(www);

                // Set the selected image as the user image
                userImage.sprite = Sprite.Create(selectedImage, new Rect(0, 0, selectedImage.width, selectedImage.height), Vector2.one * 0.5f);

                // Set the user image component to display as a circle
                userImage.type = Image.Type.Filled;
                userImage.fillMethod = Image.FillMethod.Radial360;
                userImage.fillOrigin = (int)Image.Origin360.Bottom;
            }
            else
            {
                Debug.LogError($"Failed to load image from file: {www.error}");
            }
        }
    }
}
