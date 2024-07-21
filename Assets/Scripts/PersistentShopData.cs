using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

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

public struct PotionIngredients
{
    public int ingredient1;
    public int ingredient2;
}

public class GooberData
{
    public float cleanlinessPercentage = 0;
    public float petPercentage = 0;
    public bool isUnlocked = true;
    public int unlockCost = 0;
    public List<GooberState> activeStates = new();

    public float HappinessPercentage => (petPercentage + cleanlinessPercentage) / 2.0f;
}

public class ShopResources
{
    public delegate void OnCurrencyChange(int aAmount);
    public event OnCurrencyChange OnCurrencyChangeEvent;

    private int _coins;

    public PotionIngredients ingredients;
    public List<GooberData> goobers = new() { new(), new(), new() };

    public int CoinAmount => _coins;

    public bool CanAfford(int aCost)
    {
        return _coins >= aCost;
    }

    public bool Purchase(int aCost)
    {
        if (!CanAfford(aCost)) { return false; }
        _coins -= aCost;
        OnCurrencyChangeEvent?.Invoke(_coins);
        return true;
    }

    public void AddCoins(int aAmount)
    {
        _coins += aAmount;
        OnCurrencyChangeEvent?.Invoke(_coins);
    }

    public void UnlockGoober(int aIndex)
    {
        goobers[aIndex].isUnlocked = true;
    }
}

public class ShopTime
{
    public delegate void OnTimeChange(bool aIsNight);
    public event OnTimeChange OnTimeChangeEvent;
    public event OnTimeChange OnTimeFreezeEvent;

    private static readonly int DEFAULT_TIME = 8;
    private static readonly int NIGHT_TIME = 20;
    private static readonly float TIME_TO_INCREMENT_HOUR = 180.0f / (NIGHT_TIME - DEFAULT_TIME);
    //private static readonly float TIME_TO_INCREMENT_HOUR = 1.0f;

    private float _timer = 0;
    private int _hour = DEFAULT_TIME;
    private bool _isTimeFrozen = true;

    public int Time => _hour;
    public bool IsNight => _hour >= NIGHT_TIME;
    public bool IsFrozen => _isTimeFrozen;

    public void IncrementHour()
    {
        if (IsNight) { return; }

        ++_hour;
        OnTimeChangeEvent?.Invoke(IsNight);
        _isTimeFrozen = IsNight;
    }

    public void ResetDay()
    {
        _hour = DEFAULT_TIME;
        _timer = 0.0f;
        ShouldFreezeTime(true);
    }

    public void ShouldFreezeTime(bool aValue)
    {
        if (_isTimeFrozen != aValue)
        {
            OnTimeFreezeEvent?.Invoke(aValue);
        }
        _isTimeFrozen = aValue;
    }

    public void FastForwardToNight()
    {
        _hour = NIGHT_TIME;
        OnTimeChangeEvent?.Invoke(IsNight);
    }

    public void UpdateTime()
    {
        if (_isTimeFrozen) { return; }
        if (IsNight) { return; }

        _timer += UnityEngine.Time.deltaTime;
        if (_timer >= TIME_TO_INCREMENT_HOUR)
        {
            _timer = 0.0f;
            IncrementHour();
        }
    }
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
    public ShopResources shopResources = new();
    public ShopTime shopTime = new();

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }
}
