using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class PlayerInteractable : MonoBehaviour
{
    [SerializeField] private PlayerInteractionType _playerInteractionType;

    public PlayerInteractionType PlayerInteractionType => _playerInteractionType;
    private ShopManager _shopManager;
    
    private void Awake()
    {
        _shopManager = FindObjectOfType<ShopManager>();
    }

    public void OnInteract()
    {
        _shopManager.OnPlayerInteract(_playerInteractionType);
    }
    public void OnDeny()
    {
        _shopManager.OnPlayerDeny(_playerInteractionType);
    }
}