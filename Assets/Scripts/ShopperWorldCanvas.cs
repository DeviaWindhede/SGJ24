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

    public void SetSpeechBubbleIcon(ShopperIconType aType)
    {
        if (aType == ShopperIconType.None)
        {
            _speechBubble.SetActive(false);
            return;
        }
        _iconImage.sprite = _speechBubbleIcons[(int)aType];
        _speechBubble.SetActive(true);
    }

    private void Awake()
    {
        _mainCamera = FindObjectOfType<PixelCamRaycast>().GetComponent<Camera>();
        _speechBubble.SetActive(false);
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
