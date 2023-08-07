using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdventureLevelPasser : MonoBehaviour
{

    public void NumberInputSet(int numberInput)
    {
        PlayerPrefs.SetInt("numberInput", numberInput);
        Debug.Log(numberInput);
        PlayerPrefs.Save();
    }

}
