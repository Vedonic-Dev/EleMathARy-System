using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AdventureNextLessonUI : MonoBehaviour
{
    public GameObject[] gameObjects;
    private int currentIndex;
    [SerializeField] private Button nextButton;
    public GameObject loaderUI;
    public Slider progressSlider;

    private void Start()
    {
        AdventureLevelLoader levelLoader = FindObjectOfType<AdventureLevelLoader>();
        currentIndex = levelLoader.numberInput;
        gameObjects[currentIndex].SetActive(true);
        nextButton.onClick.AddListener(NextUI);
    }

    private void ClearUI()
    {
        foreach (GameObject gameObject in gameObjects)
        {
            gameObject.SetActive(false);
        }
    }

    public void NextUI() 
    {
        currentIndex++;
        ClearUI();

        if (currentIndex < gameObjects.Length)
        {
            StartCoroutine(ShowProgressBar());
        }
        else
        {
            currentIndex = 0;
            StartCoroutine(ShowProgressBar());
        }
    }

    private IEnumerator ShowProgressBar()
    {
        // Show the loader UI
        loaderUI.SetActive(true);

        // Reset the progress slider
        progressSlider.value = 0;

        // Calculate the target progress value
        float targetProgress = 1.0f;

        // Calculate the increment per second
        float incrementPerSecond = 1.0f / 1.5f;

        float currentProgress = 0;

        // Animate the progress slider filling up over time
        while (currentProgress < targetProgress)
        {
            currentProgress += incrementPerSecond * Time.deltaTime;
            progressSlider.value = currentProgress;
            yield return null;
        }

        // Hide the loader UI
        loaderUI.SetActive(false);

        // Set the next game object active
        gameObjects[currentIndex].SetActive(true);
    }

}
