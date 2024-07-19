using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralButtonBehaviour : MonoBehaviour
{
    public void HideElement(GameObject aObject)
    {
        aObject.SetActive(false);
    }

    public void ShowElement(GameObject aObject)
    {
        aObject.SetActive(true);
    }

    public void NavigateToScene(string aSceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(aSceneName);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
