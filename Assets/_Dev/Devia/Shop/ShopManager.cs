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
}

public class ShopManager : MonoBehaviour
{
    [SerializeField] private ShopQueue _queue;
    [SerializeField] private GameObject _shopperPrefab;
    [SerializeField] private ShopLocations _shopLocations;
    [SerializeField] private float _spawnMaxInterval = 5.0f;
    [SerializeField] private int _maxShoppers = 4;

    private float _spawnTimer = 0.0f;
    private float _spawnInterval = 5.0f;
    private List<GameObject> _activeShoppers = new();

    public ShopQueue Queue => _queue;

    // Start is called before the first frame update
    void Start()
    {
        _spawnInterval = _spawnMaxInterval;
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

    private void SpawnShopper()
    {
        var shopper = Instantiate(
            _shopperPrefab,
            _shopLocations.Entrance.transform.position,
            Quaternion.LookRotation(-_queue.QueueDirection)
        );
        var shopperBehaviour = shopper.GetComponent<ShopperBehaviour>();
        shopperBehaviour.Init(this);
        shopperBehaviour.SetDestination(GetRandomType());
        _activeShoppers.Add(shopper);

        AudioManager.Instance.PlaySound(ShopSoundByte.ShopBell);
    }

    public void DestroyShopper(GameObject aShopper)
    {
        _activeShoppers.Remove(aShopper);
        Destroy(aShopper);
    }

    public void OnPlayerInteract(PlayerInteractionType aType)
    {
        switch (aType)
        {
            case PlayerInteractionType.Entrance:
                break;
            case PlayerInteractionType.Register:
                var firstShopper = _queue.GetFirstShopper();
                if (firstShopper == null) { return; }
                firstShopper.Interact();
                break;
            case PlayerInteractionType.Potions:
                break;
            case PlayerInteractionType.TarotReading:
                break;
            case PlayerInteractionType.GooberCare:
                break;
            case PlayerInteractionType.Enchanting:
                break;
            default:
                break;
        }
    }

    void Update()
    {
        if (_activeShoppers.Count >= _maxShoppers) { return; }

        _spawnTimer += Time.deltaTime;
        if (_spawnTimer >= _spawnInterval)
        {
            _spawnTimer = 0.0f;
            _spawnInterval = UnityEngine.Random.Range(1.0f, _spawnMaxInterval);
            SpawnShopper();
        }
    }

    private void OnDrawGizmosSelected()
    {
        _queue.DrawGizmos(_maxShoppers);
    }
}
