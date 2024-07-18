using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralButtonBehaviour : MonoBehaviour
{
    public void NavigateToScene(string aSceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(aSceneName);
    }
}
