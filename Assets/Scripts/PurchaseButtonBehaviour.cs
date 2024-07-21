using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

enum PurchaseType
{
    Goober,
    Outfit,
    PotionIngredient
}

public class PurchaseButtonBehaviour : MonoBehaviour
{
    [SerializeField] private PurchaseType _purchaseType;
    [SerializeField] private GameObject _border;
    [SerializeField] private TMPro.TextMeshProUGUI _nameText;
    [SerializeField] private TMPro.TextMeshProUGUI _costText;

    [Header("Type Specific Variables")]
    [SerializeField] private int _index;

    private Button _button;
    
    private Unlockable CurrentData
    {
        get
        {
            switch (_purchaseType)
            {
                case PurchaseType.Goober:
                    return PersistentShopData.Instance.shopResources.goobers[_index];
                case PurchaseType.Outfit:
                    return PersistentShopData.Instance.shopResources.outfits[_index];
                case PurchaseType.PotionIngredient:
                    return PersistentShopData.Instance.shopResources.ingredients[_index];
                default:
                    return null;
            }
        }
    }

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
        if (CurrentData.isUnlocked && !CurrentData.unlimitedPurchases) { return; }
        if (!PersistentShopData.Instance.shopResources.CanAfford(CurrentData.unlockCost)) { return; }

        switch (_purchaseType)
        {
            case PurchaseType.Goober:
                PersistentShopData.Instance.shopResources.UnlockGoober(_index);
                AudioManager.Instance.PlaySound(ShopSoundByte.Click);
                break;
            case PurchaseType.Outfit:
                PersistentShopData.Instance.shopResources.UnlockOutfit(_index);
                AudioManager.Instance.PlaySound(ShopSoundByte.MajorPurchase);
                break;
            case PurchaseType.PotionIngredient:
                PersistentShopData.Instance.shopResources.PurchaseIngredient(_index);
                AudioManager.Instance.PlaySound(ShopSoundByte.Click);
                break;
            default:
                break;
        }
        SetEnableButton(!CurrentData.isUnlocked);
    }

    private void SetEnableButton(bool aIsEnabled)
    {
        _button.enabled = aIsEnabled;
        if (_border == null) { return; }
        _border.SetActive(!aIsEnabled);
    }

    public void SetIndex(int aIndex)
    {
        _index = aIndex;
    }
}
