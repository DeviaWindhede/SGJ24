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
    [SerializeField] private TMPro.TextMeshProUGUI _timeText;
    [SerializeField] private ShopNextDayButton _newDayButton;

    private void Awake()
    {
        OnTimeFreezeEvent(PersistentShopData.Instance.shopTime.IsFrozen);
        OnTimeChangeEvent(PersistentShopData.Instance.shopTime.IsNight);

        PersistentShopData.Instance.shopTime.OnTimeChangeEvent += OnTimeChangeEvent;
        PersistentShopData.Instance.shopTime.OnTimeFreezeEvent += OnTimeFreezeEvent;

        _shopCamera.gameObject.SetActive(true);
        _computerCamera.gameObject.SetActive(false);
    }

    public void ShowNewDayButton()
    {
        _newDayButton.SetVisibility(PersistentShopData.Instance.shopTime.IsNight);
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
    }

    private void OnTimeFreezeEvent(bool aIsFrozen)
    {
        _timeText.color = aIsFrozen ? Color.grey : Color.white;
    }

    private void OnTimeChangeEvent(bool aIsNight)
    {
        _timeText.text = PersistentShopData.Instance.shopTime.Time.ToString() + ":00";
    }
}
