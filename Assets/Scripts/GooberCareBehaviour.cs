using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[System.Serializable]
class GooberState
{
    public Vector2Int index;

    public float petMultiplier = 0.0f;
    public float dirtiness = 0.0f;
    public bool shouldIgnore = false;
    public bool IsDirty => dirtiness < 0.9f;
}

public enum GooberType
{
    PetTool,
    SoapSponge,
    Nothing
}

public class GooberCareBehaviour : MonoBehaviour
{
    public delegate void OnFinishedSession();
    public event OnFinishedSession FinishedSession;

    [SerializeField] private Camera _initalRayCamera;
    [SerializeField] private ProgressBar _progressBar;
    [SerializeField] private float _cleanlinessThreshold = 0.9f;
    [SerializeField] private Button _washButton;
    [SerializeField] private Button _petButton;

    [SerializeField] private Transform _gooberCollection;
    [SerializeField] private GameObject _petToolPrefab;
    [SerializeField] private GameObject _washToolPrefab;

    [SerializeField, Min(1)] private int _gridWidth;
    [SerializeField, Min(1)] private int _gridHeight;
    [SerializeField] private float _cleaningSpeed;
    [SerializeField] private float _pettingSpeed;

    private List<GameObject> _washToolList = new();
    [SerializeField] private List<GooberState> activeStates = new();
    private List<List<GooberState>> grid = new();

    private PlayerInputActions _inputActions;
    private Camera _camera;
    private PixelCamRaycast _pixelCamRay;
    private bool _hasCalledFinishedSession = false;
    private Vector2 _mouseDelta;
    private bool _isMouseDown;
    private GooberType _currentTool;
    private float _petPercentage;
    private float _cleanlinessPercentage;
    public float HappinessPercentage => (_petPercentage + _cleanlinessPercentage) / 2.0f;
    private void CalculateCleanliness()
    {
        int total = 0;
        int clean = 0;
        for (int i = 0; i < grid.Count; i++)
        {
            for (int j = 0; j < grid[i].Count; j++)
            {
                if (grid[i][j].shouldIgnore) { continue; }
                ++total;
                clean += grid[i][j].IsDirty ? 0 : 1;
            }
        }
        _cleanlinessPercentage = (float)clean / (float)total;

        if (_cleanlinessPercentage >= _cleanlinessThreshold)
        {
            _cleanlinessPercentage = 1.0f;
        }

        if (_progressBar != null)
        {
            _progressBar.SetTargetFillPercentage(HappinessPercentage);
        }
    }


    private void SetSoapAsTool()
    {
        print("holy moly");
        SetUpdateTool(GooberType.SoapSponge);
    }

    private void SetPetAsTool()
    {
        _washToolList[(int)GooberType.SoapSponge].transform.GetChild(0).GetComponent<ParticleSystem>().enableEmission = false;
        SetUpdateTool(GooberType.PetTool);
    }

    // Start is called before the first frame update
    void Start()
    {
        _pixelCamRay = FindObjectOfType<PixelCamRaycast>();
        _camera = _pixelCamRay.GetComponent<Camera>();
        {
            int childIndex = UnityEngine.Random.Range(0, _gooberCollection.childCount);
            _gooberCollection.GetChild(childIndex).gameObject.SetActive(true);
        }

        _washButton.onClick.AddListener(SetSoapAsTool);
        _petButton.onClick.AddListener(SetPetAsTool);

        _washToolList.Add(Instantiate(_petToolPrefab));
        _washToolList.Add(Instantiate(_washToolPrefab));
        _washToolList[(int)GooberType.SoapSponge].transform.GetChild(0).GetComponent<ParticleSystem>().enableEmission = false;

        foreach (var item in _washToolList)
        {
            item.SetActive(false);
        }

        _inputActions = new();
        _inputActions.Enable();
        _inputActions.MiniGameControls.Enable();
        _inputActions.MiniGameControls.MouseDown.performed += ctx => { _isMouseDown = true; };
        _inputActions.MiniGameControls.MouseDown.canceled += ctx => { _isMouseDown = false; };
        _inputActions.MiniGameControls.MouseMove.performed += ctx => { _mouseDelta = ctx.ReadValue<Vector2>(); };
        _inputActions.MiniGameControls.MouseMove.canceled += ctx => { _mouseDelta = Vector2.zero; };


        // min = x, max = y
        Vector2Int minMaxHeight = new();
        Vector2Int minMaxWidth = new();

        for (int x = 0; x < _gridWidth; x++)
        {
            grid.Add(new List<GooberState>());
            for (int y = 0; y < _gridHeight; y++)
            {
                grid[x].Add(new GooberState());
                grid[x][y].shouldIgnore = true;
                grid[x][y].index = new Vector2Int(x, y);

                Vector3 viewport = CellToViewport(new(x, y));
                Ray ray = _initalRayCamera.ViewportPointToRay(viewport);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    if (!hit.transform.CompareTag("PlayerInteractable")) { continue; }

                    if (minMaxHeight.x > y) { minMaxHeight.x = y; }
                    if (minMaxHeight.y < y) { minMaxHeight.y = y; }
                    if (minMaxWidth.x > x) { minMaxWidth.x = x; }
                    if (minMaxWidth.y < x) { minMaxWidth.y = x; }
                    grid[x][y].shouldIgnore = false;
                    activeStates.Add(grid[x][y]);
                }
            }
        }

        if (activeStates.Count == 0)
        {
            Debug.LogError("No goober found!!! D:");
            return;
        }

        int randomIndex = UnityEngine.Random.Range(0, activeStates.Count);
        activeStates[randomIndex].petMultiplier = 2.0f;

        Vector2Int startingIndex = activeStates[randomIndex].index;
        for (int i = 0; i < activeStates.Count; i++)
        {
            if (randomIndex == i) { continue; }

            int xIndex = activeStates[i].index.x - minMaxWidth.x;
            int yIndex = activeStates[i].index.y - minMaxHeight.x;

            float xDistance = (float)xIndex / (float)(minMaxWidth.y - minMaxWidth.x);
            float yDistance = (float)yIndex / (float)(minMaxHeight.y - minMaxHeight.x);

            activeStates[i].petMultiplier = 2.0f - (xDistance / yDistance);
        }
    }

    private void OnDestroy()
    {
        _washButton.onClick.RemoveAllListeners();
        _petButton.onClick.RemoveAllListeners();

        _inputActions.MiniGameControls.MouseDown.performed -= ctx => { _isMouseDown = true; };
        _inputActions.MiniGameControls.MouseDown.canceled -= ctx => { _isMouseDown = false; };
        _inputActions.MiniGameControls.MouseMove.performed -= ctx => { _mouseDelta = ctx.ReadValue<Vector2>(); };
        _inputActions.MiniGameControls.MouseMove.canceled -= ctx => { _mouseDelta = Vector2.zero; };
        _inputActions.MiniGameControls.Disable();
        _inputActions.Disable();
    }

    public void SetUpdateTool(GooberType aTool)
    {
        _washToolList[(int)_currentTool].SetActive(false);
        _currentTool = aTool;
        _washToolList[(int)_currentTool].SetActive(true);
    }

    private void Update()
    {
        switch (_currentTool)
        {
            case GooberType.SoapSponge:
                UpdateWashTool();
                break;
            case GooberType.PetTool:
                UpdatePetTool();
                break;
            default:
                break;
        }

        if (FinishedSession != null && !_hasCalledFinishedSession && HappinessPercentage >= 1.0f)
        {
            FinishedSession();
            _hasCalledFinishedSession = true;
        }

        _progressBar.SetTargetFillPercentage(HappinessPercentage);
    }

    void UpdateVisualToolLocation(GameObject aGameObject, RaycastHit aHit)
    {
        if (aHit.transform == null)
        {
            Vector3 mousePos = new Vector3(
                    Input.mousePosition.x,
                    Input.mousePosition.y,
                    _camera.transform.position.z
            );
            Transform t = _washToolList[(int)_currentTool].transform;
            t.SetPositionAndRotation(_initalRayCamera.ScreenToWorldPoint(mousePos), Quaternion.identity);
            return;
        }

        _washToolList[(int)_currentTool].transform.position = aHit.point;
        _washToolList[(int)_currentTool].transform.rotation = Quaternion.LookRotation(aHit.normal);
    }

    private void UpdateWashTool()
    {
        Ray ray = _pixelCamRay.GetRay(Input.mousePosition);
        RaycastHit hit;

        bool result = Physics.Raycast(ray, out hit);
        UpdateVisualToolLocation(_washToolList[(int)_currentTool], hit);

        if (!result) { return; }
        if (!_isMouseDown)
        {
            _washToolList[(int)GooberType.SoapSponge].transform.GetChild(0).GetComponent<ParticleSystem>().enableEmission = false;
            return;
        }
        if (_mouseDelta.magnitude < 0.1f) { return; }

        Vector3 cell = _camera.WorldToViewportPoint(hit.point);
        cell.x = cell.x * _gridWidth;
        cell.y = cell.y * _gridHeight;

        var c = grid[(int)cell.x][(int)cell.y];
        if (c.shouldIgnore) { return; }

        c.dirtiness += _cleaningSpeed * Time.deltaTime;
        if (c.dirtiness > 1.0f) { c.dirtiness = 1.0f; }

        _washToolList[(int)GooberType.SoapSponge].transform.GetChild(0).GetComponent<ParticleSystem>().enableEmission = true;

        CalculateCleanliness();
    }

    private void UpdatePetTool()
    {
        Ray ray = _pixelCamRay.GetRay(Input.mousePosition);
        RaycastHit hit;
        bool result = Physics.Raycast(ray, out hit);
        UpdateVisualToolLocation(_washToolList[(int)_currentTool], hit);

        if (!result) { return; }
        if (!_isMouseDown) { return; }
        if (_mouseDelta.magnitude < 0.1f) { return; }
        Vector3 cell = _camera.WorldToViewportPoint(hit.point);
        cell.x = cell.x * _gridWidth;
        cell.y = cell.y * _gridHeight;

        var c = grid[(int)cell.x][(int)cell.y];
        if (c.shouldIgnore) { return; }

        _petPercentage += _pettingSpeed * Time.deltaTime * c.petMultiplier;
        _petPercentage = Mathf.Clamp(_petPercentage, 0.0f, 1.0f);
    }

    Vector3 CellToViewport(Vector2 cell)
    {
        return new Vector3(cell.x / _gridWidth, cell.y / _gridHeight, _initalRayCamera.nearClipPlane);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        for (int x = 0; x < grid.Count; x++)
        {
            for (int y = 0; y < grid[x].Count; y++)
            {
                if (grid[x][y].dirtiness >= 1.0f || grid[x][y].shouldIgnore) { continue; }
                Color c = Color.Lerp(Color.red, Color.green, grid[x][y].dirtiness);
                Gizmos.color = c;

                Vector3 cell = _camera.ViewportToWorldPoint(CellToViewport(new(x, y)));
                Gizmos.DrawWireCube(cell, new Vector3(1.0f / _gridWidth, 1.0f / _gridHeight, 0));
            }
        }
    }
}
