using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdventureChooseStageUI : MonoBehaviour
{

    [SerializeField]
    private GameObject oneToTenUI;
    [SerializeField]
    private GameObject elevenToTwentyUI;

    private void ClearUI()
    {
        oneToTenUI.SetActive(false);
        elevenToTwentyUI.SetActive(false);
    }

    public void oneToTen() 
    {
        ClearUI();
        oneToTenUI.SetActive(true);
    }

    public void elevenToTwenty() 
    {
        ClearUI();
        elevenToTwentyUI.SetActive(true);
    }
}
