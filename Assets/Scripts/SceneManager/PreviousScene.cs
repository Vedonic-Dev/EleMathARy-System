using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PreviousScene : MonoBehaviour
{
    // Save the current scene index to PlayerPrefs
    private void Start()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        PlayerPrefs.SetInt("PreviousSceneIndex", currentSceneIndex);
        PlayerPrefs.Save();
    }
}
