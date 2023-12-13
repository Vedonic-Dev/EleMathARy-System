using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;

public class RecordsEntry : MonoBehaviour
{
    public static RecordsEntry instance;
    public GameObject recordsEntryPrefab;
    public RectTransform leaderboardContainer;
    public FirebaseApp firebaseApp;
    public FirebaseFirestore firestore;
    public FirebaseAuth auth;
    private FirebaseManager firebaseManager;

    public RawImage goldenCrownImage; // Change Image to RawImage
    public RawImage silverCrownImage; // Change Image to RawImage
    public RawImage bronzeCrownImage; // Change Image to RawImage

    private string nameOfCurrentUser;
    void Start()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError($"Failed to initialize Firebase: {task.Exception}");
                return;
            }

            firebaseApp = FirebaseApp.DefaultInstance;
            firestore = FirebaseFirestore.DefaultInstance;
            auth = FirebaseAuth.DefaultInstance;

            LoadLeaderboardData();
        });
    }

    private void Awake()
    {
        firebaseManager = FirebaseManager.instance;
        DontDestroyOnLoad(gameObject);

        if (firebaseManager == null)
        {
            Debug.LogError("FireBaseManager object not found in the scene.");
        }

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

    async void LoadLeaderboardData()
    {
        string currentUserId = firebaseManager.GetCurrentUserId();
        // Retrieve the scores from Firestore
        List<LeaderboardEntry> leaderboardData = await GetScores();

        // Instantiate and populate the leaderboard entries
        for (int i = 0; i < leaderboardData.Count; i++)
        {
            GameObject entryObject = Instantiate(recordsEntryPrefab, leaderboardContainer);
            RecordsEntryUI entryUI = entryObject.GetComponent<RecordsEntryUI>();

            LeaderboardEntry entry = leaderboardData[i]; // Retrieve the current leaderboard entry

            entryUI.rankText.text = entry.rank.ToString();
            entryUI.nameText.text = entry.name;

            if (nameOfCurrentUser == entry.name)
            {
                // Change the text color to green for the current user
                Color darkerGreen = new Color(10 / 255f, 93 / 255f, 0 / 255f, 1);
                entryUI.rankText.color = darkerGreen;
                entryUI.nameText.color = darkerGreen;
            }

            entryUI.DisplayStars(entry.score); // Display stars based on the player's score

            // Adjust the position of the entry based on its index
            Vector3 entryPosition = new Vector3(0f, -i * 100f, 0f); // Adjust the Y position as needed
            entryObject.transform.localPosition = entryPosition;

            // Add crown icons directly to the leaderboard entry
            if (entry.rank == 1)
            {
                AddCrown(entryUI.rankText, goldenCrownImage); // Use the public goldenCrownImage variable
            }
            else if (entry.rank == 2)
            {
                AddCrown(entryUI.rankText, silverCrownImage); // Use the public silverCrownImage variable
            }
            else if (entry.rank == 3)
            {
                AddCrown(entryUI.rankText, bronzeCrownImage); // Use the public bronzeCrownImage variable
            }
        }

        // Adjust the size of the leaderboard container based on the total height
        float totalHeight = 100f + (leaderboardData.Count * 100f); // Adjust the height as needed
        leaderboardContainer.sizeDelta = new Vector2(leaderboardContainer.sizeDelta.x, totalHeight);
    }

    async Task<List<LeaderboardEntry>> GetScores()
    {
        List<LeaderboardEntry> leaderboardData = new List<LeaderboardEntry>();
        string currentUserId = firebaseManager.GetCurrentUserId();

        // Fetch the player's data
        DocumentSnapshot playerSnapshot = await firestore.Collection("Users").Document(currentUserId).GetSnapshotAsync();

        if (playerSnapshot.Exists)
        {
            // Get the current grade and section from the user document
            int playerGrade = playerSnapshot.GetValue<int>("UserGrade");
            string sectionId = playerSnapshot.GetValue<string>("UserSection");
            nameOfCurrentUser = playerSnapshot.GetValue<string>("UserName");

            // Connect to your Firestore database
            FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

            // Retrieve the scores from the "SimpleLeaderboard" collection in Firestore
            Query leaderboardQuery = db.Collection("SimpleLeaderboard")
                .Document(playerGrade.ToString())
                .Collection(sectionId.ToString())
                .OrderByDescending("score");

            QuerySnapshot snapshot = await leaderboardQuery.GetSnapshotAsync();

            // Process each document in the snapshot
            int rank = 1;
            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    string name = document.GetValue<string>("playerName");
                    int score = document.GetValue<int>("score");

                    LeaderboardEntry entry = new LeaderboardEntry(rank, name, score);
                    leaderboardData.Add(entry);
                    rank++;
                }
                else
                {
                    Debug.LogWarning("Document does not exist in Firestore.");
                }
            }
        }
        else
        {
            Debug.LogError("Current user document does not exist in the 'user' collection.");
        }

        return leaderboardData;
    }

    // Define your LeaderboardEntry struct as a nested class
    public struct LeaderboardEntry
    {
        public int rank;
        public string name;
        public int score;

        public LeaderboardEntry(int rank, string name, int score)
        {
            this.rank = rank;
            this.name = name;
            this.score = score;
        }
    }

    // Add crown icon directly to the rank text
    private void AddCrown(Text rankText, RawImage crownImage)
    {
        // Create a new GameObject for the crown icon
        GameObject crownObject = new GameObject("CrownIcon");
        crownObject.transform.SetParent(rankText.transform, false);

        // Add a RawImage component to the crown object
        RawImage crown = crownObject.AddComponent<RawImage>();

        // Set the crown icon texture based on the RawImage component provided
        crown.texture = crownImage.texture;
        crown.color = crownImage.color;

        // Adjust the size and position of the crown icon relative to the rank text
        RectTransform crownRectTransform = crown.rectTransform;
        crownRectTransform.sizeDelta = new Vector2(50f, 50f); // Adjust the size as needed
        crownRectTransform.anchoredPosition = new Vector2(-50f, 0f); // Adjust the position as needed
    }


}
