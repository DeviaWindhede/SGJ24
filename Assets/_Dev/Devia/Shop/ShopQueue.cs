using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShopQueue
{
    public Transform QueueTransform;
    [SerializeField] private float _queueSpacing = 1.0f;
    [SerializeField] private Vector3 _queueDirection = Vector3.forward;
    public Vector3 QueueDirection => _queueDirection;

    private List<ShopperBehaviour> _shopperQueue = new();

    public int Enter(ShopperBehaviour aShopper)
    {
        _shopperQueue.Add(aShopper);
        return _shopperQueue.Count - 1;
    }

    public void AddShopper(ShopperBehaviour aShopper, int aIndex)
    {
        _shopperQueue.Insert(aIndex, aShopper);
        for (int i = 0; i < _shopperQueue.Count; i++)
        {
            _shopperQueue[i].OnQueueUpdate(i);
        }
    }

    public void Leave()
    {
        _shopperQueue.RemoveAt(0);
        for (int i = 0; i < _shopperQueue.Count; i++)
        {
            _shopperQueue[i].OnQueueUpdate(i);
        }
    }

    public ShopperBehaviour GetFirstShopper()
    {
        if (_shopperQueue.Count == 0) { return null; }
        return _shopperQueue[0];
    }

    public Vector3 GetQueuePosition(int aIndex)
    {
        return QueueTransform.position + aIndex * _queueSpacing * _queueDirection;
    }

    public void DrawGizmos(int aMaxShoppers)
    {
        Gizmos.color = Color.yellow;

        Vector3 size = new Vector3(0.35f, 1.5f, 0.35f);
        for (int i = 0; i < aMaxShoppers; i++)
        {
            Gizmos.DrawWireCube(GetQueuePosition(i) + Vector3.up * size.y / 2, size);
        }
    }
}
