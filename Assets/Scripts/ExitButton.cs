using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExitButton : MonoBehaviour
{
    [Header("Confirmation Dialog")]
    [SerializeField]
    private GameObject confirmationDialog;

    public void OnExitButtonClicked()
    {
        // Show confirmation dialog
        confirmationDialog.SetActive(true);
    }

    public void OnConfirmExitButtonClicked()
    {
        // Exit the application
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    public void OnCancelExitButtonClicked()
    {
        // Hide confirmation dialog
        confirmationDialog.SetActive(false);
    }

}
