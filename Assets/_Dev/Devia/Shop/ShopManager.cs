using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    CharacterCustomization
}

public struct ShopManagerState
{
    public bool isTarotActive;
}

public class ShopManager : MonoBehaviour
{
    [SerializeField] private SceneNames _sceneNames;
    [SerializeField] private ShopQueue _queue;
    [SerializeField] private GameObject _shopperPrefab;
    [SerializeField] private ShopLocations _shopLocations;
    [SerializeField] private float _spawnMaxInterval = 5.0f;
    [SerializeField] private int _maxShoppers = 4;

    private ShopManagerState _state;
    private float _spawnTimer = 0.0f;
    private float _spawnInterval = 5.0f;
    private List<ShopperBehaviour> _activeShoppers = new();
    private string _sceneToLoad = "";

    public ShopQueue Queue => _queue;

    // Start is called before the first frame update
    void Start()
    {
        _spawnInterval = _spawnMaxInterval;

        _state = PersistentShopData.Instance.shopManagerState;

        foreach (var t in PersistentShopData.Instance.shopperData)
        {
            SpawnShopper(t.state.currentStateType, false);

            var shopper = _activeShoppers[^1];
            shopper.transform.SetPositionAndRotation(
                t.transformData.position,
                t.transformData.rotation
            );

            var shopperBehaviour = shopper.GetComponent<ShopperBehaviour>();
            shopperBehaviour.state = t.state;
            shopperBehaviour.SetCurrentDestination(t.state.currentDestination);

            if (shopperBehaviour.state.queueIndex >= 0)
            {
                _queue.AddShopper(shopperBehaviour, shopperBehaviour.state.queueIndex);
            }
        }
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
        var shopper = Instantiate(
            _shopperPrefab,
            _shopLocations.Entrance.transform.position,
            Quaternion.LookRotation(-_queue.QueueDirection)
        );
        var shopperBehaviour = shopper.GetComponent<ShopperBehaviour>();
        shopperBehaviour.Init(this, aType);
        shopperBehaviour.SetDestination(GetRandomType());
        _activeShoppers.Add(shopperBehaviour);

        if (!aShouldPlaySound) { return; }

        AudioManager.Instance.PlaySound(ShopSoundByte.ShopBell);
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

    public void OnPlayerInteract(PlayerInteractionType aType)
    {
        switch (aType)
        {
            case PlayerInteractionType.Entrance:
                break;
            case PlayerInteractionType.Register:
                if (_state.isTarotActive) { break; }

                var firstShopper = _queue.GetFirstShopper();
                if (firstShopper == null) { return; }
                firstShopper.Interact();
                _state.isTarotActive = firstShopper.state.currentShopDestination == ShopLocationType.TarotReading;
                break;
            case PlayerInteractionType.Potions:
                ChangeScene(aType);
                break;
            case PlayerInteractionType.TarotReading:
                if (!_state.isTarotActive) { break; }

                _state.isTarotActive = false;
                ChangeScene(aType);
                break;
            case PlayerInteractionType.GooberCare:
                ChangeScene(aType);
                break;
            case PlayerInteractionType.Enchanting:
                ChangeScene(aType);
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
    }

    private void HandleSpawnShoppers()
    {
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

    private void OnDrawGizmosSelected()
    {
        _queue.DrawGizmos(_maxShoppers);
    }
}
