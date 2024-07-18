using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TransformData
{
    public Vector3 position;
    public Quaternion rotation;
}

public struct ShopperData
{
    public TransformData transformData;
    public ShopperState state;
}

public class PersistentShopData : MonoBehaviour
{
    static PersistentShopData _instance;
    public static PersistentShopData Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<PersistentShopData>();
                if (_instance == null)
                {
                    GameObject go = new GameObject();
                    go.name = typeof(PersistentShopData).Name;
                    _instance = go.AddComponent<PersistentShopData>();
                }
            }
            return _instance;
        }
    }

    public List<ShopperData> shopperData = new();
    public TransformData playerTransform = new();
    public ShopManagerState shopManagerState = new();

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
