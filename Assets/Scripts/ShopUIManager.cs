using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShopUIManager : MonoBehaviour
{
    [SerializeField] private Cinemachine.CinemachineVirtualCamera _shopCamera;
    [SerializeField] private Cinemachine.CinemachineVirtualCamera _computerCamera;
    [SerializeField] private GameObject _shopUI;
    [SerializeField] private GameObject _computerUI;
    [SerializeField] private ShopNextDayButton _newDayButton;
    [SerializeField] private TMPro.TextMeshProUGUI _timeText;
    [SerializeField] private TMPro.TextMeshProUGUI _sleepPotionText;
    [SerializeField] private TMPro.TextMeshProUGUI _healthPotionText;
    [SerializeField] private TMPro.TextMeshProUGUI _lovePotionText;

    public ShopNextDayButton ShopNextDayButton => _newDayButton;

    private void Awake()
    {
        OnTimeFreezeEvent(PersistentShopData.Instance.shopTime.IsFrozen);
        OnTimeChangeEvent(PersistentShopData.Instance.shopTime.IsNight);

        for (int i = 0; i < Enum.GetValues(typeof(PotionType)).Length; i++)
        {
            PotionType type = (PotionType)i;
            OnPotionChangeEvent(type, PersistentShopData.Instance.shopResources.GetPotionAmount(type));
        }

        PersistentShopData.Instance.shopTime.OnTimeChangeEvent += OnTimeChangeEvent;
        PersistentShopData.Instance.shopTime.OnTimeFreezeEvent += OnTimeFreezeEvent;
        PersistentShopData.Instance.shopResources.OnPotionChangeEvent += OnPotionChangeEvent;

        _shopCamera.gameObject.SetActive(true);
        _computerCamera.gameObject.SetActive(false);
    }

    private void OnPotionChangeEvent(PotionType aType, int aAmount)
    {
        print("Potion Change Event");
        switch (aType)
        {
            case PotionType.Sleep:
                _sleepPotionText.text = aAmount.ToString();
                break;
            case PotionType.Health:
                _healthPotionText.text = aAmount.ToString();
                break;
            case PotionType.Love:
                _lovePotionText.text = aAmount.ToString();
                break;
        }
    }

    public void ShouldShowComputerUI(bool aValue)
    {
        if (aValue)
        {
            _shopUI.SetActive(false);
            _computerUI.SetActive(true);
            _shopCamera.gameObject.SetActive(false);
            _computerCamera.gameObject.SetActive(true);
        }
        else
        {
            _shopUI.SetActive(true);
            _computerUI.SetActive(false);
            _shopCamera.gameObject.SetActive(true);
            _computerCamera.gameObject.SetActive(false);
        }

        FindObjectOfType<PlayerBehaviour>().ShouldEnableMovement(!aValue);
    }


    private void OnDestroy()
    {
        PersistentShopData.Instance.shopTime.OnTimeChangeEvent -= OnTimeChangeEvent;
        PersistentShopData.Instance.shopTime.OnTimeFreezeEvent -= OnTimeFreezeEvent;
        PersistentShopData.Instance.shopResources.OnPotionChangeEvent -= OnPotionChangeEvent;
    }

    private void OnTimeFreezeEvent(bool aIsFrozen)
    {
        _timeText.color = aIsFrozen ? new Color(128 / 255.0f, 125 / 255.0f, 127 / 255.0f) : new Color(61 / 255.0f, 38 / 255.0f, 53 / 255.0f);
    }

    private void OnTimeChangeEvent(bool aIsNight)
    {
        _timeText.text = PersistentShopData.Instance.shopTime.Time.ToString() + ":00";
    }
}
