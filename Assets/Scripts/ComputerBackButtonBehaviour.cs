using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputerBackButtonBehaviour : MonoBehaviour
{
    [SerializeField] private GameObject _button;
    private ComputerUIBehaviour _computerUIBehaviour;

    void Awake()
    {
        _computerUIBehaviour = FindObjectOfType<ComputerUIBehaviour>();

        _button.SetActive(false);
        _computerUIBehaviour.onComputerPageChanged += OnComputerPageChanged;
    }

    private void OnDestroy()
    {
        _computerUIBehaviour.onComputerPageChanged -= OnComputerPageChanged;
    }

    private void OnComputerPageChanged(ComputerPageType aType)
    {
        if (aType == ComputerPageType.MainPage)
        {
            _button.SetActive(false);
            return;
        }

        _button.SetActive(true);
    }
}
