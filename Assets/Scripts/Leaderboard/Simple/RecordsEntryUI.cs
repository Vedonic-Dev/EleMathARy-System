using UnityEngine;
using UnityEngine.UI;

public class RecordsEntryUI : MonoBehaviour
{
    public Text rankText;
    public Text nameText;
    public Text scoreText;
    public Transform starContainer; // Parent transform for star icons

    // This method will update the star icons based on the player's score
    public void DisplayStars(int score)
    {
        // Calculate the star count based on the player's score
        int starCount = CalculateStarCount(score);

        // Loop through the star container and activate/deactivate stars based on the star count
        for (int i = 0; i < starContainer.childCount; i++)
        {
            Transform star = starContainer.GetChild(i);
            star.gameObject.SetActive(i < starCount);
        }
    }

    // Calculate star count based on score (using the provided logic)
    private int CalculateStarCount(int score)
    {
        float percentage = ((float)score / 10f) * 100f;

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
