using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TarotGameBehaviour : MonoBehaviour
{
    [SerializeField] private GameObject _backButton;

    private PixelCamRaycast _pixelCamRaycast;
    private TarotCardBehaviour _hoveredCard;
    private PlayerInputActions _inputActions;
    private RaycastHit _hit;
    private TarotDeckBehaviour _deck;

    private int _flippedCards = 0;

    // Start is called before the first frame update
    void Start()
    {
        _pixelCamRaycast = FindObjectOfType<PixelCamRaycast>();
        _deck = FindObjectOfType<TarotDeckBehaviour>();

        _inputActions = new();
        _inputActions.Enable();

        _inputActions.MiniGameControls.Enable();
        _inputActions.MiniGameControls.MouseDown.performed += _ => OnMouseClick();

        _backButton.SetActive(false);
    }



    private void OnDestroy()
    {
        _inputActions.MiniGameControls.MouseDown.performed -= _ => OnMouseClick();

        _inputActions.MiniGameControls.Disable();
        _inputActions.Disable();
    }

    private void SetHovered(bool aValue)
    {
        if (!_hoveredCard) { return; }
        _hoveredCard.SetHovered(aValue);
    }

    private void Update()
    {
        Ray ray = _pixelCamRaycast.GetRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out _hit))
        {
            SetHovered(false);
            _hoveredCard = null;
            return;
        }

        if (_hit.collider.gameObject.CompareTag("TarotDeck")) { return; }

        TarotCardBehaviour tarotCard = _hit.collider.gameObject.GetComponent<TarotCardBehaviour>();

        if (_hoveredCard != tarotCard)
        {
            SetHovered(false);
        }

        _hoveredCard = tarotCard;
        SetHovered(true);
    }

    private void OnMouseClick()
    {
        if (_hit.collider != null && _hit.collider.gameObject.CompareTag("TarotDeck"))
        {
            _hit.collider.gameObject.GetComponent<TarotDeckBehaviour>().OnClick();
            return;
        }

        if (!_hoveredCard) { return; }
        _hoveredCard.Select();

        _flippedCards++;
        if (_flippedCards < _deck.TarotCardsToSpawn) { return; }

        PersistentShopData.Instance.shopManagerState.tarotPrice = 40; // TODO
        _backButton.SetActive(true);
    }
}
