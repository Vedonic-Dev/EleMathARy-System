using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderboardUI : MonoBehaviour
{

    [SerializeField]
    private GameObject simpleLeaderboardUI;
    [SerializeField]
    private GameObject adventureLeaderboardUI;

    private void ClearUI()
    {
        simpleLeaderboardUI.SetActive(false);
        adventureLeaderboardUI.SetActive(false);
    }

    public void simpleLeaderboard() 
    {
        ClearUI();
        simpleLeaderboardUI.SetActive(true);
    }

    public void adventureLeaderboard() 
    {
        ClearUI();
        adventureLeaderboardUI.SetActive(true);
    }
}
