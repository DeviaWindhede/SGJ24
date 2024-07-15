using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public struct ShopperData
{
    public Transform SpawnPoint;
}

enum ShopperState
{
    Deciding,
    Purchasing,
    Leaving
}

public class ShopperBehaviour : MonoBehaviour
{
    [SerializeField] private ShopperData _shopperData;

    [SerializeField] private NavMeshAgent _agent;

    private Transform _currentDestination;
    private ShopManager _shopManager;
    private ShopperState _state;
    private ShopLocationType _currentShopDestination;
    private bool _hasReachedDestination;

    private float _decideTimer = 0.0f;
    private float _decideMaxInterval = 5.0f;
    private float _decideInterval = 5.0f;

    public void Init(ShopManager aShopManager)
    {
        _agent = GetComponent<NavMeshAgent>();
        _shopManager = aShopManager;
    }

    public void SetDestination(ShopLocationType aType)
    {
        if (aType == _currentShopDestination) { return; }

        // TODO: Kolla occupied slots
        _currentDestination = _shopManager.GetShopLocation(aType);
        _agent.SetDestination(_currentDestination.position);
        _currentShopDestination = aType;
        _hasReachedDestination = false;

        if (aType == ShopLocationType.Entrance)
        {
            _state = ShopperState.Leaving;
            return;
        }

        if (aType == ShopLocationType.Register)
        {
           _state = ShopperState.Purchasing;
            return;
        }

        _state = ShopperState.Deciding;
    }

    // Update is called once per frame
    void Update()
    {
        switch (_state)
        {
            case ShopperState.Deciding:
                Deciding();
                break;
            case ShopperState.Purchasing:
                Purchasing();
                break;
            case ShopperState.Leaving:
                FinishShopping();
                break;
            default:
                break;
        }

        var delta = _currentDestination.transform.position - transform.position;
        _hasReachedDestination = delta.magnitude < 1.0f;
    }


    private void Deciding()
    {
        if (_decideTimer <= 0)
        {
            _decideInterval = UnityEngine.Random.Range(1.0f, _decideMaxInterval);
            _decideTimer = 0;
        }

        _decideTimer += Time.deltaTime;
        if (_decideTimer >= _decideInterval)
        {
            _decideTimer = 0;

            ShopLocationType type = _shopManager.GetRandomType();
            SetDestination(type);
        }
    }

    private void Purchasing()
    {
        SetDestination(ShopLocationType.Register);

        if (!_hasReachedDestination) { return; }

        _state = ShopperState.Leaving;
    }

    private void FinishShopping()
    {
        SetDestination(ShopLocationType.Entrance);

        if (!_hasReachedDestination) { return; }

        _shopManager.DeleteShopper(gameObject);
    }
}
