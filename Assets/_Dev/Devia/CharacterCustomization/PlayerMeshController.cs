using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMeshController : MonoBehaviour
{
    [SerializeField] private string _characterPartBaseName = "character:c_";
    [SerializeField] private Transform _defaultBody;
    [SerializeField] private Transform _characterParts;
    
    private SelectedItems _currentlySelectedItems;
    public SelectedItems CurrentlySelectedItems => _currentlySelectedItems;

    public void LoadCharacter()
    {
        _currentlySelectedItems.bodyName = PlayerPrefs.GetString("BodyName", "");
        _currentlySelectedItems.hairName = PlayerPrefs.GetString("HairName", "");
        _currentlySelectedItems.accessoriesName = PlayerPrefs.GetString("AccessoriesName", "");

        UpdateCharacter(_currentlySelectedItems);
    }

    private void Awake()
    {
        LoadCharacter();
    }

    public void UpdateCharacter(SelectedItems aSelectedItems)
    {
        if (_characterParts == null) { return; }

        for (int i = 0; i < _characterParts.childCount; i++)
        {
            _characterParts.GetChild(i).gameObject.SetActive(false);
        }
        _defaultBody.gameObject.SetActive(true);

        _currentlySelectedItems = aSelectedItems;

        SetEquipment(_currentlySelectedItems.bodyName, true);
        SetEquipment(_currentlySelectedItems.hairName, true);
        SetEquipment(_currentlySelectedItems.accessoriesName, true);
    }

    private void SetEquipment(string aName, bool aActive)
    {
        if (aName == "") { return; }

        string fullName = _characterPartBaseName + aName;
        _characterParts.Find(fullName)?.gameObject.SetActive(aActive);
    }

    public void SaveCharacter()
    {
        PlayerPrefs.SetString("BodyName", _currentlySelectedItems.bodyName);
        PlayerPrefs.SetString("HairName", _currentlySelectedItems.hairName);
        PlayerPrefs.SetString("AccessoriesName", _currentlySelectedItems.accessoriesName);
        PlayerPrefs.Save();
    }
}
