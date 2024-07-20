using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static UnityEditor.Experimental.GraphView.GraphView;

[System.Serializable]
public struct ShopInteractable
{
    public Transform NavAgentWindowShopLocation;
    public Transform NavAgentInteractLocation;
}

[System.Serializable]
public struct ShopLocations
{
    public Transform Entrance;

    public ShopInteractable Potions;
    public ShopInteractable TarotReading;
    public ShopInteractable GooberAdoption;
    public ShopInteractable Enchanting;

    public Transform TarotShopperSeat;
}

[System.Serializable]
public class SceneNames
{
    public string Shop = "ShopScene";
    public string TarotReading = "TarotReadingScene";
    public string GooberCare = "GooberCareScene";
    public string Potions = "PotionScene";
    public string CharacterCustomization = "CharacterCustomizationScene";
    public string Enchanting = "EnchantingScene";
}

public enum ShopLocationType
{
    Potions,
    TarotReading,
    GooberAdoption,
    Enchanting
}

public enum PlayerInteractionType
{
    Entrance,
    Register,
    Potions,
    TarotReading,
    GooberCare,
    Enchanting,
    CharacterCustomization,
    Computer,
    None
}

public struct ShopManagerState
{
    public bool isTarotActive;
    public bool isStandingInRegister;
}

public class ShopManager : MonoBehaviour
{
    [SerializeField] private SceneNames _sceneNames;
    [SerializeField] private ShopQueue _queue;
    [SerializeField] private GameObject _shopperPrefab;
    [SerializeField] private ShopLocations _shopLocations;
    [SerializeField] private float _spawnMaxInterval = 5.0f;
    [SerializeField] private int _maxShoppers = 4;

    private PlayerBehaviour _player;
    private ShopUIManager _uiManager;
    private ShopManagerState _state;
    private float _spawnTimer = 0.0f;
    private float _spawnInterval = 5.0f;
    private List<ShopperBehaviour> _activeShoppers = new();
    private string _sceneToLoad = "";
    private bool _shouldLetInCustomers = false;

    public ShopQueue Queue => _queue;
    public ShopLocations ShopLocations => _shopLocations;
    public bool IsStandingInRegister => _state.isStandingInRegister;

    // Start is called before the first frame update
    void Start()
    {
        _uiManager = FindObjectOfType<ShopUIManager>();
        _player = FindObjectOfType<PlayerBehaviour>();

        _spawnInterval = _spawnMaxInterval;
        _state = PersistentShopData.Instance.shopManagerState;

        foreach (var t in PersistentShopData.Instance.shopperData)
        {
            InstantiateShopper(t.state.currentStateType);

            var shopper = _activeShoppers[^1];
            shopper.transform.SetPositionAndRotation(
                t.transformData.position,
                t.transformData.rotation
            );

            var shopperBehaviour = shopper.GetComponent<ShopperBehaviour>();
            shopperBehaviour.Init(this, t.state);
            shopperBehaviour.SetCurrentDestination(t.state.currentDestination);

            if (shopperBehaviour.state.queueIndex >= 0)
            {
                _queue.InitializeShopper(shopperBehaviour, shopperBehaviour.state.queueIndex);
            }
        }

        PersistentShopData.Instance.shopTime.OnTimeChangeEvent += OnTimeChangeEvent;
        _player.OnInteractableChangedEvent += OnPlayerInteractableChangedEvent;

        _uiManager.SetOpenStatus(_shouldLetInCustomers);
    }

    private void OnPlayerInteractableChangedEvent(PlayerInteractionType aType)
    {
        _state.isStandingInRegister = aType == PlayerInteractionType.Register;
    }

    private void OnTimeChangeEvent(bool aIsNight)
    {
        if (!aIsNight) { return; }

        foreach (var shopper in _activeShoppers)
        {
            if (shopper.CurrentStateType == typeof(QueueState)) { continue; }
            shopper.ChangeState(typeof(LeavingState)); // potential bug :) (stuck on tarot)
        }
        ShouldLetInCustomers(false);
    }

    public void ShouldLetInCustomers(bool aValue)
    {
        _shouldLetInCustomers = aValue;
        AudioManager.Instance.PlaySound(ShopSoundByte.Placeholder);

        if (!_shouldLetInCustomers && !PersistentShopData.Instance.shopTime.IsNight)
        {
            PersistentShopData.Instance.shopTime.FastForwardToNight();
        }
        else
        {
            PersistentShopData.Instance.shopTime.ShouldFreezeTime(!_shouldLetInCustomers);
        }

        _uiManager.SetOpenStatus(_shouldLetInCustomers);
    }

    public Vector3 GetEntrancePosition()
    {
        return _shopLocations.Entrance.position;
    }

    public ShopInteractable GetShopPosition(ShopLocationType aType)
    {
        switch (aType)
        {
            case ShopLocationType.Potions:
                return _shopLocations.Potions;
            case ShopLocationType.TarotReading:
                return _shopLocations.TarotReading;
            case ShopLocationType.GooberAdoption:
                return _shopLocations.GooberAdoption;
            case ShopLocationType.Enchanting:
                return _shopLocations.Enchanting;
            default:
                return _shopLocations.Potions;
        }
    }

    public static ShopLocationType GetRandomType(int aOffset = 0)
    {
        return (ShopLocationType)UnityEngine.Random.Range(aOffset, Enum.GetValues(typeof(ShopLocationType)).Length);
    }

    private void SpawnShopper(System.Type aType, bool aShouldPlaySound = true)
    {
        ShopperBehaviour shopperBehaviour = InstantiateShopper(aType);
        shopperBehaviour.Init(this, new ShopperState());
        shopperBehaviour.SetDestination(GetRandomType());

        if (!aShouldPlaySound) { return; }

        AudioManager.Instance.PlaySound(ShopSoundByte.ShopBell);
    }

    private ShopperBehaviour InstantiateShopper(System.Type aType)
    {
        var shopper = Instantiate(
            _shopperPrefab,
            _shopLocations.Entrance.transform.position,
            Quaternion.LookRotation(-_queue.QueueDirection)
        );
        var shopperBehaviour = shopper.GetComponent<ShopperBehaviour>();
        _activeShoppers.Add(shopperBehaviour);

        return shopperBehaviour;
    }

    public void DestroyShopper(GameObject aShopper)
    {
        int index = -1;
        for (int i = 0; i < _activeShoppers.Count; i++)
        {
            if (_activeShoppers[i] == aShopper.GetComponent<ShopperBehaviour>())
            {
                index = i;
                break;
            }
        }
        _activeShoppers.RemoveAt(index);

        Destroy(aShopper);

    }

    public void OnPlayerDeny(PlayerInteractionType aType)
    {
        switch (aType)
        {
            case PlayerInteractionType.Register:
                var firstShopper = _queue.GetFirstShopper();
                if (firstShopper == null) { return; }

                firstShopper.LeaveQueue();
                firstShopper.ChangeState(typeof(LeavingState));
                break;
            case PlayerInteractionType.Computer:
                if (!PersistentShopData.Instance.shopTime.IsNight) { break; }
                _uiManager.ShouldShowComputerUI(false);
                break;
            default:
                break;
        }
    }

    public void OnPlayerInteract(PlayerInteractionType aType)
    {
        switch (aType)
        {
            case PlayerInteractionType.Entrance:
            {
                if (PersistentShopData.Instance.shopTime.IsNight) { break; }

                ShouldLetInCustomers(!_shouldLetInCustomers);
                break;
            }
            case PlayerInteractionType.Register:
                var firstShopper = _queue.GetFirstShopper();
                if (firstShopper == null) { return; }

                if (firstShopper.state.currentShopDestination == ShopLocationType.TarotReading &&
                    firstShopper.state.currentQueueType == ShopperQueueType.Action)
                {
                    if (!_state.isTarotActive)
                    {
                        _state.isTarotActive = true;
                        firstShopper.Interact();
                    }
                    break;
                }

                firstShopper.Interact();
                break;
            case PlayerInteractionType.Potions:
                if (!PersistentShopData.Instance.shopTime.IsNight) { break; }
                ChangeScene(aType);
                break;
            case PlayerInteractionType.TarotReading:
                if (!_state.isTarotActive) { break; }

                _state.isTarotActive = false;

                var shopper = _activeShoppers.Where(x => 
                    x.state.currentStateType == typeof(ActionState) && 
                    x.state.currentShopDestination == ShopLocationType.TarotReading
                ).FirstOrDefault();
                if (!shopper) { break; }

                shopper.Interact();
                ChangeScene(aType);
                break;
            case PlayerInteractionType.GooberCare:
                if (!PersistentShopData.Instance.shopTime.IsNight) { break; }
                ChangeScene(aType);
                break;
            case PlayerInteractionType.Enchanting:
                if (!PersistentShopData.Instance.shopTime.IsNight) { break; }
                ChangeScene(aType);
                break;
            case PlayerInteractionType.CharacterCustomization:
                ChangeScene(aType);
                break;
            case PlayerInteractionType.Computer:
                if (!PersistentShopData.Instance.shopTime.IsNight) { break; }
                _uiManager.ShouldShowComputerUI(true);
                break;
            default:
                break;
        }
    }

    void ChangeScene(PlayerInteractionType aType)
    {
        PersistentShopData.Instance.shopperData.Clear();

        foreach (var item in _activeShoppers)
        {
            var shopper = item.GetComponent<ShopperBehaviour>();

            ShopperData data = new ShopperData
            {
                transformData = new TransformData
                {
                    position = shopper.transform.position,
                    rotation = shopper.transform.rotation
                },
                state = shopper.state
            };
            data.state.currentStateType = shopper.CurrentStateType;
            PersistentShopData.Instance.shopperData.Add(data);
        }

        _sceneToLoad = "Shop";
        switch (aType)
        {
            case PlayerInteractionType.Potions:
                _sceneToLoad = _sceneNames.Potions;
                break;
            case PlayerInteractionType.TarotReading:
                _sceneToLoad = _sceneNames.TarotReading;
                break;
            case PlayerInteractionType.GooberCare:
                _sceneToLoad = _sceneNames.GooberCare;
                break;
            case PlayerInteractionType.Enchanting:
                _sceneToLoad = _sceneNames.Enchanting;
                break;
            case PlayerInteractionType.CharacterCustomization:
                _sceneToLoad = _sceneNames.CharacterCustomization;
                break;
            default:
                break;
        }
    }

    void Update()
    {
        HandleSpawnShoppers();

        for (int i = 0; i < _activeShoppers.Count; i++)
        {
            _activeShoppers[i].DoUpdate();
        }

        if (PersistentShopData.Instance.shopTime.IsNight && _activeShoppers.Count == 0)
        {
            _uiManager.ShowNewDayButton();
            PersistentShopData.Instance.shopperData.Clear();
        }

        PersistentShopData.Instance.shopTime.UpdateTime();
    }

    private void HandleSpawnShoppers()
    {
        if (!_shouldLetInCustomers) { return; }
        if (_activeShoppers.Count >= _maxShoppers) { return; }

        _spawnTimer += Time.deltaTime;
        if (_spawnTimer >= _spawnInterval)
        {
            _spawnTimer = 0.0f;
            _spawnInterval = UnityEngine.Random.Range(1.0f, _spawnMaxInterval);
            SpawnShopper(typeof(DecisionState));
        }
    }

    private void LateUpdate()
    {
        if (_sceneToLoad == "") { return; }

        UnityEngine.SceneManagement.SceneManager.LoadScene(_sceneToLoad);
    }

    private void OnDestroy()
    {
        PersistentShopData.Instance.shopTime.OnTimeChangeEvent -= OnTimeChangeEvent;
    }

    private void OnDrawGizmosSelected()
    {
        _queue.DrawGizmos(_maxShoppers);
    }
}
