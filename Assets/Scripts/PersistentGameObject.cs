using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistentGameObject : MonoBehaviour
{
    void Start()
    {
        var objs = FindObjectsByType<PersistentGameObject>(FindObjectsSortMode.None);

        foreach (var obj in objs)
        {
            if (obj.gameObject == gameObject) { continue; }

            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(transform.gameObject);   
    }
}
