using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ShopperIconType
{
    Potions,
    TarotReading,
    GooberAdoption,
    Enchanting,
    Waiting,
    Paying,
    None
}

public class ShopperWorldCanvas : MonoBehaviour
{
    [SerializeField] private List<Sprite> _speechBubbleIcons;
    [SerializeField] private List<Sprite> _potionIcons;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private GameObject _speechBubble;
    [SerializeField] private Image _iconImage;
    private Camera _mainCamera;
    private Animator _animator;
    private bool _isEnabled;

    public void SetSpeechBubblePotionIcon(PotionType aType)
    {
        SetSpeechBubbleInternal(ShopperIconType.Potions, aType);
    }

    public void SetSpeechBubbleIcon(ShopperIconType aType)
    {
        SetSpeechBubbleInternal(aType, PotionType.Sleep);
    }

    private void SetSpeechBubbleInternal(ShopperIconType aType, PotionType aPotionType)
    {
        bool enabled = aType != ShopperIconType.None;
        if (_isEnabled == enabled) { return; }

        _isEnabled = enabled;
        StartCoroutine(UpdateSpeechBubble(aType, aPotionType));
    }

    private IEnumerator UpdateSpeechBubble(ShopperIconType aType, PotionType aPotionType)
    {
        bool isEnabled = aType != ShopperIconType.None;
        if (isEnabled)
        {
            if (aType == ShopperIconType.Potions)
            {
                _iconImage.sprite = _potionIcons[(int)aPotionType];
            }
            else
            {
                _iconImage.sprite = _speechBubbleIcons[(int)aType];
            }
            _animator.SetTrigger("Show");
        }
        else
        {
            _animator.SetTrigger("Hide");
        }

        _speechBubble.SetActive(true);
        while (true)
        {
            if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Idle")) { break; }

            yield return null;
        }
        _speechBubble.SetActive(isEnabled);
    }

    private void Awake()
    {
        _mainCamera = FindObjectOfType<PixelCamRaycast>().GetComponent<Camera>();
        _speechBubble.SetActive(false);

        _animator = _speechBubble.GetComponent<Animator>();
    }
}
