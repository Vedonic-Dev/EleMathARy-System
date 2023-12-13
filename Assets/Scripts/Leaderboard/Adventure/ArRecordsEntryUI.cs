using UnityEngine;
using UnityEngine.UI;

public class ArRecordsEntryUI : MonoBehaviour
{
    public Text rankText;
    public Text nameText;
    public Text scoreText;
    public Transform starContainer; // Parent transform for star icons

    private ArRecordsEntry arRecordsEntry; // Reference to the ArRecordsEntry instance

    private void Start()
    {
        // Assuming you have a reference to the ArRecordsEntry instance
        arRecordsEntry = FindObjectOfType<ArRecordsEntry>(); // Find the instance in the scene

        if (arRecordsEntry == null)
        {
            Debug.LogError("ArRecordsEntry instance not found.");
        }
    }

    // This method will update the star icons based on the player's score and totalQuestions
    public void DisplayStars(int score, int totalQuestions)
    {
        // Calculate the star count based on the player's score and totalQuestions
        int starCount = CalculateStarCount(score, totalQuestions);

        // Loop through the star container and activate/deactivate stars based on the star count
        for (int i = 0; i < starContainer.childCount; i++)
        {
            Transform star = starContainer.GetChild(i);
            star.gameObject.SetActive(i < starCount);
        }
    }

    // Calculate star count based on score and totalQuestions
    private int CalculateStarCount(int score, int totalQuestions)
    {
        float percentage = ((float)score / totalQuestions) * 100f;

        if (percentage >= 67f)
        {
            return 3; // 3 stars
        }
        else if (percentage >= 34f)
        {
            return 2; // 2 stars
        }
        else
        {
            return 1; // 1 star
        }
    }
}
