using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMeshController : MonoBehaviour
{
    [SerializeField] private Transform _characterParts;
    
    private SelectedItems _currentlySelectedItems;
    public SelectedItems CurrentlySelectedItems => _currentlySelectedItems;

    public void LoadCharacter()
    {
        _currentlySelectedItems.bodyIndex = PlayerPrefs.GetInt("BodyIndex", 2);
        _currentlySelectedItems.hairIndex = PlayerPrefs.GetInt("HairIndex", -1);
        _currentlySelectedItems.accessoriesIndex = PlayerPrefs.GetInt("AccessoriesIndex", -1);

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
        _characterParts.GetChild(2).gameObject.SetActive(true);

        _currentlySelectedItems = aSelectedItems;

        if (_currentlySelectedItems.bodyIndex >= 0)
            _characterParts.GetChild(_currentlySelectedItems.bodyIndex).gameObject.SetActive(true);

        if (_currentlySelectedItems.hairIndex >= 0)
            _characterParts.GetChild(_currentlySelectedItems.hairIndex).gameObject.SetActive(true);

        if (_currentlySelectedItems.accessoriesIndex >= 0)
            _characterParts.GetChild(_currentlySelectedItems.accessoriesIndex).gameObject.SetActive(true);
    }

    public void SaveCharacter()
    {
        PlayerPrefs.SetInt("BodyIndex", _currentlySelectedItems.bodyIndex);
        PlayerPrefs.SetInt("HairIndex", _currentlySelectedItems.hairIndex);
        PlayerPrefs.SetInt("AccessoriesIndex", _currentlySelectedItems.accessoriesIndex);
        PlayerPrefs.Save();
    }
}
