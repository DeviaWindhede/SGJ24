using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.UI;
using UnityEngine;

public class CustomizationCategoryItem : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Image _border;
    [SerializeField] private UnityEngine.UI.Image _icon;

    private CharacterCustomizationWindow _window;
    private CharacterCustomItem _item;
    private UnityEngine.UI.Button _button;

    public UnityEngine.UI.Image Border => _border;

    public void Init(CharacterCustomItem aCustomItem, CharacterCustomizationWindow aWindow, bool aIsSelected)
    {
        _window = aWindow;
        _button = GetComponent<UnityEngine.UI.Button>();
        _button.onClick.AddListener(OnClick);

        _item = aCustomItem;
        _icon.sprite = _item.itemSprite;
        name = _item.itemName;
        _border.gameObject.SetActive(aIsSelected);
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveListener(OnClick);
    }

    public void OnClick()
    {
        _window.SetItem(_item.type, _item.skeletonPartName, _border.gameObject);
    }
}
