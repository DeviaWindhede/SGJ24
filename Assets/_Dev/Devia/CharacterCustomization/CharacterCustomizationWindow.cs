using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

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


    private UnityEngine.UI.ScrollRect _scrollRect;
    private CategoryType _currentCategory = 0;

    Transform GetGrid(CategoryType aType)
    {
        return _grindMask.transform.GetChild((int)aType);
    }

    void OnCategorySelected(CategoryType aType)
    {
        GetGrid(_currentCategory).gameObject.SetActive(false);
        GetGrid(aType).gameObject.SetActive(true);

        _categories[(int)_currentCategory].camera.gameObject.SetActive(false);
        _categories[(int)aType].camera.gameObject.SetActive(true);

        _currentCategory = aType;
        _scrollRect.content = GetGrid(aType).GetComponent<RectTransform>();
        _scrollRect.normalizedPosition = new(0, _scrollRect.normalizedPosition.y);

        _categoryTitle.text = aType.ToString();
        //var brain = Camera.main.GetComponent<Cinemachine.CinemachineBrain>();
        //_cameras
    }

    // Start is called before the first frame update
    void Start()
    {
        _fullBodyCamera.gameObject.SetActive(false);
        for (int i = 0; i < _categories.Count; i++)
        {
            _categories[i].camera.gameObject.SetActive(false);
        }

        _scrollRect = GetComponent<UnityEngine.UI.ScrollRect>();

        _confirmButton.onClick.AddListener(OnConfirmButtonPressed);

        InstantiateContainers();
        InstantiateItems();

        OnCategorySelected(_currentCategory);
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
            var button = go.GetComponent<UnityEngine.UI.Button>();
            button.onClick.AddListener(() => OnCategorySelected(type));
        }
    }

    private void InstantiateItems()
    {
        List<CharacterCustomItem> items = Resources.LoadAll<CharacterCustomItem>("CustomCharacterData").ToList();

        for (int i = 0; i < items.Count; i++)
        {
            var go = Instantiate(_categoryItemPrefab, GetGrid(items[i].type));
            var item = go.GetComponent<CustomizationCategoryItem>();
            item.Init();
            item.SetItem(items[i]);
        }
    }

    private void OnConfirmButtonPressed()
    {
        print("Confirm button pressed");
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
    } 
}
