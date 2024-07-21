using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PurchaseGooberButtonBehaviour : MonoBehaviour
{
    [SerializeField] private GameObject _border;
    [SerializeField] private TMPro.TextMeshProUGUI _nameText;
    [SerializeField] private TMPro.TextMeshProUGUI _costText;

    private Button _button;
    private int _gooberDataIndex;
    
    private GooberData CurrentData => PersistentShopData.Instance.shopResources.goobers[_gooberDataIndex];

    void Start()
    {
        _button = GetComponent<Button>();
        
        _nameText.text = CurrentData.name;
        _costText.text = CurrentData.unlockCost.ToString();

        SetEnableButton(!CurrentData.isUnlocked);

        _button.onClick.AddListener(OnButtonClicked);
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        if (CurrentData.isUnlocked) { return; }
        if (!PersistentShopData.Instance.shopResources.CanAfford(CurrentData.unlockCost)) { return; }
    
        PersistentShopData.Instance.shopResources.UnlockGoober(_gooberDataIndex);
        SetEnableButton(!CurrentData.isUnlocked);
    }

    private void SetEnableButton(bool aIsEnabled)
    {
        _button.enabled = aIsEnabled;
        _border.SetActive(!aIsEnabled);
    }

    public void SetGooberData(int aGooberDataIndex)
    {
        _gooberDataIndex = aGooberDataIndex;
    }
}
