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
    None
}

public class ShopperWorldCanvas : MonoBehaviour
{
    [SerializeField] private List<Sprite> _speechBubbleIcons;

    [SerializeField] private Canvas _canvas;
    [SerializeField] private GameObject _speechBubble;
    [SerializeField] private Image _iconImage;
    private Camera _mainCamera;
    private Animator _animator;
    private bool _isEnabled;

    public void SetSpeechBubbleIcon(ShopperIconType aType)
    {
        bool enabled = aType != ShopperIconType.None;
        if (_isEnabled == enabled) { return; }

        _isEnabled = enabled;
        StartCoroutine(UpdateSpeechBubble(aType));
    }

    private IEnumerator UpdateSpeechBubble(ShopperIconType aType)
    {
        bool isEnabled = aType != ShopperIconType.None;
        if (isEnabled)
        {
            _iconImage.sprite = _speechBubbleIcons[(int)aType];
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


    public void RotateTowardsCamera()
    {
        _canvas.transform.rotation = _mainCamera.transform.rotation;
    }


    // Update is called once per frame
    void Update()
    {
        RotateTowardsCamera();
    }
}
