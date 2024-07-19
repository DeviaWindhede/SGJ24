using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShopUIManager : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI _timeText;
    [SerializeField] private TMPro.TextMeshProUGUI _shopStatusText;
    [SerializeField] private Button _newDayButton;

    private void Start()
    {
        OnTimeFreezeEvent(PersistentShopData.Instance.shopTime.IsFrozen);
        OnTimeChangeEvent(PersistentShopData.Instance.shopTime.IsNight);

        PersistentShopData.Instance.shopTime.OnTimeChangeEvent += OnTimeChangeEvent;
        PersistentShopData.Instance.shopTime.OnTimeFreezeEvent += OnTimeFreezeEvent;

        _newDayButton.onClick.AddListener(OnNewDayButton);
        _newDayButton.gameObject.SetActive(PersistentShopData.Instance.shopTime.IsNight);
    }

    private void OnNewDayButton()
    {
        PersistentShopData.Instance.shopTime.ResetDay();
        FindObjectOfType<PlayerBehaviour>().ResetPosition();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        _newDayButton.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        PersistentShopData.Instance.shopTime.OnTimeChangeEvent -= OnTimeChangeEvent;
        PersistentShopData.Instance.shopTime.OnTimeFreezeEvent -= OnTimeFreezeEvent;
        _newDayButton.onClick.RemoveListener(OnNewDayButton);
    }

    private void OnTimeFreezeEvent(bool aIsFrozen)
    {
        _timeText.color = aIsFrozen ? Color.grey : Color.white;
    }

    private void OnTimeChangeEvent(bool aIsNight)
    {
        _timeText.text = PersistentShopData.Instance.shopTime.Time.ToString() + ":00";
        if (aIsNight)
        {
            _newDayButton.gameObject.SetActive(true);
        }
    }

    public void SetOpenStatus(bool aIsOpen)
    {
        if (aIsOpen)
        {
            _shopStatusText.text = "Open";
        }
        else
        {
            _shopStatusText.text = "Closed";
        }
    }
}
