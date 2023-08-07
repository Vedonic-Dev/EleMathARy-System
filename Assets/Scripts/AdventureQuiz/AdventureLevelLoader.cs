using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AdventureLevelLoader : MonoBehaviour
{
    public GameObject[] gameObjects;
    public int numberInput;

    private void Start()
    {
        numberInput = PlayerPrefs.GetInt("numberInput");
        Debug.Log(numberInput);
        ActivateGameObject(numberInput);
    }

    void ActivateGameObject(int sceneNumber)
    {
        if (sceneNumber >= 0 && sceneNumber < gameObjects.Length)
        {
            GameObject targetObject = gameObjects[sceneNumber];
            Debug.Log("Activating GameObject: " + targetObject.name);

            if (targetObject != null)
            {
                targetObject.SetActive(true);
                Debug.Log("GameObject activated successfully.");
            }
            else
            {
                Debug.LogError("Target GameObject is null.");
            }
        }
        else
        {
            Debug.LogError("Invalid sceneNumber: " + sceneNumber);
        }
    }

}
