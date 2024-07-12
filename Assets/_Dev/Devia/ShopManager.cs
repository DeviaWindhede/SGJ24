using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ShopLocations
{
    public Transform Entrance;
    public Transform Register;

    public Transform Potions;
    public Transform TarotReading;
    public Transform GooberAdoption;
    public Transform Enchanting;
}

public enum ShopLocationType
{
    Entrance,
    Register,
    Potions,
    TarotReading,
    GooberAdoption,
    Enchanting
}

public class ShopManager : MonoBehaviour
{
    [SerializeField] private GameObject _shopperPrefab;
    [SerializeField] private ShopLocations _shopLocations;
    [SerializeField] private float _spawnMaxInterval = 5.0f;
    [SerializeField] private int _maxShoppers = 5;

    private List<GameObject> _activeShoppers = new();
    private float _spawnTimer = 0.0f;
    private float _spawnInterval = 5.0f;

    // Start is called before the first frame update
    void Start()
    {
        _spawnInterval = _spawnMaxInterval;
    }

    public Transform GetShopLocation(ShopLocationType aType)
    {
        switch (aType)
        {
            case ShopLocationType.Entrance:
                return _shopLocations.Entrance;
            case ShopLocationType.Register:
                return _shopLocations.Register;
            case ShopLocationType.Potions:
                return _shopLocations.Potions;
            case ShopLocationType.TarotReading:
                return _shopLocations.TarotReading;
            case ShopLocationType.GooberAdoption:
                return _shopLocations.GooberAdoption;
            case ShopLocationType.Enchanting:
                return _shopLocations.Enchanting;
            default:
                return _shopLocations.Entrance;
        }
    }
    public ShopLocationType GetRandomType(int aOffset = 0)
    {
        return (ShopLocationType)UnityEngine.Random.Range(aOffset, Enum.GetValues(typeof(ShopLocationType)).Length);
    }

    public void DeleteShopper(GameObject aShopper)
    {
        _activeShoppers.Remove(aShopper);
        Destroy(aShopper);
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

    void SpawnShopper()
    {
        var shopper = Instantiate(
            _shopperPrefab,
            _shopLocations.Entrance.transform.position,
            Quaternion.identity
        );
        var shopperBehaviour = shopper.GetComponent<ShopperBehaviour>();
        shopperBehaviour.Init(this);
        shopperBehaviour.SetDestination(GetRandomType(2));
        _activeShoppers.Add(shopper);
    }
}
