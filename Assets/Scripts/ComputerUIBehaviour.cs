using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ShopPageData
{
    public GameObject mainPage;
    public GameObject arkenGoob;
    public GameObject henrysAndMagics;
    public GameObject alchemica;
}

public enum ComputerPageType
{
    MainPage,
    ArkenGoob,
    HenrysAndMagics,
    Alchemica
}

public class ComputerUIBehaviour : MonoBehaviour
{
    public delegate void OnComputerPageChanged(ComputerPageType aType);
    public event OnComputerPageChanged onComputerPageChanged;

    [SerializeField] private ShopPageData _shopPageData;

    private ComputerPageType _currentType;

    private GameObject GetPageFromType(ComputerPageType aType)
    {
        switch (aType)
        {
            case ComputerPageType.MainPage:
                return _shopPageData.mainPage;
            case ComputerPageType.ArkenGoob:
                return _shopPageData.arkenGoob;
            case ComputerPageType.HenrysAndMagics:
                return _shopPageData.henrysAndMagics;
            case ComputerPageType.Alchemica:
                return _shopPageData.alchemica;
            default:
                return null;
        }
    }

    public void SetComputerPage(ComputerPageType aType)
    {
        GetPageFromType(_currentType).SetActive(false);
        _currentType = aType;
        GetPageFromType(_currentType).SetActive(true);

        onComputerPageChanged?.Invoke(_currentType);
    }

    public void SetComputerPage(ComputerUIEnum aEnum)
    {
       SetComputerPage(aEnum.type);
    }

    public void EnterComputer()
    {
        SetComputerPage(ComputerPageType.MainPage);
    }

    public void ExitComputer()
    {
        SetComputerPage(ComputerPageType.MainPage);
        gameObject.SetActive(false);
    }
}
