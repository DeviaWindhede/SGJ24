using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopCurrencyText : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI _text;

    void Awake()
    {
        _text = GetComponent<TMPro.TextMeshProUGUI>();

        _text.text = PersistentShopData.Instance.shopResources.CoinAmount.ToString();
        PersistentShopData.Instance.shopResources.OnCurrencyChangeEvent += OnCurrencyChangeEvent;
    }

    private void OnDestroy()
    {
        PersistentShopData.Instance.shopResources.OnCurrencyChangeEvent -= OnCurrencyChangeEvent;
    }

    private void OnCurrencyChangeEvent(int aAmount)
    {
        _text.text = aAmount.ToString();
    }
}