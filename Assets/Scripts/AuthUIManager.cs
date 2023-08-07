using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class AuthUIManager : MonoBehaviour
{
    public static AuthUIManager instance;

    [Header("References")]
    [SerializeField]
    private GameObject checkingForAccountUI;
    [SerializeField]
    private GameObject loginUI;
    [SerializeField]
    private GameObject registerUI;
    [SerializeField]
    private GameObject forgotPasswordUI;
    [SerializeField]
    private GameObject outputGO;

    private void Awake() 
    {
        if (instance == null)
        {
            instance = this;        
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void ClearUI()
    {
        loginUI.SetActive(false);
        registerUI.SetActive(false);
        forgotPasswordUI.SetActive(false);
        outputGO.SetActive(false);
        checkingForAccountUI.SetActive(false);
        FirebaseManager.instance.ClearOuputs();
    }

    public void LoginScreen() 
    {
        ClearUI();
        loginUI.SetActive(true);
    }

    public void RegisterScreen() 
    {
        ClearUI();
        registerUI.SetActive(true);
    }

    public void ForgotPassScreen() 
    {
        ClearUI();
        forgotPasswordUI.SetActive(true);
    }

    public void OutputGoExit() 
    {
        outputGO.SetActive(false);
    }

    public GameObject loaderUI;
    public Slider progressSlider;

    public void LoadScene(int index) 
    {
        StartCoroutine(LoadScene_Coroutine(index));
    }

    public IEnumerator LoadScene_Coroutine(int index)
    {
        progressSlider.value = 0;
        loaderUI.SetActive(true);

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(index);
        asyncOperation.allowSceneActivation = false;
        float progress = 0;
        while (!asyncOperation.isDone) {

            progress = Mathf.MoveTowards(progress, asyncOperation.progress, Time.deltaTime);
            progressSlider.value = progress;
            if (progress >= 0.9f) {

                progressSlider.value = 1;
                asyncOperation.allowSceneActivation = true;
            
            
            }
            yield return null;
        
        }

    }
}
