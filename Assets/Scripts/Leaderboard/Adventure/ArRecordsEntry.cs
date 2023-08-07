using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;

public class ArRecordsEntry : MonoBehaviour
{
    public GameObject recordsEntryPrefab;
    public RectTransform leaderboardContainer;

    void Start()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError($"Failed to initialize Firebase: {task.Exception}");
                return;
            }

            LoadLeaderboardData();
        });
    }

    async void LoadLeaderboardData()
    {
        // Retrieve the  scores from Firestore
        List<LeaderboardEntry> leaderboardData = await GetScores();


        // Instantiate and populate the leaderboard entries
        for (int i = 0; i < leaderboardData.Count; i++)
        {
            GameObject entryObject = Instantiate(recordsEntryPrefab, leaderboardContainer);
            RecordsEntryUI entryUI = entryObject.GetComponent<RecordsEntryUI>();

            LeaderboardEntry entry = leaderboardData[i]; // Retrieve the current leaderboard entry

            entryUI.rankText.text = entry.rank.ToString();
            entryUI.nameText.text = entry.name;
            entryUI.scoreText.text = entry.score.ToString();

            // Adjust the position of the entry based on its index
            Vector3 entryPosition = new Vector3(0f, -i * 100f, 0f); // Adjust the Y position as needed
            entryObject.transform.localPosition = entryPosition;
        } 

        // Adjust the size of the leaderboard container based on the total height
        float totalHeight = 100f + (leaderboardData.Count * 100f); // Adjust the height as needed
        leaderboardContainer.sizeDelta = new Vector2(leaderboardContainer.sizeDelta.x, totalHeight);
    }

    async Task<List<LeaderboardEntry>> GetScores()
    {
        List<LeaderboardEntry> leaderboardData = new List<LeaderboardEntry>();

        // Connect to your Firestore database
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        // Retrieve the  scores from the "leaderboard" collection in Firestore
        Query leaderboardQuery = db.Collection("AdventureLeaderboard")
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

        return leaderboardData;
    }

    // Structure to hold leaderboard entry data
    struct LeaderboardEntry
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
}