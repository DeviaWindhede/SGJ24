using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageGoober : MonoBehaviour
{
    private List<GameObject> _goobers = new();

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            _goobers.Add(transform.GetChild(i).gameObject);
            _goobers[i].SetActive(false);
        }
    }

    public void UpdateGoober(int aIndex)
    {
        bool isUnlocked = PersistentShopData.Instance.shopResources.goobers[aIndex].isUnlocked;
        bool isClaimed = PersistentShopData.Instance.shopResources.goobers[aIndex].isClaimed;
        _goobers[aIndex].SetActive(isUnlocked && !isClaimed);
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < _goobers.Count; i++)
        {
            UpdateGoober(i);
        }
    }
}
