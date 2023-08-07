using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitScene : MonoBehaviour
{

    // Load the previous scene
    public void GoBack()
    {
        int previousSceneIndex = PlayerPrefs.GetInt("PreviousSceneIndex");
        SceneManager.LoadScene(previousSceneIndex);
    }
}
