using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputerArkenGoobButtonBehaviour : MonoBehaviour
{
    [SerializeField] private GameObject _purchaseButtonPrefab;

    void Start()
    {
        for (int i = 0; i < PersistentShopData.Instance.shopResources.goobers.Count; i++)
        {
            var goober = PersistentShopData.Instance.shopResources.goobers[i];
            var button = Instantiate(_purchaseButtonPrefab, transform);
            var buttonBehaviour = button.GetComponent<PurchaseButtonBehaviour>();
            buttonBehaviour.SetIndex(i);
        }
    }
}
