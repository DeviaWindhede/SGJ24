using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomizationCategoryItem : MonoBehaviour
{
    private UnityEngine.UI.Button _button;
    private UnityEngine.UI.Image _border;
    private UnityEngine.UI.Image _icon;

    public void Init()
    {
        _button = GetComponent<UnityEngine.UI.Button>();
        _border = GetComponent<UnityEngine.UI.Image>();
        _icon = transform.GetChild(0).GetComponent<UnityEngine.UI.Image>();

        _button.onClick.AddListener(OnClick);
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveListener(OnClick);
    }

    public void OnClick()
    {
        print($"Wow you clicked me! {name}");
    }

    public void SetItem(CharacterCustomItem aItem)
    {
        _icon.sprite = aItem.itemSprite;
        name = aItem.itemName;
    }
}
