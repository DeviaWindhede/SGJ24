using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.UI;
using UnityEngine;

public class CustomizationCategoryItem : MonoBehaviour
{
    private CharacterCustomizationWindow _window;
    private CharacterCustomItem _item;
    private UnityEngine.UI.Button _button;
    private UnityEngine.UI.Image _border;
    private UnityEngine.UI.Image _icon;

    public void Init(CharacterCustomItem aCustomItem, CharacterCustomizationWindow aWindow)
    {
        _window = aWindow;
        _button = GetComponent<UnityEngine.UI.Button>();
        _border = GetComponent<UnityEngine.UI.Image>();
        _icon = transform.GetChild(0).GetComponent<UnityEngine.UI.Image>();

        _button.onClick.AddListener(OnClick);

        _item = aCustomItem;
        _icon.sprite = _item.itemSprite;
        name = _item.itemName;
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveListener(OnClick);
    }

    public void OnClick()
    {
        _window.SetItem(_item.type, _item.index);
    }
}
