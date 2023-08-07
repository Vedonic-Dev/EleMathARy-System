// ConfirmationDialog.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ConfirmationDialog : MonoBehaviour
{
    public GameObject dialogPanel;
    public Button exitButton;
    public Button cancelButton;

    private UnityAction exitAction;

    public void Setup(UnityAction exitAction)
    {
        this.exitAction = exitAction;
    }

    public void ShowDialog()
    {
        dialogPanel.SetActive(true);
    }

    public void HideDialog()
    {
        dialogPanel.SetActive(false);
    }

    public void OnExitButtonClick()
    {
        exitAction?.Invoke();
    }

    public void OnCancelButtonClick()
    {
        HideDialog();
    }
}
