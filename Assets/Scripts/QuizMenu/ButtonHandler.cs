// ButtonHandler.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonHandler : MonoBehaviour
{
    public GameObject confirmationDialog;
    private ConfirmationDialog dialogScript;
    public int exitLocation;

    private void Start()
    {
        dialogScript = confirmationDialog.GetComponent<ConfirmationDialog>();
        dialogScript.exitButton.onClick.AddListener(DestroyCurrentSceneAndRedirectToLeaderboard);
        dialogScript.cancelButton.onClick.AddListener(dialogScript.HideDialog);
    }

    public void OnLeaderboardButtonClick()
    {
        dialogScript.ShowDialog();
    }

    public void OnLeaveQuizButtonClick()
    {
        dialogScript.exitButton.onClick.RemoveAllListeners();
        dialogScript.exitButton.onClick.AddListener(DestroyCurrentSceneAndRedirectToScene4);
        dialogScript.ShowDialog();
    }

    public void OnAboutUsButtonClick()
    {
        dialogScript.exitButton.onClick.RemoveAllListeners();
        dialogScript.exitButton.onClick.AddListener(DestroyCurrentSceneAndRedirectToAboutUs);
        dialogScript.ShowDialog();
    }

    private void DestroyCurrentSceneAndRedirectToLeaderboard()
    {
        DestroyCurrentScene();
        SceneManager.LoadScene(9);
    }

    private void DestroyCurrentSceneAndRedirectToScene4()
    {
        DestroyCurrentScene();
        SceneManager.LoadScene(exitLocation);
    }

    private void DestroyCurrentSceneAndRedirectToAboutUs()
    {
        DestroyCurrentScene();
        SceneManager.LoadScene(11);
    }

    private void DestroyCurrentScene()
    {
        // Unload the current scene
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
    }
}
