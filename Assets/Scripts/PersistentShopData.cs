using System;
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


public class Unlockable
{
    public bool isUnlocked = false;
    public bool unlimitedPurchases = false;
    public int unlockCost = 0;
    public string name = "Goober Name";
}

public class GooberData : Unlockable
{
    public float cleanlinessPercentage = 0;
    public float petPercentage = 0;
    public bool isClaimed = false;
    public List<GooberState> activeStates = new();

    public float HappinessPercentage => (petPercentage + cleanlinessPercentage) / 2.0f;
}

public enum PotionIngredientType
{
    Lavender,
    Crystals,
    Peppermint,
    Worms,
    Gnomecap,
    Toadslime
}

public class PotionIngredient : Unlockable
{
    public int amount = 0;
    public PotionIngredientType type;

    public PotionIngredient()
    {
        unlimitedPurchases = true;
    }
}

public enum PotionType
{
    Sleep,
    Health,
    Love
}

public class Potion
{
    public int amount = 0;
    public int sellPrice = 10;
    public PotionType type;
}

public class ShopResources
{
    public delegate void OnCurrencyChange(int aAmount);
    public event OnCurrencyChange OnCurrencyChangeEvent;

    public delegate void OnPotionChange(PotionType aType, int aAmount);
    public event OnPotionChange OnPotionChangeEvent;

    private int _coins = 0;
    private Potion[] potions = {
        new() { type = PotionType.Sleep, sellPrice = 40 },
        new() { type = PotionType.Health, sellPrice = 50 },
        new() { type = PotionType.Love, sellPrice = 60 }
    };

    public PotionIngredient[] ingredients = {
        new() { unlockCost = 1, type = PotionIngredientType.Lavender, name = "Lavender" },
        new() { unlockCost = 2, type = PotionIngredientType.Crystals, name = "Crystals" },
        new() { unlockCost = 3, type = PotionIngredientType.Peppermint, name = "Peppermint" },
        new() { unlockCost = 4, type = PotionIngredientType.Worms, name = "Worms" },
        new() { unlockCost = 5, type = PotionIngredientType.Gnomecap, name = "Gnomecap" },
        new() { unlockCost = 6, type = PotionIngredientType.Toadslime, name = "Toadslime" }
    };

    public List<GooberData> goobers = new() {
        new() { name = "Bunny", unlockCost = 250 },
        new() { name = "Pants!!!", unlockCost = 5000 },
        new() { name = "Sheep", unlockCost = 1000 }
    };

    public List<Unlockable> outfits = new() { new() { unlockCost = 3000, name = "Arcana" }, new() { unlockCost = 1000, name = "Astro" } };

    public int CoinAmount => _coins;

    public int GetPotionSellPrice(PotionType aType)
    {
        return GetPotion(aType).sellPrice;
    }

    public void AddPotion(PotionType aType, int aAmount = 1)
    {
        GetPotion(aType).amount += aAmount;
        OnPotionChangeEvent?.Invoke(aType, aAmount);
    }
    public int GetPotionAmount(PotionType aType)
    {
        return GetPotion(aType).amount;
    }

    public bool RemovePotion(PotionType aType, int aAmount = 1)
    {
        Potion potion = GetPotion(aType);
        if (potion.amount < aAmount) { return false; }

        potion.amount -= aAmount;
        OnPotionChangeEvent?.Invoke(aType, potion.amount);

        return true;
    }

    public bool DoesPotionExist(PotionType aType)
    {
        return GetPotion(aType).amount > 0;
    }

    private Potion GetPotion(PotionType aType)
    {
        return potions[(int)aType];
    }

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
        if (!Purchase(goobers[aIndex].unlockCost)) { return; }

        goobers[aIndex].isUnlocked = true;
    }

    public void UnlockOutfit(int aIndex)
    {
        if (!Purchase(outfits[aIndex].unlockCost)) { return; }

        outfits[aIndex].isUnlocked = true;
    }

    public void PurchaseIngredient(int aIndex)
    {
        if (!Purchase(ingredients[aIndex].unlockCost)) { return; }

        ingredients[aIndex].amount++;
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
