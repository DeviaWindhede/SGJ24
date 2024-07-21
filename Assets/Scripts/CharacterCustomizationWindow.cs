using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public enum CategoryType
{
    Body,
    Hair,
    Accessories
}

[System.Serializable]
struct CategoryUI
{
    public CategoryType type;
    public Cinemachine.CinemachineVirtualCamera camera;
}

[System.Serializable]
public struct SelectedItems
{
    public string bodyName;
    public string hairName;
    public string accessoriesName;
}

public class CharacterCustomizationWindow : MonoBehaviour
{
    [SerializeField] private Cinemachine.CinemachineVirtualCamera _fullBodyCamera;
    [SerializeField] private TextMeshProUGUI _categoryTitle;
    [SerializeField] private List<CategoryUI> _categories;
    [SerializeField] private GameObject _grindMask;
    [SerializeField] private GameObject _grindContainerPrefab;
    [SerializeField] private GameObject _categoryContainer;
    [SerializeField] private GameObject _categoryTypePrefab;
    [SerializeField] private GameObject _categoryItemPrefab;
    [SerializeField] private UnityEngine.UI.Button _confirmButton;
    [SerializeField] private UnityEngine.UI.Button _revertButton;
    [SerializeField] private SelectedItems _selectedItems;

    [SerializeField] private Color _categorySelectedColor;
    [SerializeField] private Color _categoryUnselectedColor;

    private Dictionary<CategoryType, GameObject> _borders = new();
    private List<Button> _categoryButtons = new();
    private PlayerMeshController _playerMeshController;
    private CategoryType _currentCategory = 0;

    public void SetItem(CategoryType aType, string aBoneNameEnding, GameObject aBorder)
    {
        _borders[aType]?.SetActive(false);
        switch (aType)
        {
            case CategoryType.Body:
                _selectedItems.bodyName = aBoneNameEnding;
                break;
            case CategoryType.Hair:
                _selectedItems.hairName = aBoneNameEnding;
                break;
            case CategoryType.Accessories:
                _selectedItems.accessoriesName = aBoneNameEnding;
                break;
            default:
                break;
        }
        _borders[aType] = aBorder;
        _borders[aType].SetActive(true);

        _playerMeshController.UpdateCharacter(_selectedItems);
    }
    Transform GetGrid(CategoryType aType)
    {
        return _grindMask.transform.GetChild((int)aType);
    }

    void OnCategorySelected(CategoryType aType, bool aShouldLayoutGroupBeEnabled = false)
    {
        GetGrid(_currentCategory).gameObject.SetActive(false);
        GetGrid(aType).gameObject.SetActive(true);

        _categories[(int)_currentCategory].camera.gameObject.SetActive(false);
        _categories[(int)aType].camera.gameObject.SetActive(true);

        _categoryContainer.GetComponent<HorizontalLayoutGroup>().enabled = aShouldLayoutGroupBeEnabled;
        for (int i = 0; i < _categoryButtons.Count; i++)
        {
            _categoryButtons[i].transform.SetSiblingIndex(i);
            _categoryButtons[i].GetComponent<Image>().color = _categoryUnselectedColor;
        }

        _currentCategory = aType;
        _categoryButtons[(int)_currentCategory].GetComponent<Image>().color = _categorySelectedColor;
        _categoryButtons[(int)_currentCategory].transform.SetAsLastSibling();

        _categoryTitle.text = aType.ToString();
    }

    private void Start()
    {
        _playerMeshController = FindObjectOfType<PlayerMeshController>();
        _fullBodyCamera.gameObject.SetActive(false);
        for (int i = 0; i < _categories.Count; i++)
        {
            _categories[i].camera.gameObject.SetActive(false);
        }

        _confirmButton.onClick.AddListener(OnConfirmButtonPressed);
        _revertButton.onClick.AddListener(OnRevertButtonPressed);

        StartCoroutine(Initialize());
    }

    private IEnumerator Initialize()
    {
        InstantiateContainers();
        InstantiateItems();

        OnCategorySelected((CategoryType)(Enum.GetValues(typeof(CategoryType)).Length - 1), true);

        yield return null;

        OnCategorySelected(CategoryType.Body, false);
    }

    private void InstantiateContainers()
    {
        for (int i = 0; i < _categories.Count; i++)
        {
            var root = Instantiate(_grindContainerPrefab, _grindMask.transform);
            root.SetActive(false);
            root.name = _categories[i].type.ToString();

            CategoryType type = _categories[i].type;
            var go = Instantiate(_categoryTypePrefab, _categoryContainer.transform);
            var button = go.GetComponent<Button>();
            button.GetComponent<Image>().color = _categoryUnselectedColor;
            button.onClick.AddListener(() => OnCategorySelected(type));

            _categoryButtons.Add(button);
        }
    }

    private void InstantiateItems()
    {
        List<CharacterCustomItem> items = Resources.LoadAll<CharacterCustomItem>("CustomCharacterData").ToList();

        _selectedItems = _playerMeshController.CurrentlySelectedItems;

        _borders.Add(CategoryType.Body, null);
        _borders.Add(CategoryType.Hair, null);
        _borders.Add(CategoryType.Accessories, null);

        for (int i = 0; i < items.Count; i++)
        {
            var go = Instantiate(_categoryItemPrefab, GetGrid(items[i].type));
            var item = go.GetComponent<CustomizationCategoryItem>();

            bool isSelected = false;
            switch (items[i].type)
            {
                case CategoryType.Body:
                    isSelected = _selectedItems.bodyName == items[i].skeletonPartName;
                    break;
                case CategoryType.Hair:
                    isSelected = _selectedItems.hairName == items[i].skeletonPartName;
                    break;
                case CategoryType.Accessories:
                    isSelected = _selectedItems.accessoriesName == items[i].skeletonPartName;
                    break;
                default:
                    break;
            }

            bool isLocked = items[i].setType != ClothingSetType.None && !PersistentShopData.Instance.shopResources.outfits[(int)items[i].setType].isUnlocked;
            item.Init(items[i], this, isSelected, isLocked);

            if (isSelected)
            {
                _borders[items[i].type] = item.Border.gameObject;
            }
        }
    }

    private void OnConfirmButtonPressed()
    {
        _playerMeshController.SaveCharacter();
        LoadShopScene();
    }

    private void OnRevertButtonPressed()
    {
        _playerMeshController.LoadCharacter();
        _selectedItems = _playerMeshController.CurrentlySelectedItems;
    }

    private void LoadShopScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("ShopScene");
    }

    private void OnDestroy()
    {
        for (int i = 0; i < _categories.Count; i++)
        {
            CategoryType type = _categories[i].type;
            var button = _categoryContainer.transform.GetChild(i).GetComponent<UnityEngine.UI.Button>();
            button.onClick.RemoveListener(() => OnCategorySelected(type));
        }
        _confirmButton.onClick.RemoveListener(OnConfirmButtonPressed);
        _revertButton.onClick.RemoveListener(OnRevertButtonPressed);
    } 
}
