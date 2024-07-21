using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralButtonBehaviour : MonoBehaviour
{
    public void HideElement(GameObject aObject)
    {
        AudioManager.Instance.PlaySound(ShopSoundByte.Click);
        aObject.SetActive(false);
    }

    public void ShowElement(GameObject aObject)
    {
        AudioManager.Instance.PlaySound(ShopSoundByte.Click);
        aObject.SetActive(true);
    }

    public void NavigateToScene(string aSceneName)
    {
        AudioManager.Instance.PlaySound(ShopSoundByte.Click);
        UnityEngine.SceneManagement.SceneManager.LoadScene(aSceneName);
    }

    public void ExitGame()
    {
        AudioManager.Instance.PlaySound(ShopSoundByte.Click);
        Application.Quit();
    }
}
